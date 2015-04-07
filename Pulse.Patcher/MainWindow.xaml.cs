using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using NAudio.FileFormats.Mp3;
using NAudio.Wave;
using Pulse.Core;
using Pulse.UI;

namespace Pulse.Patcher
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly DisposableStack _disposables = new DisposableStack();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _disposables.Dispose();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Stream stream = _disposables.Add(Assembly.GetExecutingAssembly().GetManifestResourceStream("Pulse.Patcher.Background.mp3"));
                IWavePlayer waveOutDevice = _disposables.Add(new WaveOut());
                Mp3FileReader audioFileReader = _disposables.Add(new Mp3FileReader(stream, CreateMp3Decompressor));
                waveOutDevice.Init(audioFileReader);
                waveOutDevice.Play();
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(this, ex);
            }
        }

        private IMp3FrameDecompressor CreateMp3Decompressor(WaveFormat mp3Format)
        {
            try
            {
                return new AcmMp3FrameDecompressor(mp3Format);
            }
            catch
            {
                return new DmoMp3FrameDecompressor(mp3Format);
            }
        }

        public async Task<string> GetSecurityKeyAsync(bool log)
        {
            string name = Dispatcher.Invoke(()=>NameTextBox.Text);
            string password = Dispatcher.Invoke(()=>PasswordBox.Password);

            return await Task.Factory.StartNew(() => GetSecurityKey(name, password, log));
        }

        public string GetSecurityKey(string name, string password, bool log)
        {
            ForumAccessor forumAccessor = new ForumAccessor();
            forumAccessor.Login(name, password);
            if (log)
                forumAccessor.Log("Установка патча", "Коварно пытаюсь поставить патч.");
            return forumAccessor.ReadSecurityKey();
        }
    }
}