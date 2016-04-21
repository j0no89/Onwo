using System;
using System.Collections.Generic;

namespace Onwo.Collections
{
    public static class DefaultComparers
    {
        public static IComparer<T> NullComparer<T>() => Comparer<T>.Create((val1, val2) => 1);
        
        public static readonly IComparer<string> StringComparer_IgnoreCase = Comparer<string>.Create(
            (s1, s2) => string.Compare(s1, s2, StringComparison.InvariantCultureIgnoreCase));
        public static readonly IEqualityComparer<string> NoCaseStringEquals =
            new MyEqualityComparer<string>((s1, s2) =>
                string.Equals(s1, s2, StringComparison.InvariantCultureIgnoreCase));
    }
}
