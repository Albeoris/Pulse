using System;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace Pulse.OpenGL
{
    public sealed class PixelFormatDescriptor
    {
        public readonly System.Drawing.Imaging.PixelFormat DrawingFormat;
        public readonly PixelInternalFormat GLPixelInternalFormat;
        public readonly PixelFormat GLPixelFormat;
        public readonly PixelType GLPixelType;

        public PixelFormatDescriptor(
            System.Drawing.Imaging.PixelFormat drawingFormat,
            PixelInternalFormat glInternalPixelFormat,
            PixelFormat glPixelFormat,
            PixelType glPixelType)
        {
            DrawingFormat = drawingFormat;
            GLPixelInternalFormat = glInternalPixelFormat;
            GLPixelFormat = glPixelFormat;
            GLPixelType = glPixelType;
        }

        public static implicit operator System.Drawing.Imaging.PixelFormat(PixelFormatDescriptor self)
        {
            return self.DrawingFormat;
        }

        public ImageFormat GetOptimalImageFormat()
        {
            return DrawingFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed ? ImageFormat.Gif : ImageFormat.Png;
        }

        public int BytesPerPixel
        {
            get
            {
                switch (DrawingFormat)
                {
                    case System.Drawing.Imaging.PixelFormat.Format8bppIndexed: // misses glColorTable setup
                        return 1;
                    case System.Drawing.Imaging.PixelFormat.Format16bppArgb1555:
                    case System.Drawing.Imaging.PixelFormat.Format16bppRgb555: // does not work
                        return 2;
                    case System.Drawing.Imaging.PixelFormat.Format24bppRgb: // works
                        return 3;
                    case System.Drawing.Imaging.PixelFormat.Format32bppRgb: // has alpha too? wtf?
                    case System.Drawing.Imaging.PixelFormat.Canonical:
                    case System.Drawing.Imaging.PixelFormat.Format32bppArgb: // works
                        return 4;
                    default:
                        throw new NotSupportedException(DrawingFormat.ToString());
                }
            }
        }

        public static implicit operator PixelFormat(PixelFormatDescriptor self)
        {
            return self.GLPixelFormat;
        }

        public static implicit operator PixelInternalFormat(PixelFormatDescriptor self)
        {
            return self.GLPixelInternalFormat;
        }

        public static implicit operator PixelType(PixelFormatDescriptor self)
        {
            return self.GLPixelType;
        }

        public static implicit operator PixelFormatDescriptor(System.Drawing.Imaging.PixelFormat drawingFormat)
        {
            PixelInternalFormat glInternalPixelFormat;
            PixelFormat glPixelFormat;
            PixelType glPixelType;
            GetOpenGlFormat(drawingFormat, out glInternalPixelFormat, out glPixelFormat, out glPixelType);
            return new PixelFormatDescriptor(drawingFormat, glInternalPixelFormat, glPixelFormat, glPixelType);
        }

        private static void GetOpenGlFormat(System.Drawing.Imaging.PixelFormat drawingFormat,
            out PixelInternalFormat glInternalPixelFormat, out PixelFormat glPixelFormat, out PixelType glPixelType)
        {
            switch (drawingFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed: // misses glColorTable setup
                    glInternalPixelFormat = PixelInternalFormat.R8;
                    glPixelFormat = PixelFormat.Red;
                    glPixelType = PixelType.UnsignedByte;
                    break;
                case System.Drawing.Imaging.PixelFormat.Format16bppArgb1555:
                case System.Drawing.Imaging.PixelFormat.Format16bppRgb555: // does not work
                    glInternalPixelFormat = PixelInternalFormat.Rgb5A1;
                    glPixelFormat = PixelFormat.Bgr;
                    glPixelType = PixelType.UnsignedShort5551Ext;
                    break;
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb: // works
                    glInternalPixelFormat = PixelInternalFormat.Rgb8;
                    glPixelFormat = PixelFormat.Bgr;
                    glPixelType = PixelType.UnsignedByte;
                    break;
                case System.Drawing.Imaging.PixelFormat.Format32bppRgb: // has alpha too? wtf?
                case System.Drawing.Imaging.PixelFormat.Canonical:
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb: // works
                    glInternalPixelFormat = PixelInternalFormat.Rgba;
                    glPixelFormat = PixelFormat.Bgra;
                    glPixelType = PixelType.UnsignedByte;
                    break;
                default:
                    throw new NotSupportedException(drawingFormat.ToString());
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is PixelFormatDescriptor && Equals((PixelFormatDescriptor)obj);
        }

        private bool Equals(PixelFormatDescriptor other)
        {
            return DrawingFormat == other.DrawingFormat
                   && GLPixelInternalFormat == other.GLPixelInternalFormat
                   && GLPixelFormat == other.GLPixelFormat
                   && GLPixelType == other.GLPixelType;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int)DrawingFormat;
                hashCode = (hashCode * 397) ^ (int)GLPixelInternalFormat;
                hashCode = (hashCode * 397) ^ (int)GLPixelFormat;
                hashCode = (hashCode * 397) ^ (int)GLPixelType;
                return hashCode;
            }
        }
    }
}