using System;
using System.IO;
using System.Reflection;
using NAudio.FileFormats.Mp3;
using NAudio.Wave;
using Pulse.Core;

namespace Pulse.Patcher
{
    public sealed class BackgroundMusicPlayer : IDisposable
    {
        public static BackgroundMusicPlayer TryCreateAndPlay()
        {
            try
            {
                BackgroundMusicPlayer result = new BackgroundMusicPlayer();
                result.Play();
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
        }

        private const string FileName = "Pulse.Patcher.Background.mp3";

        private readonly Stream _stream;
        private readonly WaveOut _waveOutDevice;
        private readonly Mp3FileReader _audioFileReader;
        private readonly DisposableStack _disposables = new DisposableStack(3);

        public BackgroundMusicPlayer()
        {
            try
            {
                _stream = _disposables.Add(Assembly.GetExecutingAssembly().GetManifestResourceStream(FileName));
                _audioFileReader = _disposables.Add(new Mp3FileReader(_stream, CreateMp3Decompressor));
                _waveOutDevice = _disposables.Add(new WaveOut());
                _waveOutDevice.Init(_audioFileReader);
                _waveOutDevice.PlaybackStopped += OnPlaybackStopped;
            }
            catch
            {
                _disposables.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void Play()
        {
            _waveOutDevice.Play();
        }

        public void Pause()
        {
            _waveOutDevice.Pause();
        }

        public PlaybackState PlaybackState
        {
            get { return _waveOutDevice.PlaybackState; }
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

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (e.Exception == null)
                _stream.SetPosition(0);

            _waveOutDevice.Play();
        }
    }
}