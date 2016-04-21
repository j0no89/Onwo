using System.Collections.Generic;

namespace Onwo.Collections
{
    public static class IListExt
    {
        public static void AddRange<T>(this IList<T> collection, IEnumerable<T> items)
            => items?.ForEach(collection.Add);
    }
}
