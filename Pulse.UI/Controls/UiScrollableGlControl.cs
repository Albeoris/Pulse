using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Pulse.Core;
using Size = System.Windows.Size;

namespace Pulse.UI.Controls
{
    public sealed class UiScrollableGlControl : UiGlControl
    {
        private readonly Lazy<Window> _windowInstance;

        public UiScrollableGlControl()
        {
            _windowInstance = new Lazy<Window>(() => Window.GetWindow(this));
        }

        private ScrollViewer ParentScrollViewer { get; set; }

        protected override void OnWindowPositionChanged(Rect rcBoundingBox)
        {
            base.OnWindowPositionChanged(rcBoundingBox);

            if (ParentScrollViewer == null)
                return;

            GeneralTransform tr = ParentScrollViewer.TransformToAncestor(_windowInstance.Value);
            Rect scrollRect = new Rect(new Size(ParentScrollViewer.ViewportWidth, ParentScrollViewer.ViewportHeight));
            scrollRect = tr.TransformBounds(scrollRect);

            Rect intersect = Rect.Intersect(scrollRect, rcBoundingBox);
            if (!intersect.IsEmpty)
            {
                tr = _windowInstance.Value.TransformToDescendant(this);
                intersect = tr.TransformBounds(intersect);
            }

            SetRegion(intersect);
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            ParentScrollViewer = this.GetParentElement<ScrollViewer>();
        }

        private void SetRegion(Rect intersect)
        {
            using (Graphics graphics = Graphics.FromHwnd(Handle))
                NativeMethods.SetWindowRgn(Handle, (new Region(ConvertRect(intersect))).GetHrgn(graphics), true);
        }

        private static RectangleF ConvertRect(Rect r)
        {
            return new RectangleF((float)r.X, (float)r.Y, (float)r.Width, (float)r.Height);
        }
    }
}