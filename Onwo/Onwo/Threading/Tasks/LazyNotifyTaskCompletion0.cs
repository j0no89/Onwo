using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Onwo.Threading.Tasks
{
    /// <summary>
    /// Watches a task and raises property-changed notifications when the task completes.
    /// </summary>
    public class LazyNotifyTaskCompletion0:INotifyTaskCompletion
    {
        private readonly AsyncAuto<object> lazy;
        public LazyNotifyTaskCompletion0(Func<Task> task, bool lazyLoad = true)
        {
            lazy = new AsyncAuto<object>(async () =>
            {
                await task();
                return null;
            });
            lazy.IsValueCreatedChanged += Lazy_IsValueCreatedChanged;
            _taskCompletedSource = new TaskCompletionSource<object>();
            _notificationsCompletedSource = new TaskCompletionSource<object>();
            if (!lazyLoad)
            {
                lazy.Start();
            }
        }
        public LazyNotifyTaskCompletion0(Action func, bool lazyLoad = true) 
            : this(() => System.Threading.Tasks.Task.Run(func), lazyLoad)
        { }
        private void Lazy_IsValueCreatedChanged(Auto<Task<object>> sender)
        {
            if (!lazy.IsValueCreated)
            {
                OnPropertyChanged(nameof(IsTaskCreated));
                return;
            }
            var task = lazy.Value;
            if (task.IsCompleted)
            {
                _taskCompletedSource.SetResult(null);
                _notificationsCompletedSource.SetResult(null);
            }
            else
            {
                var scheduler = (SynchronizationContext.Current == null) ? TaskScheduler.Current : TaskScheduler.FromCurrentSynchronizationContext();
                task.ContinueWith(TaskCompletedAction,
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    scheduler);
            }
            OnPropertyChanged(nameof(IsTaskCreated));
        }
        private void TaskCompletedAction(Task task)
        {
            if (!lazy.IsValueCreated)
                return;
            if (!ReferenceEquals(task, Task))
                return;
            _taskCompletedSource.SetResult(null);
            var propertyChanged = PropertyChanged;
            if (propertyChanged == null)
            {
                _notificationsCompletedSource.SetResult(null);
                return;
            }
            propertyChanged(this, new PropertyChangedEventArgs(nameof(Status)));
            propertyChanged(this, new PropertyChangedEventArgs(nameof(IsCompleted)));
            propertyChanged(this, new PropertyChangedEventArgs(nameof(IsNotCompleted)));
            if (task.IsCanceled)
            {
                propertyChanged(this, new PropertyChangedEventArgs(nameof(IsCanceled)));
            }
            else if (task.IsFaulted)
            {
                propertyChanged(this, new PropertyChangedEventArgs(nameof(IsFaulted)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(Exception)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(InnerException)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(ErrorMessage)));
            }
            else
            {
                propertyChanged(this, new PropertyChangedEventArgs(nameof(IsSuccessfullyCompleted)));
            }
            _notificationsCompletedSource.SetResult(null);
        }
        TaskAwaiter INotifyTaskCompletion.GetAwaiter()
        {
            Task task = this.Task;
            return task.GetAwaiter();
        }
        public ConfiguredTaskAwaitable ConfigureAwait(bool continueOnCapturedContext = false)
        {
            return this.Task.ConfigureAwait(continueOnCapturedContext);
        }
        public TaskAwaiter GetAwaiter()
        {
            return Task.GetAwaiter();
        }
        public void Start()
        {
            lazy.Start();
        }
        public bool IsTaskCreated => lazy.IsValueCreated;
        public void Reset()
        {
            if (!lazy.IsValueCreated)
                return;
            var oldTask = lazy.Value;
            if (!oldTask.IsCompleted)
                lazy.ResetValue();
            _taskCompletedSource = new TaskCompletionSource<object>();
            _notificationsCompletedSource = new TaskCompletionSource<object>();
            var propertyChanged = PropertyChanged;
            if (propertyChanged == null)
                return;
            propertyChanged(this, new PropertyChangedEventArgs(nameof(Status)));
            propertyChanged(this, new PropertyChangedEventArgs(nameof(IsCompleted)));
            propertyChanged(this, new PropertyChangedEventArgs(nameof(IsNotCompleted)));
            if (oldTask.IsCanceled)
            {
                propertyChanged(this, new PropertyChangedEventArgs(nameof(IsCanceled)));
            }
            else if (oldTask.IsFaulted)
            {
                propertyChanged(this, new PropertyChangedEventArgs(nameof(IsFaulted)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(Exception)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(InnerException)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(ErrorMessage)));
            }
            else
            {
                propertyChanged(this, new PropertyChangedEventArgs(nameof(IsSuccessfullyCompleted)));
            }
        }
        public Task Task => lazy.Value;
        Task INotifyTaskCompletion.Task => Task;
        public Task TaskCompleted => _taskCompletedSource.Task;
        private TaskCompletionSource<object> _taskCompletedSource;
        public Task NotificationsCompleted => _notificationsCompletedSource.Task;
        private TaskCompletionSource<object> _notificationsCompletedSource;
        public TaskStatus Status => Task.Status;
        public bool IsCompleted => Task.IsCompleted;
        public bool IsNotCompleted => !Task.IsCompleted;
        public bool IsSuccessfullyCompleted => Task.Status == TaskStatus.RanToCompletion;
        public bool IsCanceled => Task.IsCanceled;
        public bool IsFaulted => Task.IsFaulted;
        public AggregateException Exception => Task.Exception;
        public Exception InnerException => Exception?.InnerException;
        public string ErrorMessage => InnerException?.Message;

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
    public class LazyNotifyTaskCompletion<TResult> : ILazyNotifyTaskCompletion<TResult>
    {
        private readonly AsyncAuto<TResult> lazy;
        public LazyNotifyTaskCompletion(Func<Task<TResult>> task, bool lazyLoad = true)
        {
            lazy=new AsyncAuto<TResult>(task);
            lazy.IsValueCreatedChanged += Lazy_IsValueCreatedChanged;
            _taskCompletedSource=new TaskCompletionSource<object>();
            _notificationsCompletedSource=new TaskCompletionSource<object>();
            if (!lazyLoad)
            {
                lazy.Start();
            }
        }
        public LazyNotifyTaskCompletion(Func<TResult> func, bool lazyLoad = true) 
            : this(() => System.Threading.Tasks.Task.Run(func), lazyLoad)
        { }
        private void Lazy_IsValueCreatedChanged(Auto<Task<TResult>> sender)
        {
            if (!lazy.IsValueCreated)
            {
                OnPropertyChanged(nameof(IsTaskCreated));
                return;
            }
            var task = lazy.Value;
            if (task.IsCompleted)
            {
                _taskCompletedSource.SetResult(null);
                _notificationsCompletedSource.SetResult(null);
            }
            else
            {
                var scheduler = (SynchronizationContext.Current == null) ? TaskScheduler.Current : TaskScheduler.FromCurrentSynchronizationContext();
                task.ContinueWith(TaskCompletedAction,
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    scheduler);
            }
            OnPropertyChanged(nameof(IsTaskCreated));
        }
        private void TaskCompletedAction(Task task)
        {
            if (!lazy.IsValueCreated)
                return;
            if (!ReferenceEquals(task, Task))
                return;
            _taskCompletedSource.SetResult(null);
            var propertyChanged = PropertyChanged;
            if (propertyChanged == null)
            {
                _notificationsCompletedSource.SetResult(null);
                return;
            }
            propertyChanged(this, new PropertyChangedEventArgs(nameof(Status)));
            propertyChanged(this, new PropertyChangedEventArgs(nameof(IsCompleted)));
            propertyChanged(this, new PropertyChangedEventArgs(nameof(IsNotCompleted)));
            if (task.IsCanceled)
            {
                propertyChanged(this, new PropertyChangedEventArgs(nameof(IsCanceled)));
            }
            else if (task.IsFaulted)
            {
                propertyChanged(this, new PropertyChangedEventArgs(nameof(IsFaulted)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(Exception)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(InnerException)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(ErrorMessage)));
            }
            else
            {
                propertyChanged(this, new PropertyChangedEventArgs(nameof(IsSuccessfullyCompleted)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(Result)));
            }
            _notificationsCompletedSource.SetResult(null);
        }
        TaskAwaiter INotifyTaskCompletion.GetAwaiter()
        {
            Task task = this.Task;
            return task.GetAwaiter();
        }
        public ConfiguredTaskAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext=false)
        {
            return this.Task.ConfigureAwait(continueOnCapturedContext);
        }
        ConfiguredTaskAwaitable INotifyTaskCompletion.ConfigureAwait(bool continueOnCapturedContext)
        {
            Task task = this.Task;
            return task.ConfigureAwait(continueOnCapturedContext);
        }
        public TaskAwaiter<TResult> GetAwaiter()
        {
            return Task.GetAwaiter();
        }
        public void Start()
        {
            lazy.Start();
        }
        public bool IsTaskCreated => lazy.IsValueCreated;
        public void Reset()
        {
            if (!lazy.IsValueCreated)
                return;
            var oldTask = lazy.Value;
            if (!oldTask.IsCompleted)
            lazy.ResetValue();
            _taskCompletedSource=new TaskCompletionSource<object>();
            _notificationsCompletedSource=new TaskCompletionSource<object>();
            var propertyChanged = PropertyChanged;
            if (propertyChanged == null)
                return;
            propertyChanged(this, new PropertyChangedEventArgs(nameof(Status)));
            propertyChanged(this, new PropertyChangedEventArgs(nameof(IsCompleted)));
            propertyChanged(this, new PropertyChangedEventArgs(nameof(IsNotCompleted)));
            if (oldTask.IsCanceled)
            {
                propertyChanged(this, new PropertyChangedEventArgs(nameof(IsCanceled)));
            }
            else if (oldTask.IsFaulted)
            {
                propertyChanged(this, new PropertyChangedEventArgs(nameof(IsFaulted)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(Exception)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(InnerException)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(ErrorMessage)));
            }
            else
            {
                propertyChanged(this, new PropertyChangedEventArgs(nameof(IsSuccessfullyCompleted)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(Result)));
            }
        }
        public Task<TResult> Task => lazy.Value;
        Task INotifyTaskCompletion.Task => Task;
        public Task TaskCompleted => _taskCompletedSource.Task;
        private TaskCompletionSource<object> _taskCompletedSource;
        public Task NotificationsCompleted => _notificationsCompletedSource.Task;
        private TaskCompletionSource<object> _notificationsCompletedSource;
        public TResult Result => (Task.Status == TaskStatus.RanToCompletion) ? Task.Result : default(TResult);
        public TaskStatus Status => Task.Status;
        public bool IsCompleted => Task.IsCompleted;
        public bool IsNotCompleted => !Task.IsCompleted;
        public bool IsSuccessfullyCompleted => Task.Status == TaskStatus.RanToCompletion;
        public bool IsCanceled => Task.IsCanceled;
        public bool IsFaulted => Task.IsFaulted;
        public AggregateException Exception => Task.Exception;
        public Exception InnerException => Exception?.InnerException;
        public string ErrorMessage => InnerException?.Message;

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class NotifyTaskCompletion2<TResult> : INotifyTaskCompletion<TResult>
    {
        public NotifyTaskCompletion2(Task<TResult> task)
        {
            Task = task;
            if (task.IsCompleted)
            {
                TaskCompleted = System.Threading.Tasks.Task.CompletedTask;
                return;
            }
            var scheduler = (SynchronizationContext.Current == null) ? TaskScheduler.Current : TaskScheduler.FromCurrentSynchronizationContext();
            TaskCompleted = task.ContinueWith(t => TaskCompletedAction(t),
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                scheduler);
        }
        private void TaskCompletedAction(Task task)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged == null)
                return;
            propertyChanged(this, new PropertyChangedEventArgs(nameof(Status)));
            propertyChanged(this, new PropertyChangedEventArgs(nameof(IsCompleted)));
            propertyChanged(this, new PropertyChangedEventArgs(nameof(IsNotCompleted)));
            if (task.IsCanceled)
            {
                propertyChanged(this, new PropertyChangedEventArgs(nameof(IsCanceled)));
            }
            else if (task.IsFaulted)
            {
                propertyChanged(this, new PropertyChangedEventArgs(nameof(IsFaulted)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(Exception)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(InnerException)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(ErrorMessage)));
            }
            else
            {
                propertyChanged(this, new PropertyChangedEventArgs(nameof(IsSuccessfullyCompleted)));
                propertyChanged(this, new PropertyChangedEventArgs(nameof(Result)));
            }
        }
        TaskAwaiter INotifyTaskCompletion.GetAwaiter()
        {
            Task t = Task;
            return t.GetAwaiter();
        }

        public ConfiguredTaskAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext)
        {
            return this.Task.ConfigureAwait(continueOnCapturedContext);
        }

        ConfiguredTaskAwaitable INotifyTaskCompletion.ConfigureAwait(bool continueOnCapturedContext)
        {
            Task task = this.Task;
            return task.ConfigureAwait(continueOnCapturedContext);
        }

        public TaskAwaiter<TResult> GetAwaiter()
        {
            return Task.GetAwaiter();
        }
        public Task<TResult> Task { get; private set; }
        Task INotifyTaskCompletion.Task => Task;
        public Task TaskCompleted { get; private set; }
        public Task NotificationsCompleted { get; }
        public TResult Result => (Task.Status == TaskStatus.RanToCompletion) ? Task.Result : default(TResult);
        public TaskStatus Status => Task.Status;
        public bool IsCompleted => Task.IsCompleted;
        public bool IsNotCompleted => !Task.IsCompleted;
        public bool IsSuccessfullyCompleted => Task.Status == TaskStatus.RanToCompletion;
        public bool IsCanceled => Task.IsCanceled;
        public bool IsFaulted => Task.IsFaulted;
        public AggregateException Exception => Task.Exception;
        public Exception InnerException => Exception?.InnerException;
        public string ErrorMessage => InnerException?.Message;

        public event PropertyChangedEventHandler PropertyChanged;
    }
    public class LazyNotifyTaskCompletion2<T> : Auto<NotifyTaskCompletion<T>>, INotifyPropertyChanged
    {
        public LazyNotifyTaskCompletion2(Func<Task<T>> initializationFunction, LazyThreadSafetyMode safetyMode = LazyThreadSafetyMode.ExecutionAndPublication)
            : base(() => new NotifyTaskCompletion<T>(Task.Run(initializationFunction)), safetyMode)
        {
        }
        public LazyNotifyTaskCompletion2(Func<T> initializationFunction, LazyThreadSafetyMode safetyMode = LazyThreadSafetyMode.ExecutionAndPublication)
           : base(() => new NotifyTaskCompletion<T>(Task.Run(initializationFunction)), safetyMode)
        {

        }

        protected override void OnIsValueChangedOverride()
        {
            OnPropertyChanged(nameof(IsValueCreated));
            OnPropertyChanged(nameof(Value));
            base.OnIsValueChangedOverride();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
