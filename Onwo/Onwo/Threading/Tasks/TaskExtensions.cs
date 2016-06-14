using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Onwo.Threading.Tasks
{
    public static class TaskExtensions
    {
        public static ConfiguredTaskAwaitable ConfigureAwait(this Task task)
        {
            return task.ConfigureAwait(false);
        }
        public static ConfiguredTaskAwaitable<T> ConfigureAwait<T>(this Task<T> task)
        {
            return task.ConfigureAwait(false);
        }

        public static async Task WaitUntilCancelled(this CancellationToken cancellation)
        {
            ManualResetEvent waitHandle = new ManualResetEvent(false);
            Action cancellationCallback = () =>
            {
                waitHandle.Set();
            };
            using (var registration = cancellation.Register(cancellationCallback, false))
            {
                await Task.Run(() => waitHandle.WaitOne(), cancellation);
            }
        }
    }
}
