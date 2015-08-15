using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Pulse.UI.Interaction;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace Pulse.UI
{
    public sealed class UiMainWindow : UiWindow
    {
        private readonly XmlLayoutSerializer _layoutSerializer;
        private readonly UiMenu _mainMenu;
        private readonly UiMenuItem _mainMenuView;

        public UiMainWindow()
        {
            #region Construct

            Assembly assembly = Assembly.GetEntryAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            Title = $"{fvi.ProductName} {fvi.FileVersion} {fvi.LegalCopyright}";

            Width = 640;
            Height = 480;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowState = WindowState.Maximized;

            UiGrid root = UiGridFactory.Create(2, 1);
            root.RowDefinitions[0].Height = GridLength.Auto;

            DockingManager dockingManager = new DockingManager();
            {
                root.AddUiElement(dockingManager, 1, 0);
                _layoutSerializer = new XmlLayoutSerializer(dockingManager);
                _layoutSerializer.LayoutSerializationCallback += OnLayoutDeserialized;
            }

            _mainMenu = UiMenuFactory.Create();
            {
                _mainMenuView = _mainMenu.AddChild(UiMenuItemFactory.Create("Вид"));
                {
                    foreach (UiMainDockableControl dockable in UiMainDockableControl.CreateKnownDockables(dockingManager))
                        _mainMenuView.AddChild(dockable.CreateMenuItem());
                }

                root.AddUiElement(_mainMenu, 0, 0);
            }

            Content = root;

            #endregion

            Loaded += OnLoaded;
            Closing += OnClosing;
        }

        private void OnLayoutDeserialized(object sender, LayoutSerializationCallbackEventArgs e)
        {
            foreach (UiMenuItem item in _mainMenuView.Items)
            {
                if (e.Model.Title != (string)item.Header)
                    continue;

                e.Content = item.CommandParameter;
                //e.Model.Content = item.CommandParameter;
                e.Cancel = false;
                return;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!File.Exists(ApplicationConfigInfo.LayoutConfigurationFilePath))
                    return;

                _layoutSerializer.Deserialize(ApplicationConfigInfo.LayoutConfigurationFilePath);
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(this, ex);
            }
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            try
            {
                Directory.CreateDirectory(ApplicationConfigInfo.ConfigurationDirectory);
                _layoutSerializer.Serialize(ApplicationConfigInfo.LayoutConfigurationFilePath);
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(this, ex);
            }
        }
    }
}