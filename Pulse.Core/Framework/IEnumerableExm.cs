using System;
using System.Collections.Generic;
using System.Linq;

namespace Pulse.Core
{
    public static class IEnumerableExm
    {
        public static IEnumerable<TResult> SafeSelect<TSource, TResult>(this IEnumerable<TSource> self, Func<TSource, TResult> selector)
        {
            return self?.Select(selector) ?? new TResult[0];
        }

        public static IEnumerable<TResult> SelectWhere<TSource, TResult>(this IEnumerable<TSource> self, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
        {
            return from item in self where predicate(item) select selector(item);
        }

        public static IEnumerable<T> Order<T>(this IEnumerable<T> self)
        {
            return self.OrderBy(t => t);
        }

        public static IEnumerable<T> Order<T>(this IEnumerable<T> self, IComparer<T> comparer)
        {
            return self.OrderBy(t => t, comparer);
        }

        public static IEnumerable<T> DistinctBy<T, TValue>(this IEnumerable<T> self, Func<T, TValue> predicate)
        {
            HashSet<TValue> set = new HashSet<TValue>();
            return self.Where(item => set.Add(predicate(item)));
        }
    }
}