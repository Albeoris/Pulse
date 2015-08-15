using System;
using System.Threading;
using System.Timers;
using System.Windows;
using NAudio.Wave;
using Pulse.Core;
using Pulse.UI;

namespace NAudioDemo.AudioPlaybackDemo
{
    public sealed class UiAudioPlayer : UiStackPanel
    {
        private readonly UiImageButton _playbackButton;
        private readonly UiImageButton _stopButton;
        private readonly UiTextBlock _timeTextBlock;
        private readonly System.Timers.Timer _timer;

        public UiAudioPlayer()
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal;

            Thickness margin = new Thickness(3);
            _playbackButton = AddUiElement(new UiImageButton {ImageSource = Icons.PlayIcon, Margin = margin});
            _stopButton = AddUiElement(new UiImageButton {ImageSource = Icons.StopIcon, Margin = margin});
            _timeTextBlock = AddUiElement(UiTextBlockFactory.Create("00:00 / 00:00"));
            _timeTextBlock.Margin = margin;
            _timeTextBlock.VerticalAlignment = VerticalAlignment.Center;
            
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += OnTimerTick;

            _playbackButton.Click += Playback;
            _stopButton.Click += Stop;

            ChangeAudioSettings(InteractionService.AudioSettings.Provide());
            InteractionService.AudioSettings.InfoProvided += ChangeAudioSettings;
        }

        private IWavePlayer _wavePlayer;
        private WaveStream _waveProvider;

        private void ChangeAudioSettings(AudioSettingsInfo audioSettings)
        {
            IWavePlayer newPlayer = audioSettings.CreateWavePlayer();
            IWavePlayer oldPlayer = Interlocked.Exchange(ref _wavePlayer, newPlayer);
            oldPlayer.SafeDispose();

            newPlayer.PlaybackStopped += OnPlaybackStopped;
        }

        public void SetWave(WaveStream waveProvider)
        {
            _wavePlayer.Stop();
            _waveProvider = TryConvertWave(waveProvider);
            UpdateTime();
        }

        private static WaveStream TryConvertWave(WaveStream waveProvider)
        {
            if (waveProvider == null)
                return null;

            switch (waveProvider.WaveFormat.Encoding)
            {
                case WaveFormatEncoding.Pcm:
                case WaveFormatEncoding.Adpcm:
                    return WaveFormatConversionStream.CreatePcmStream(waveProvider);
            }

            return waveProvider;
        }

        private void Playback(object sender, RoutedEventArgs e)
        {
            if (_waveProvider == null)
                return;

            try
            {
                switch (_wavePlayer.PlaybackState)
                {
                    case PlaybackState.Playing:
                    {
                        _wavePlayer.Pause();
                        _timer.Stop();
                        _playbackButton.ImageSource = Icons.PlayIcon;
                        break;
                    }
                    case PlaybackState.Paused:
                    {
                        _wavePlayer.Play();
                        _timer.Start();
                        _playbackButton.ImageSource = Icons.PauseIcon;
                        break;
                    }
                    case PlaybackState.Stopped:
                    {
                        _wavePlayer.Init(_waveProvider);
                        _wavePlayer.Play();
                        _timer.Start();
                        _playbackButton.ImageSource = Icons.PauseIcon;
                        break;
                    }
                    default:
                    {
                        throw new NotImplementedException(_wavePlayer.PlaybackState.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(this, ex);
            }
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            try
            {
                _wavePlayer.Stop();
            }
            catch(Exception ex)
            {
                UiHelper.ShowError(this, ex);
            }
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => OnPlaybackStopped(sender, e));
                return;
            }

            try
            {
                _playbackButton.ImageSource = Icons.PlayIcon;
                _timeTextBlock.Text = "00:00 / 00:00";

                if (_waveProvider != null)
                    _waveProvider.CurrentTime = TimeSpan.Zero;

                if (e.Exception != null)
                    UiHelper.ShowError(this, e.Exception);
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(this, ex);
            }
            finally
            {
                _timer.Stop();
            }
        }

        private void OnTimerTick(object sender, ElapsedEventArgs e)
        {
            UpdateTime();
        }

        private void UpdateTime()
        {
            try
            {
                if (_waveProvider == null)
                {
                    _timeTextBlock.Dispatcher.Invoke(() => _timeTextBlock.Text = "00:00 / 00:00");
                    return;
                }

                TimeSpan currentTime = (_wavePlayer.PlaybackState == PlaybackState.Stopped) ? TimeSpan.Zero : _waveProvider.CurrentTime;
                TimeSpan totalTime = _waveProvider.TotalTime;

                _timeTextBlock.Dispatcher.Invoke(() => _timeTextBlock.Text = $"{(int)currentTime.TotalMinutes:00}:{currentTime.Seconds:00} / {(int)totalTime.TotalMinutes:00}:{totalTime.Seconds:00}");
            }
            catch
            {
            }
        }
    }
}