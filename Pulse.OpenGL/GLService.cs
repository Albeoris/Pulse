using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Windows.Forms.Integration;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using Pulse.Core;

namespace Pulse.OpenGL
{
    public static class GLService
    {
        private static readonly object Lock;
        private static readonly ThreadLocal<int> ContextReferenceCount;
        private static readonly ThreadLocal<IGraphicsContext> ContextReference;

        //private static readonly NativeWindow GLWindow;
        private static readonly GLControl GLControl;
        private static readonly IGraphicsContext GLGraphicsContext;
        private static readonly ConcurrentDictionary<WindowsFormsHost, SubscribeContext> SubscribeContexts;
        private static readonly ConcurrentDictionary<Action, DrawingContext> DrawingContexts;

        private static event Action InvalidateControlRequestRecived;
        private static event Action<int, int> ViewportDesiredSizeChanged;

        static GLService()
        {
            Lock = new object();
            ContextReferenceCount = new ThreadLocal<int>(() => 0, false);

            GLControl = new GLControl(GraphicsMode.Default);
            GLControl.CreateControl();
            //GLWindow = new NativeWindow();
            GLGraphicsContext = GLControl.Context;
            //GLGraphicsContext = new GraphicsContext(GraphicsMode.Default, GLControl.WindowInfo);
            SubscribeContexts = new ConcurrentDictionary<WindowsFormsHost, SubscribeContext>();
            DrawingContexts = new ConcurrentDictionary<Action, DrawingContext>();

            GLGraphicsContext.MakeCurrent(null);

            DisposableSerivce.Register(new DisposableAction(Dispose));
        }

        private static void Dispose()
        {
            GLGraphicsContext.Dispose();
            GLControl.Dispose();
        }

        public static void SubscribeControl(WindowsFormsHost glControlHost)
        {
            Exceptions.CheckArgumentNull(glControlHost, "glControlHost");

            GLControl glControl = glControlHost.Child as GLControl;
            if (glControl == null) throw new ArgumentException("glControlHost");

            SubscribeContext subscribeContext = new SubscribeContext
            {
                OnInvalidateControlRequestRecived = () => glControl.Invalidate(),
                OnViewportDesiredSizeChanged = (dw, dh) => glControlHost.Dispatcher.BeginInvoke(new Action(() =>
                {
                    glControlHost.Width = dw;
                    glControlHost.Height = dh;
                    glControl.Width = dw;
                    glControl.Height = dh;
                }))
            };
            if (!SubscribeContexts.TryAdd(glControlHost, subscribeContext))
                throw new Exception("Элемент управления уже зарегистрирован.");

            InvalidateControlRequestRecived += subscribeContext.OnInvalidateControlRequestRecived;
            ViewportDesiredSizeChanged += subscribeContext.OnViewportDesiredSizeChanged;

            glControl.Invalidate();
        }

        public static void UnsubscribeControl(WindowsFormsHost glControlHost)
        {
            Exceptions.CheckArgumentNull(glControlHost, "glControlHost");

            GLControl glControl = glControlHost.Child as GLControl;
            if (glControl == null) throw new ArgumentException("glControlHost");

            SubscribeContext subscribeContext;
            if (!SubscribeContexts.TryRemove(glControlHost, out subscribeContext))
                throw new Exception("Элемент управления не зарегистрирован.");

            ViewportDesiredSizeChanged -= subscribeContext.OnViewportDesiredSizeChanged;
            InvalidateControlRequestRecived -= subscribeContext.OnInvalidateControlRequestRecived;
        }

        public static AutoResetEvent RegisterDrawMethod(Action action)
        {
            Exceptions.CheckArgumentNull(action, "action");

            DrawingContext drawingContext = new DrawingContext(action, new Thread(DrawingThreadProc) {IsBackground = true});

            if (!DrawingContexts.TryAdd(action, drawingContext))
                throw new Exception("Метод уже зарегистрирован.");

            drawingContext.StartWorking();
            return drawingContext.DrawEvent;
        }

        public static void UnregisterDrawMethod(Action action)
        {
            Exceptions.CheckArgumentNull(action, "action");

            DrawingContext drawingContext;
            if (!DrawingContexts.TryRemove(action, out drawingContext))
                throw new Exception("Метод не зарегистрирован.");

            drawingContext.StopEvent.Set();
        }

        private static void DrawingThreadProc(object obj)
        {
            DrawingContext drawingContext = (DrawingContext)obj;
            while (true)
            {
                if (WaitHandle.WaitAny(drawingContext.WaitHandles) != 0)
                    break;

                DateTime begin = DateTime.Now;

                drawingContext.Drawer();
                InvalidateViewports();

                int span = 200 - (int)(DateTime.Now - begin).TotalMilliseconds;
                if (span > 0)
                    Thread.Sleep(span);
            }
        }

        public static void SetViewportDesiredSize(int width, int height)
        {
            Exceptions.CheckArgumentOutOfRangeException(width, "width", 0, int.MaxValue);
            Exceptions.CheckArgumentOutOfRangeException(height, "height", 0, int.MaxValue);
                
            Action<int, int> handler = ViewportDesiredSizeChanged;
            if (handler != null) handler(width, height);
        }

        public static void InvalidateViewports()
        {
            Action handler = InvalidateControlRequestRecived;
            if (handler != null) handler();
        }

        public static IDisposable AcquireContext(IWindowInfo windowInfo = null)
        {
            Monitor.Enter(Lock);
            if (1 == ++ContextReferenceCount.Value)
                GLGraphicsContext.MakeCurrent(windowInfo ?? GLControl.WindowInfo);

            return new DisposableAction(FreeContext, true);
        }

        private static void FreeContext()
        {
            try
            {
                GL.Finish();
                ErrorCode error = GL.GetError();
                if (error != ErrorCode.NoError)
                    throw Exceptions.CreateException("GLError: {0} ({1})", error, (int)error);
            }
            finally
            {
                if (0 == --ContextReferenceCount.Value)
                {
                    //GLGraphicsContext.SwapBuffers();
                    GLGraphicsContext.MakeCurrent(null);
                }

                Monitor.Exit(Lock);
            }
        }

        private sealed class SubscribeContext
        {
            public Action OnInvalidateControlRequestRecived;
            public Action<int, int> OnViewportDesiredSizeChanged;
        }

        private sealed class DrawingContext
        {
            public readonly Action Drawer;
            public readonly Thread WorkingThread;
            public readonly AutoResetEvent DrawEvent;
            public readonly ManualResetEvent StopEvent;
            public readonly WaitHandle[] WaitHandles;

            public DrawingContext(Action drawer, Thread workingThread)
            {
                Drawer = Exceptions.CheckArgumentNull(drawer, "drawer");
                WorkingThread = Exceptions.CheckArgumentNull(workingThread, "workingThread");
                DrawEvent = new AutoResetEvent(false);
                StopEvent = new ManualResetEvent(false);
                WaitHandles = new WaitHandle[] {DrawEvent, StopEvent};
            }

            public void StartWorking()
            {
                WorkingThread.Start(this);
            }
        }
    }
}