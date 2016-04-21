using System;
using System.Threading;
using System.Threading.Tasks;

namespace Onwo.Threading.Tasks
{
    public static class SchedulerExtensions
    {
        public static Task RunTask(this TaskScheduler scheduler, Action action)
        {
            return RunTask(scheduler, action, CancellationToken.None, TaskCreationOptions.None);
        }
        public static Task RunTask(this TaskScheduler scheduler, Action action, CancellationToken cancellationToken)
        {
            return RunTask(scheduler, action, cancellationToken, TaskCreationOptions.None);
        }
        public static Task RunTask(this TaskScheduler scheduler, Action action, TaskCreationOptions creationOptions)
        {
            return Task.Factory.StartNew(action, CancellationToken.None, creationOptions, scheduler);
        }
        public static Task RunTask(this TaskScheduler scheduler, Action action, CancellationToken cancellationToken, TaskCreationOptions creationOptions)
        {
            return Task.Factory.StartNew(action, cancellationToken, creationOptions, scheduler);
        }

        public static Task<TResult> RunTask<TResult>(this TaskScheduler scheduler, Func<TResult> func)
        {
            return RunTask(scheduler, func, CancellationToken.None, TaskCreationOptions.None);
        }
        public static Task<TResult> RunTask<TResult>(this TaskScheduler scheduler, Func<TResult> func, CancellationToken cancellationToken)
        {
            return RunTask(scheduler, func, cancellationToken, TaskCreationOptions.None);
        }
        public static Task<TResult> RunTask<TResult>(this TaskScheduler scheduler, Func<TResult> func, TaskCreationOptions creationOptions)
        {
            return Task.Factory.StartNew(func, CancellationToken.None, creationOptions, scheduler);
        }
        public static Task<TResult> RunTask<TResult>(this TaskScheduler scheduler, Func<TResult> func, CancellationToken cancellationToken, TaskCreationOptions creationOptions)
        {
            return Task.Factory.StartNew(func, cancellationToken, creationOptions, scheduler);
        }

        public static Task RunTask(this TaskScheduler scheduler, Action<object> action, object obj)
        {
            return RunTask(scheduler, action, obj, CancellationToken.None, TaskCreationOptions.None);
        }
        public static Task RunTask(this TaskScheduler scheduler, Action<object> action, object obj, CancellationToken cancellationToken)
        {
            return RunTask(scheduler, action, obj, cancellationToken, TaskCreationOptions.None);
        }
        public static Task RunTask(this TaskScheduler scheduler, Action<object> action, object obj, TaskCreationOptions creationOptions)
        {
            return Task.Factory.StartNew(action, obj, CancellationToken.None, creationOptions, scheduler);
        }
        public static Task RunTask(this TaskScheduler scheduler, Action<object> action, object obj, CancellationToken cancellationToken, TaskCreationOptions creationOptions)
        {
            return Task.Factory.StartNew(action, obj, cancellationToken, creationOptions, scheduler);
        }

        public static Task<TResult> RunTask<TResult>(this TaskScheduler scheduler, Func<object, TResult> func, object obj)
        {
            return RunTask(scheduler, func, obj, CancellationToken.None, TaskCreationOptions.None);
        }
        public static Task<TResult> RunTask<TResult>(this TaskScheduler scheduler, Func<object, TResult> func, object obj, CancellationToken cancellationToken)
        {
            return RunTask(scheduler, func, obj, cancellationToken, TaskCreationOptions.None);
        }
        public static Task<TResult> RunTask<TResult>(this TaskScheduler scheduler, Func<object, TResult> func, object obj, TaskCreationOptions creationOptions)
        {
            return Task.Factory.StartNew(func, obj, CancellationToken.None, creationOptions, scheduler);
        }
        public static Task<TResult> RunTask<TResult>(this TaskScheduler scheduler, Func<object, TResult> func, object obj, CancellationToken cancellationToken, TaskCreationOptions creationOptions)
        {
            return Task.Factory.StartNew(func, obj, cancellationToken, creationOptions, scheduler);
        }
    }
}
