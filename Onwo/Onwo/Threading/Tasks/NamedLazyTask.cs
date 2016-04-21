using System;
using System.Threading.Tasks;

namespace Onwo.Threading.Tasks
{
    public class NamedLazyTask : LazyNotifyTaskCompletion0,INamedLazyTask
    {
        public NamedLazyTask(Func<Task> task, string name = "", bool lazyLoad = true)
            : base(task, lazyLoad)
        {
            this.Name = name;
        }
        public NamedLazyTask(Action func, string name = "", bool lazyLoad = true)
            : this(() => System.Threading.Tasks.Task.Run(func), name, lazyLoad)
        { }
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }
    }
    public class NamedLazyTask<TResult>:LazyNotifyTaskCompletion<TResult>,INamedLazyTask<TResult>
    {
        public static NamedLazyTask<TResult> FromTaskCompletionSource(TaskCompletionSource<TResult> tcs,string name="")
        {
            return new NamedLazyTask<TResult>(() => tcs.Task, name, false);
        }
        private string _name;

        public NamedLazyTask(Func<Task<TResult>> task, string name = "", bool lazyLoad = true)
            : base(task, lazyLoad)
        {
            this.Name = name;
        }
        public NamedLazyTask(Func<TResult> func, string name = "", bool lazyLoad = true) 
            : this(() => System.Threading.Tasks.Task.Run(func), name, lazyLoad)
        { }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }
    }
}
