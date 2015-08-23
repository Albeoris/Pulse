using System.Collections.Generic;

namespace Pulse.Core
{
    public static class ListExm
    {
        public static T AddAndReturn<T>(this List<T> list, T item)
        {
            list.Add(item);
            return item;
        }
    }
}