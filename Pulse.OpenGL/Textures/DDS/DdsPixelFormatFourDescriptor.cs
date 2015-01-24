using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Pulse.OpenGL
{
    /// <summary>
    /// A FourCC descriptor.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 4)]
    public struct DdsPixelFormatFourDescriptor : IEquatable<DdsPixelFormatFourDescriptor>
    {
        private readonly uint _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="DdsPixelFormatFourDescriptor" /> struct.
        /// </summary>
        /// <param name="ddsPixelFormatFourDescriptor">The fourCC value as a string .</param>
        public DdsPixelFormatFourDescriptor(string ddsPixelFormatFourDescriptor)
        {
            if (ddsPixelFormatFourDescriptor.Length != 4)
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Invalid length for FourCC(\"{0}\". Must be be 4 characters long ", ddsPixelFormatFourDescriptor), "ddsPixelFormatFourDescriptor");
            _value = ((uint)ddsPixelFormatFourDescriptor[3]) << 24 | ((uint)ddsPixelFormatFourDescriptor[2]) << 16 | ((uint)ddsPixelFormatFourDescriptor[1]) << 8 | ddsPixelFormatFourDescriptor[0];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DdsPixelFormatFourDescriptor" /> struct.
        /// </summary>
        /// <param name="byte1">The byte1.</param>
        /// <param name="byte2">The byte2.</param>
        /// <param name="byte3">The byte3.</param>
        /// <param name="byte4">The byte4.</param>
        public DdsPixelFormatFourDescriptor(char byte1, char byte2, char byte3, char byte4)
        {
            _value = ((uint)byte4) << 24 | ((uint)byte3) << 16 | ((uint)byte2) << 8 | byte1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DdsPixelFormatFourDescriptor" /> struct.
        /// </summary>
        /// <param name="ddsPixelFormatFourDescriptor">The fourCC value as an uint.</param>
        public DdsPixelFormatFourDescriptor(uint ddsPixelFormatFourDescriptor)
        {
            _value = ddsPixelFormatFourDescriptor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DdsPixelFormatFourDescriptor" /> struct.
        /// </summary>
        /// <param name="ddsPixelFormatFourDescriptor">The fourCC value as an int.</param>
        public DdsPixelFormatFourDescriptor(int ddsPixelFormatFourDescriptor)
        {
            _value = unchecked((uint)ddsPixelFormatFourDescriptor);
        }

        public static implicit operator uint(DdsPixelFormatFourDescriptor d)
        {
            return d._value;
        }

        public static implicit operator int(DdsPixelFormatFourDescriptor d)
        {
            return unchecked((int)d._value);
        }

        public static implicit operator DdsPixelFormatFourDescriptor(uint d)
        {
            return new DdsPixelFormatFourDescriptor(d);
        }

        public static implicit operator DdsPixelFormatFourDescriptor(int d)
        {
            return new DdsPixelFormatFourDescriptor(d);
        }

        public static implicit operator string(DdsPixelFormatFourDescriptor d)
        {
            return d.ToString();
        }

        public static implicit operator DdsPixelFormatFourDescriptor(string d)
        {
            return new DdsPixelFormatFourDescriptor(d);
        }

        public override string ToString()
        {
            return string.Format("{0}", new string(new[]
            {
                (char)(_value & 0xFF),
                (char)((_value >> 8) & 0xFF),
                (char)((_value >> 16) & 0xFF),
                (char)((_value >> 24) & 0xFF)
            }));
        }

        public bool Equals(DdsPixelFormatFourDescriptor other)
        {
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is DdsPixelFormatFourDescriptor && Equals((DdsPixelFormatFourDescriptor)obj);
        }

        public override int GetHashCode()
        {
            return (int)_value;
        }

        public static bool operator ==(DdsPixelFormatFourDescriptor left, DdsPixelFormatFourDescriptor right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DdsPixelFormatFourDescriptor left, DdsPixelFormatFourDescriptor right)
        {
            return !left.Equals(right);
        }
    }
}