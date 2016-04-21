using System;
using System.Collections.Generic;

namespace Onwo.Collections
{
    public class MyEqualityComparer<T> : EqualityComparer<T>
    {
        private readonly Func<T, T, bool> _compareFunc;
        private readonly Func<T, int> _hashFunc;
        public MyEqualityComparer(Func<T, T, bool> compareFunc)
            :this(compareFunc, null) { }
        public MyEqualityComparer(Func<T, T, bool> compareFunc, Func<T, int> hashFunc)
        {
            _compareFunc = compareFunc;
            if (hashFunc == null)
                _hashFunc = obj => { return obj.GetHashCode(); };
            else
                _hashFunc = hashFunc;
        }
        public override bool Equals(T x, T y)
        {
            return _compareFunc(x, y);
        }
        public override int GetHashCode(T obj)
        {
            return _hashFunc(obj);
        }
    }
}
