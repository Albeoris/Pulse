using System;
using System.Collections.Generic;

namespace Pulse.Core
{
    [Serializable]
    public sealed class LambdaComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _equals;
        private readonly Func<T, int> _hash;

        private LambdaComparer(Func<T,T,bool> equals, Func<T, int> hash)
        {
            _equals = equals;
            _hash = hash;
        }

        public bool Equals(T x, T y)
        {
            return _equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return _hash(obj);   
        }
    }
}