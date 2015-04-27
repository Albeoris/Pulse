using System;

namespace Pulse.Core
{
    public sealed class CircularBuffer<T>
    {
        private readonly T[] _buff;
        private int _index;

        public int Length
        {
            get { return _buff.Length; }
        }

        public long Index
        {
            get { return _index; }
        }

        public CircularBuffer(int length)
        {
            if (length < 1)
                throw new Exception("Длина циклического буфера не может быть меньше 1.");

            _buff = new T[length];
        }

        public void Write(T value)
        {
            _buff[_index] = value;
            _index = (_index + 1) % _buff.Length;
        }

        public void Write(byte[] value, int index, int length)
        {
            index += (length / _buff.Length) * _buff.Length;
            length %= _buff.Length;

            int last = Math.Min(length, (_buff.Length - _index));
            Array.Copy(value, index, _buff, _index, last);

            int first = length - last;
            if (first != 0)
                Array.Copy(value, index + last, _buff, 0, first);

            _index = (index + length) % _buff.Length;
        }

        public T GetByOffset(int offset)
        {
            if (offset < 0)
                offset = _buff.Length + offset;
            return _buff[offset];
        }
    }
}