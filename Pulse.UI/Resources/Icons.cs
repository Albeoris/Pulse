using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brushes = System.Windows.Media.Brushes;
using ShapePath = System.Windows.Shapes.Path;

namespace Pulse.UI
{
    public static class Icons
    {
        public static DrawingImage OkIcon
        {
            get { return LazyOkIcon.Value; }
        }

        public static DrawingImage CrossIcon
        {
            get { return LazyCrossIcon.Value; }
        }

        public static DrawingImage PendingIcon
        {
            get { return LazyPendingIcon.Value; }
        }

        public static DrawingImage PlayIcon
        {
            get { return LazyPlayIcon.Value; }
        }

        public static DrawingImage PauseIcon
        {
            get { return LazyPauseIcon.Value; }
        }

        public static DrawingImage StopIcon
        {
            get { return LazyStopIcon.Value; }
        }

        public static DrawingImage EnabledMusicIcon
        {
            get { return LazyEnabledMusicIcon.Value; }
        }

        public static DrawingImage DisabledMusicIcon
        {
            get { return LazyDisabledMusicIcon.Value; }
        }

        public static DrawingImage EnabledSwitchIcon
        {
            get { return LazyEnabledSwitchIcon.Value; }
        }

        public static DrawingImage DisabledSwitchIcon
        {
            get { return LazyDisabledSwitchIcon.Value; }
        }

        public static DrawingImage PackageIcon
        {
            get { return LazyPackageIcon.Value; }
        }

        public static BitmapSource DiskIcon
        {
            get { return LazyDiskIcon.Value; }
        }

        public static BitmapSource FolderIcon
        {
            get { return LazyFolderIcon.Value; }
        }

        public static BitmapSource TxtFileIcon
        {
            get { return LazyTxtFileIcon.Value; }
        }

        private static readonly Lazy<DrawingImage> LazyOkIcon = new Lazy<DrawingImage>(CreateGreenOkIcon);
        private static readonly Lazy<DrawingImage> LazyCrossIcon = new Lazy<DrawingImage>(CreateRedCrossIcon);
        private static readonly Lazy<DrawingImage> LazyPendingIcon = new Lazy<DrawingImage>(CreatePendingIcon);

        private static readonly Lazy<DrawingImage> LazyPlayIcon = new Lazy<DrawingImage>(CreatePlayIcon);
        private static readonly Lazy<DrawingImage> LazyPauseIcon = new Lazy<DrawingImage>(CreatePauseIcon);
        private static readonly Lazy<DrawingImage> LazyStopIcon = new Lazy<DrawingImage>(CreateStopIcon);
        
        private static readonly Lazy<DrawingImage> LazyEnabledMusicIcon = new Lazy<DrawingImage>(CreateEnabledMusicIcon);
        private static readonly Lazy<DrawingImage> LazyDisabledMusicIcon = new Lazy<DrawingImage>(CreateDisabledMusicIcon);
        private static readonly Lazy<DrawingImage> LazyEnabledSwitchIcon = new Lazy<DrawingImage>(CreateEnabledSwitchIcon);
        private static readonly Lazy<DrawingImage> LazyDisabledSwitchIcon = new Lazy<DrawingImage>(CreateDisabledSwitchIcon);

        private static readonly Lazy<DrawingImage> LazyPackageIcon = new Lazy<DrawingImage>(CreatePackageIcon);
        private static readonly Lazy<BitmapSource> LazyDiskIcon = new Lazy<BitmapSource>(CreateDiskIcon);
        private static readonly Lazy<BitmapSource> LazyFolderIcon = new Lazy<BitmapSource>(CreateDirectoryIcon);
        private static readonly Lazy<BitmapSource> LazyTxtFileIcon = new Lazy<BitmapSource>(() => CreateFileIcon(".txt"));

        private static DrawingImage CreateGreenOkIcon()
        {
            Pen pen = new Pen(Brushes.DarkGreen, 3);
            PathGeometry geometry = (PathGeometry)Application.Current.FindResource("CheckmarkIconGeometry");
            GeometryDrawing drawning = new GeometryDrawing(Brushes.ForestGreen, pen, geometry);
            DrawingImage imageSource = new DrawingImage(drawning);

            imageSource.Freeze();
            return imageSource;
        }

        private static DrawingImage CreateRedCrossIcon()
        {
            Pen pen = new Pen(Brushes.DarkRed, 3);
            PathGeometry geometry = (PathGeometry)Application.Current.FindResource("CrossIconGeometry");
            GeometryDrawing drawning = new GeometryDrawing(Brushes.Red, pen, geometry);
            DrawingImage imageSource = new DrawingImage(drawning);

            imageSource.Freeze();
            return imageSource;
        }

        private static DrawingImage CreatePendingIcon()
        {
            Pen pen = new Pen(Brushes.DimGray, 3);
            PathGeometry geometry = (PathGeometry)Application.Current.FindResource("ClockIconGeometry");
            GeometryDrawing drawning = new GeometryDrawing(Brushes.DarkGray, pen, geometry);
            DrawingImage imageSource = new DrawingImage(drawning);

            imageSource.Freeze();
            return imageSource;
        }

        private static DrawingImage CreatePlayIcon()
        {
            Pen pen = new Pen(Brushes.Green, 3);
            PathGeometry geometry = (PathGeometry)Application.Current.FindResource("PlayIconGeometry");
            GeometryDrawing drawning = new GeometryDrawing(Brushes.LimeGreen, pen, geometry);
            DrawingImage imageSource = new DrawingImage(drawning);

            imageSource.Freeze();
            return imageSource;
        }

        private static DrawingImage CreatePauseIcon()
        {
            Pen pen = new Pen(Brushes.Black, 3);
            PathGeometry geometry = (PathGeometry)Application.Current.FindResource("PauseIconGeometry");
            GeometryDrawing drawning = new GeometryDrawing(Brushes.OrangeRed, pen, geometry);
            DrawingImage imageSource = new DrawingImage(drawning);

            imageSource.Freeze();
            return imageSource;
        }

        private static DrawingImage CreateStopIcon()
        {
            Pen pen = new Pen(Brushes.Black, 3);
            PathGeometry geometry = (PathGeometry)Application.Current.FindResource("StopIconGeometry");
            GeometryDrawing drawning = new GeometryDrawing(Brushes.Black, pen, geometry);
            DrawingImage imageSource = new DrawingImage(drawning);

            imageSource.Freeze();
            return imageSource;
        }

        private static DrawingImage CreateEnabledMusicIcon()
        {
            Pen pen = new Pen(Brushes.Black, 0);
            PathGeometry geometry = (PathGeometry)Application.Current.FindResource("MusicIconGeometry");
            GeometryDrawing drawning = new GeometryDrawing(Brushes.Black, pen, geometry);
            DrawingImage imageSource = new DrawingImage(drawning);

            imageSource.Freeze();
            return imageSource;
        }

        private static DrawingImage CreateDisabledMusicIcon()
        {
            Pen pen = new Pen(Brushes.LightGray, 0);
            PathGeometry geometry = (PathGeometry)Application.Current.FindResource("MusicIconGeometry");
            GeometryDrawing drawning = new GeometryDrawing(Brushes.LightGray, pen, geometry);
            DrawingImage imageSource = new DrawingImage(drawning);

            imageSource.Freeze();
            return imageSource;
        }

        private static DrawingImage CreateEnabledSwitchIcon()
        {
            Pen pen = new Pen(Brushes.Black, 0);
            PathGeometry geometry = (PathGeometry)Application.Current.FindResource("SwitchIconGeometry");
            GeometryDrawing drawning = new GeometryDrawing(Brushes.Black, pen, geometry);
            DrawingImage imageSource = new DrawingImage(drawning);

            imageSource.Freeze();
            return imageSource;
        }

        private static DrawingImage CreateDisabledSwitchIcon()
        {
            Pen pen = new Pen(Brushes.LightGray, 0);
            PathGeometry geometry = (PathGeometry)Application.Current.FindResource("SwitchIconGeometry");
            GeometryDrawing drawning = new GeometryDrawing(Brushes.LightGray, pen, geometry);
            DrawingImage imageSource = new DrawingImage(drawning);

            imageSource.Freeze();
            return imageSource;
        }

        private static DrawingImage CreatePackageIcon()
        {
            Pen pen = new Pen(Brushes.Black, 3);
            PathGeometry geometry = (PathGeometry)Application.Current.FindResource("PackageIconGeometry");
            GeometryDrawing drawning = new GeometryDrawing(Brushes.Blue, pen, geometry);
            DrawingImage imageSource = new DrawingImage(drawning);

            imageSource.Freeze();
            return imageSource;
        }

        private static BitmapSource CreateDiskIcon()
        {
            string disk = Path.GetPathRoot(Path.GetTempPath());
            return ShellHelper.ExtractAssociatedIcon(disk, false);
        }

        private static BitmapSource CreateDirectoryIcon()
        {
            string folder = Path.GetTempPath();
            return ShellHelper.ExtractAssociatedIcon(folder, false);
        }

        private static BitmapSource CreateFileIcon(string extension)
        {
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + extension);
            File.Create(path).Close();
            try
            {
                return ShellHelper.ExtractAssociatedIcon(path, false);
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}