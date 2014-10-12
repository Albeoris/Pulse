using System;
using System.Windows;

namespace Pulse.UI
{
    public sealed class UiGameFileCommander : UiMainDockableControl
    {
        private readonly UiTreeView _treeView;

        public UiGameFileCommander()
        {
            #region Construct

            Header = "Ресурсы игры";
            //Width = 240;
            //Height = 300;

            _treeView = UiTreeViewFactory.Create();
            {
                Content = _treeView;
            }
            
            #endregion

            Loaded += OnLoaded;
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            InteractionService.Refreshed += Refresh;
            Refresh();
        }

        private void Refresh()
        {
            try
            {
                UiArchiveTreeViewItem rootNode = UiArchiveTreeBuilder.Build(InteractionService.GamePath);
                _treeView.Items.Add(rootNode);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public override int Index
        {
            get { return 0; }
        }
    }
}