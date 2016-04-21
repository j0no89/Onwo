using System;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace Onwo.Threading.Tasks
{
    public interface IAwaitable
    {
        IAwaiter GetAwaiter();
    }

    public interface IAwaiter : ICriticalNotifyCompletion
    {
        // INotifyCompletion has one method: void OnCompleted(Action continuation);

        // ICriticalNotifyCompletion implements INotifyCompletion,
        // also has this method: void UnsafeOnCompleted(Action continuation);

        bool IsCompleted { get; }

        void GetResult();
    }
    public interface IAwaitable<out TResult>:IAwaitable
    {
        new IAwaiter<TResult> GetAwaiter();
    }

    public interface IAwaiter<out TResult> : IAwaiter
    {
        new TResult GetResult();
    }
    public class MyAwaiter : IAwaiter
    {
        public MyAwaiter(TaskAwaiter awaiter)
        {
            _onCompleted = awaiter.OnCompleted;
            _unsafeOnCompleted = awaiter.UnsafeOnCompleted;
            _getResult = awaiter.GetResult;
            _isCompleted = () => awaiter.IsCompleted;
        }

        public MyAwaiter(ConfiguredTaskAwaitable awaitable)
        {
            var awaiter = awaitable.GetAwaiter();
            _onCompleted = awaiter.OnCompleted;
            _unsafeOnCompleted = awaiter.UnsafeOnCompleted;
            _getResult = awaiter.GetResult;
            _isCompleted = () => awaiter.IsCompleted;
        }
        public MyAwaiter(YieldAwaitable awaitable)
        {
            var awaiter = awaitable.GetAwaiter();
            _onCompleted = awaiter.OnCompleted;
            _unsafeOnCompleted = awaiter.UnsafeOnCompleted;
            _getResult = awaiter.GetResult;
            _isCompleted = () => awaiter.IsCompleted;
        }
        public MyAwaiter(DispatcherPriorityAwaitable awaitable)
        {
            var awaiter = awaitable.GetAwaiter();
            _onCompleted = awaiter.OnCompleted;
            _unsafeOnCompleted = awaiter.OnCompleted;
            _getResult = awaiter.GetResult;
            _isCompleted = () => awaiter.IsCompleted;
        }
        private readonly Action<Action> _onCompleted;
        private readonly Action<Action> _unsafeOnCompleted;
        private readonly Action _getResult;
        private readonly Func<bool> _isCompleted;
        public void OnCompleted(Action continuation)
        {
            _onCompleted(continuation);
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            throw new NotImplementedException();
        }

        public bool IsCompleted => _isCompleted();
        public void GetResult()
        {
            _getResult();
        }
    }
    public class MyAwaiter<T>:IAwaiter<T>
    {
        public MyAwaiter(TaskAwaiter<T> awaiter )
        {
            _onCompleted = awaiter.OnCompleted;
            _unsafeOnCompleted = awaiter.UnsafeOnCompleted;
            _getResult = awaiter.GetResult;
            _isCompleted = () => awaiter.IsCompleted;
        }

        public MyAwaiter(ConfiguredTaskAwaitable<T> awaitable)
        {
            var awaiter = awaitable.GetAwaiter();
            _onCompleted = awaiter.OnCompleted;
            _unsafeOnCompleted = awaiter.UnsafeOnCompleted;
            _getResult = awaiter.GetResult;
            _isCompleted = () => awaiter.IsCompleted;
        }
        
        private readonly Action<Action> _onCompleted;
        private readonly Action<Action> _unsafeOnCompleted;
        private readonly Func<T> _getResult;
        private readonly Func<bool> _isCompleted;
        public void OnCompleted(Action continuation)
        {
            _onCompleted(continuation);
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            throw new NotImplementedException();
        }

        public bool IsCompleted => _isCompleted();
        void IAwaiter.GetResult()
        {
            var unused = GetResult();
        }

        public T GetResult()
        {
            return _getResult();
        }
    }
}
