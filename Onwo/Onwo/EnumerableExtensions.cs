using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Onwo.Collections;

namespace Onwo
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T,int> action)
        {
            int index = 0;
            foreach (var item in enumerable)
            {
                action(item,index);
                index++;
            }
        }
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
        }
        public static IEnumerable<T> EnumerateFrom<T>(this T[] array, int startIndex)
        {
            for (int index = startIndex; index < array.Length; index++)
            {
                yield return array[index];
            }
        }
        public static IEnumerable<T> EnumerateFrom<T>(this IList<T> list, int startIndex)
        {
            for (int index = startIndex; index < list.Count; index++)
            {
                yield return list[index];
            }
        }

        public static int IndexOfFirst<T>(this IEnumerable<T> enumerable, Predicate<T> pred)
        {
            int cnt = 0;
            foreach (var item in enumerable)
            {
                if (pred(item))
                    return cnt;
                cnt++;
            }
            return -1;
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, T, bool> predicate)
        {
            var eq = new MyEqualityComparer<T>(predicate);
            return first.Except(second, eq);
        }
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable)
            => new ObservableCollection<T>(enumerable);
    }
}
