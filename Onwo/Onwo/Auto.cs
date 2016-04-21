using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;

namespace Onwo
{
    [ComVisible(false)]
    [HostProtection(Action = SecurityAction.LinkDemand,
    Resources = HostProtectionResource.Synchronization | HostProtectionResource.SharedState)]
    public class Auto<T>
    {
        public Auto(Func<T> initializationFunction,
            LazyThreadSafetyMode safetyMode = LazyThreadSafetyMode.ExecutionAndPublication)
        {
            _syncRoot = new object();
            _isValueCreated = false;
            ThreadSafety = safetyMode;
            InitializationFunction = initializationFunction;
        }

        protected LazyThreadSafetyMode ThreadSafety { get; private set; }
        protected Func<T> InitializationFunction { get; private set; }
        private readonly object _syncRoot;

        public T Value
        {
            get
            {
                if (!_isValueCreated)
                {
                    bool triggerEvent = false;
                    if (ThreadSafety == LazyThreadSafetyMode.PublicationOnly)
                    {
                        var value = InitializationFunction();
                        
                        lock (_syncRoot)
                        {
                            if (!_isValueCreated)
                            {
                                _value = value;
                                _isValueCreated = true;
                                triggerEvent = true;
                            }
                        }
                       
                    }
                    else if (ThreadSafety == LazyThreadSafetyMode.ExecutionAndPublication)
                    {
                        lock (_syncRoot)
                        {
                            if (!_isValueCreated)
                            {
                                _value = InitializationFunction();
                                _isValueCreated = true;
                                triggerEvent = true;
                            }
                        }
                    }
                    else
                    {
                        _value = InitializationFunction();
                        _isValueCreated = true;
                        triggerEvent = true;
                    }
                    if (triggerEvent)
                        OnIsValueCreatedChanged();
                }
                return _value;
            }
        }
        private T _value;

        public bool IsValueCreated
        {
            get { return _isValueCreated; }

        }

        public delegate void IsValueCreatedChangedEventHandler(Auto<T> sender);

        public event IsValueCreatedChangedEventHandler IsValueCreatedChanged;
        protected virtual void OnIsValueChangedOverride() { }
        private void OnIsValueCreatedChanged()
        {
            //must be called outside of the locks
            OnIsValueChangedOverride();
            IsValueCreatedChanged?.Invoke(this);
        }
        private bool _isValueCreated;
        public void ResetValue()
        {
            if (_isValueCreated)
            {
                bool triggerEvent = false;
                if (ThreadSafety != LazyThreadSafetyMode.None)
                {
                    lock (_syncRoot)
                    {
                        if (_isValueCreated)
                        {
                            _value = default(T);
                            _isValueCreated = false;
                            triggerEvent = true;
                        }
                    }
                }
                else
                {
                    _value = default(T);
                    _isValueCreated = false;
                    triggerEvent = true;
                }
                if (triggerEvent)
                    OnIsValueCreatedChanged();
            }
            
        }
    }
    public static class Auto
    {
        public static Auto<T> Create<T>(Func<T> initializationFunction, LazyThreadSafetyMode safetyMode = LazyThreadSafetyMode.ExecutionAndPublication)
            where T : class
        {
            return new Auto<T>(initializationFunction, safetyMode);

        }
    }

}
