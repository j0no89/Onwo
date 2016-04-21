using System.Collections.Generic;

namespace Onwo.Collections
{
    public static class ComparerExtensions
    {
        public static IComparer<T> Invert<T>(this IComparer<T> comparer)
        {
            return Comparer<T>.Create((item1, item2) => comparer.Compare(item2, item1));
        }
    }
}
