using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using NAudio.Wave;
using Pulse.Core;
using Pulse.UI;
using Shazzam.Shaders;

namespace Pulse.Patcher
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly DisposableStack _disposables = new DisposableStack();
        private readonly BackgroundMusicPlayer _player;

        public MainWindow()
        {
            InitializeComponent();
            Unloaded += OnUnloaded;
            MouseDown += OnMouseDown;

            _player = _disposables.Add(BackgroundMusicPlayer.TryCreateAndPlay());
            PlayButton.GameSettings = GameSettings;
            PlayButton.MusicPlayer = _player;

            LocalizatorEnvironmentInfo info = InteractionService.LocalizatorEnvironment.Provide();
            if (!info.PlayMusic)
                OnMusicButtonClick(MusicButton, new RoutedEventArgs());
            if (!info.ExitAfterRunGame)
                OnSwitchButtonClick(SwitchButton, new RoutedEventArgs());
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _disposables.Dispose();
        }

        private void OnMusicButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_player == null)
                    return;

                UiImageButton button = (UiImageButton)sender;
                if (_player.PlaybackState == PlaybackState.Playing)
                {
                    InteractionService.LocalizatorEnvironment.Provide().PlayMusic = false;
                    button.ImageSource = Icons.DisabledMusicIcon;
                    _player.Pause();
                }
                else
                {
                    InteractionService.LocalizatorEnvironment.Provide().PlayMusic = true;
                    button.ImageSource = Icons.EnabledMusicIcon;
                    _player.Play();
                }
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(this, ex);
            }
        }

        private void OnSwitchButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                UiImageButton button = (UiImageButton)sender;
                if (ReferenceEquals(button.ImageSource, Icons.DisabledSwitchIcon))
                {
                    button.ImageSource = Icons.EnabledSwitchIcon;
                    InteractionService.LocalizatorEnvironment.Provide().ExitAfterRunGame = true;
                }
                else
                {
                    button.ImageSource = Icons.DisabledSwitchIcon;
                    InteractionService.LocalizatorEnvironment.Provide().ExitAfterRunGame = false;
                }
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(this, ex);
            }
        }

//        public string GetUserName()
//        {
//            return Dispatcher.Invoke(()=>NameTextBox.Text);
//        }

//        public async Task<string> GetSecurityKeyAsync(bool log)
//        {
//            string name = Dispatcher.Invoke(()=>NameTextBox.Text);
//            string password = Dispatcher.Invoke(()=>PasswordBox.Password);
//
//            return await Task.Factory.StartNew(() => GetSecurityKey(name, password, log));
//        }
//
//        public string GetSecurityKey(string name, string password, bool log)
//        {
//            ForumAccessor forumAccessor = new ForumAccessor();
//            forumAccessor.Login(name, password);
//            if (log)
//                forumAccessor.Log("Установка патча", "Коварно пытаюсь поставить патч.");
//            return forumAccessor.ReadSecurityKey();
//        }
        private void OnHyperlinkClick(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}