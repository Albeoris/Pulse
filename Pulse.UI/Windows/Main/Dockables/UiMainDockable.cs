using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;

namespace Pulse.UI
{
    public abstract class UiMainDockableControl : UserControl
    {
        protected abstract int Index { get; }
        protected string Header { get; set; }
        private DockingManager DockingManager { get; set; }
        internal LayoutAnchorable LayoutAnchorable { get; set; }
        private readonly object _lock = new object();

        public static UiMainDockableControl[] CreateKnownDockables(DockingManager dockingManager)
        {
            Type currentType = typeof(UiMainDockableControl);
            Assembly currentAssymbly = Assembly.GetExecutingAssembly();

            Type[] types = currentAssymbly.GetTypes();
            SortedList<int, UiMainDockableControl> list = new SortedList<int, UiMainDockableControl>();
            foreach (Type type in types)
            {
                if (!type.IsSubclassOf(currentType))
                    continue;

                UiMainDockableControl dockableControl = (UiMainDockableControl)Activator.CreateInstance(type);
                dockableControl.DockingManager = dockingManager;
                list.Add(dockableControl.Index, dockableControl);
            }
            return list.Values.ToArray();
        }

        public UiMenuItem CreateMenuItem()
        {
            return UiMenuItemFactory.Create(Header, MenuCommand.Instance, this);
        }

        private sealed class MenuCommand : ICommand
        {
            public static readonly MenuCommand Instance = new MenuCommand();

            private MenuCommand()
            {
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                UiMainDockableControl window = (UiMainDockableControl)parameter;
                window.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                window.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                lock (window._lock)
                {
                    LayoutAnchorable layout = window.LayoutAnchorable ?? (window.LayoutAnchorable = window.DockingManager.Layout.Descendents().OfType<LayoutAnchorable>().FirstOrDefault(l => l.Title == window.Header));
                    if (layout == null)
                    {
                        layout = new LayoutAnchorable
                        {
                            ContentId = window.Header,
                            Title = window.Header,
                            FloatingWidth = window.Width,
                            FloatingHeight = window.Height,
                            FloatingLeft = 200,
                            FloatingTop = 200,
                            Content = window
                        };

                        window.LayoutAnchorable = layout;
                        layout.AddToLayout(window.DockingManager, AnchorableShowStrategy.Most);
                        layout.Float();
                    }
                    else
                    {
                        if (layout.IsHidden)
                            layout.Show();
                        else
                            layout.Hide();
                    }
                }
            }

            public event EventHandler CanExecuteChanged;
        }
    }
}