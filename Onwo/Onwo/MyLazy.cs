using System;

namespace Onwo
{
    public class MyLazy<T>
    {
        private T _value;
        private bool _hasValue;
        private Func<T> _valueFactory;
        public Func<T> ValueFactory
        {
            get { return _valueFactory; }
            set
            {
                _valueFactory = value;
                Reset();
            }
        }
        public bool HasValue => _hasValue;

        public T Value => getValue();

        protected T getValue()
        {
            if (_hasValue)
                return _value;
            _value = _valueFactory == null ? default(T) : ValueFactory();
            _hasValue = true;
            OnValueCreated();
            return _value;
        }

        public MyLazy() : this(null)
        {
        }
        public MyLazy(Func<T> valueFactory)
        {
            _valueFactory = valueFactory;
        }
        public void Reset()
        {
            var old = _value;
            _value = default(T);
            _hasValue = false;
            OnValueDestroyed(old);
        }
        public delegate void ValueCreatedEventHandler(MyLazy<T> lazy);

        public event ValueCreatedEventHandler ValueCreated;

        protected virtual void OnValueCreated()
        {
            ValueCreated?.Invoke(this);
        }
        public delegate void ValueDestroyedEventHandler(MyLazy<T> lazy, T oldVal);

        public event ValueDestroyedEventHandler ValueDestroyed;

        protected virtual void OnValueDestroyed(T old)
        {
            ValueDestroyed?.Invoke(this, old);
        }
    }
}
