using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Onwo
{
    public class AsyncAuto<T>:Auto<Task<T>>
    {
        public AsyncAuto(Func<Task<T>> initializationFunction, LazyThreadSafetyMode safetyMode = LazyThreadSafetyMode.ExecutionAndPublication) 
            : base(()=>Task.Run(initializationFunction), safetyMode)
        {
        }
        public AsyncAuto(Func<T> initializationFunction, LazyThreadSafetyMode safetyMode = LazyThreadSafetyMode.ExecutionAndPublication)
           : base(()=>Task.Run(initializationFunction), safetyMode)
        {
        }
        public void Start()
        {
            var temp = this.Value;
        }

        public TaskAwaiter<T> GetAwaiter()
        {
            return this.Value.GetAwaiter();
        } 
    }
}
