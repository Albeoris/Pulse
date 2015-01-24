using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core
{
    public sealed class Pair<T1, T2>
    {
        public T1 Item1;
        public T2 Item2;

        public bool IsAnyEmpty
        {
            get { return Item1 == null || Item2 == null; }
        }
    }
}