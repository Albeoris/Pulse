using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Pulse.Core;

namespace Pulse.OpenGL
{
    public static class ColorsHelper
    {
        private const double ColorRate = 255 / 31;

        public static bool IsBlack(Color color)
        {
            return (0 == color.R && color.R == color.G && color.G == color.B);
        }

        public static Color ReadColor(Stream input, byte[] buff)
        {
            switch (buff.Length)
            {
                case 2:
                    return ReadA1B5G5R5Color(input, buff);
                case 3:
                    return ReadB8G8R8Color(input, buff);
                case 4:
                    return ReadBGRAColor(input, buff);
                default:
                    throw new NotSupportedException(buff.Length.ToString(CultureInfo.InvariantCulture));
            }
        }

        private static Color ReadA1B5G5R5Color(Stream input, byte[] buff)
        {
            input.EnsureRead(buff, 0, 2);
            ushort color = BitConverter.ToUInt16(buff, 0);

            return Color.FromArgb(
                (byte)(((color >> 15) & 1) * 255),
                (byte)Math.Round((color & 31) * ColorRate),
                (byte)Math.Round((color >> 5 & 31) * ColorRate),
                (byte)Math.Round((color >> 10 & 31) * ColorRate));
        }

        private static Color ReadB8G8R8Color(Stream input, byte[] buff)
        {
            input.EnsureRead(buff, 0, 3);
            return Color.FromArgb(255, buff[2], buff[1], buff[0]);
        }

        private static Color ReadBGRAColor(Stream input, byte[] buff)
        {
            input.EnsureRead(buff, 0, 4);
            return Color.FromArgb(buff[3], buff[2], buff[1], buff[0]);
        }

        public static void WriteBgra(Stream output, Color color)
        {
            output.WriteStruct(color.B);
            output.WriteStruct(color.G);
            output.WriteStruct(color.R);
            output.WriteStruct(color.A);
        }

        public static void WriteBgr(Stream output, Color color)
        {
            output.WriteStruct(color.B);
            output.WriteStruct(color.G);
            output.WriteStruct(color.R);
        }
    }
}
