using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Pulse.Core;
using Pulse.UI;

namespace Pulse.Patcher
{
    public sealed class UiPatcherDownloadButton : UiButton
    {
        private const string FileName = "ff13.ffrtt.ru";

        private readonly UiTextBlock _downloadButtonLabel;
        private readonly UiTextBlock _downloadButtonTimeLabel;

        private DateTime _localFileTime;
        private DateTime _remoteFileTime = new DateTime(2015, 1, 1);

        public UiPatcherDownloadButton()
        {
            Loaded += OnLoaded;

            Width = 200;
            Height = 80;

            UiStackPanel stackPanel = UiStackPanelFactory.Create(Orientation.Vertical);
            {
                _downloadButtonLabel = stackPanel.AddUiElement(UiTextBlockFactory.Create("Скачать"));
                _downloadButtonTimeLabel = stackPanel.AddUiElement(UiTextBlockFactory.Create(String.Empty));
            }

            Content = stackPanel;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _localFileTime = File.GetCreationTime(FileName);
                RefreshColor(false);

                const string gitArchiveLink = "http://github.com/Albeoris/Pulse/blob/master/Translate/FF13TranslationAlpha.ecp";

                int count = 2;
                using (HttpClient client = new HttpClient())
                using (Stream input = await client.GetStreamAsync(gitArchiveLink))
                {
                    StreamReader sr = new StreamReader(input);
                    while (!sr.EndOfStream && count > 0)
                    {
                        string line = await sr.ReadLineAsync();
                        if (line == null)
                            continue;

                        const string sizePattern = "<div class=\"info file-name\">";
                        const string timePattern = "<time datetime=\"";

                        if (line.Contains(sizePattern))
                        {
                            line = sr.ReadLine();
                            count--;

                            if (line == null)
                                continue;

                            string size = line.Replace("span>", " ").Trim(' ', '<', '/', '\t');
                            _downloadButtonLabel.Text = "Скачать (" + size + ')';
                            continue;
                        }

                        int tokenIndex = line.IndexOf(timePattern, StringComparison.Ordinal);
                        if (tokenIndex < 0)
                            continue;

                        tokenIndex += timePattern.Length;
                        int length = line.IndexOf('"', tokenIndex + 1) - tokenIndex;

                        string value = line.Substring(tokenIndex, length);
                        count--;

                        if (!DateTime.TryParse(value, out _remoteFileTime))
                            continue;

                        _downloadButtonTimeLabel.Text = _remoteFileTime.ToString(CultureInfo.CurrentCulture);
                        RefreshColor(true);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private void RefreshColor(bool isStrictEquality)
        {
            if (isStrictEquality)
            {
                if (_localFileTime == _remoteFileTime)
                {
                    MakeGray();
                    return;
                }
            }
            else if (_localFileTime > _remoteFileTime)
            {
                MakeGray();
                return;
            }

            MakeGreen();
        }

        private void MakeGray()
        {
            Background = Brushes.Gray;
        }

        private void MakeGreen()
        {
            Background = Brushes.ForestGreen;
        }

        protected override async void OnClick()
        {
            base.OnClick();

            IsEnabled = false;

            try
            {
                await Downloader.Download(FileName);
                
                _localFileTime = _remoteFileTime;
                File.SetCreationTime(FileName, _localFileTime);
                RefreshColor(true);

                MessageBox.Show((Window)this.GetRootElement(), "Готово!");
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(ex);
            }
            finally
            {
                IsEnabled = true;
            }
        }
    }
}