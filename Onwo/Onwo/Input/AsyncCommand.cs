using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using Onwo.Threading;
using Onwo.Threading.Tasks;

namespace Onwo.Input
{
    public class AsyncCommand : AsyncCommandBase, INotifyPropertyChanged
    {
        protected readonly Func<CancellationToken, PauseToken, Task> _command;
        private NotifyTaskCompletion _execution;
        //private NotifyTaskCompletion<TResult> _execution;
        public AsyncCommand(Func<Task> command) : this((cancel, pause) => command())
        {
        }
        public AsyncCommand(Func<CancellationToken, Task> command)
            : this((cancel, pause) => command(cancel))
        { }
        public AsyncCommand(Func<CancellationToken, PauseToken, Task> command)
            :base()
        {
            _command = command;
            _execution=new NotifyTaskCompletion(Task.CompletedTask);
        }
        

        public override bool CanExecute(object parameter)
        {
            return Execution == null || Execution.IsCompleted;
        }
        public override async Task ExecuteAsync(object parameter)
        {
            _cancelCommand.NotifyCommandStarting();
            _pauseCommand.NotifyCommandStarting();
            Execution = new NotifyTaskCompletion(_command(_cancelCommand.Token,_pauseCommand.Token));
            RaiseCanExecuteChanged();
            await Execution.TaskCompletion.ConfigureAwait(false);
            _cancelCommand.NotifyCommandFinished();
            _pauseCommand.NotifyCommandFinished();
            RaiseCanExecuteChanged();
        }
        public NotifyTaskCompletion Execution
        {
            get { return _execution; }
            private set
            {
                if (Equals(value, _execution)) return;
                _execution = value;
                OnPropertyChanged();
            }
        }
    }
    public class AsyncCommand<TResult> : AsyncCommandBase, INotifyPropertyChanged
    {
        protected readonly Func<CancellationToken, PauseToken, Task<TResult>> _command;
        private NotifyTaskCompletion<TResult> _execution;
        public AsyncCommand(Func<Task<TResult>> command):this((cancel, pause) => command())

        {
        }
        public AsyncCommand(Func<CancellationToken, Task<TResult>> command)
            :this((cancel,pause)=>command(cancel))
        {
            
        }
        public AsyncCommand(Func<CancellationToken, PauseToken, Task<TResult>> command)
        {
            _command = command;
            _execution=new NotifyTaskCompletion<TResult>(Task.FromResult(default(TResult)));
        }
        public override bool CanExecute(object parameter)
        {
            return Execution == null || Execution.IsCompleted;
        }
        public override async Task ExecuteAsync(object parameter)
        {
            _cancelCommand.NotifyCommandStarting();
            _pauseCommand.NotifyCommandStarting();
            Execution = new NotifyTaskCompletion<TResult>(_command(_cancelCommand.Token,_pauseCommand.Token));
            RaiseCanExecuteChanged();
            await Execution.TaskCompletion.ConfigureAwait(false);
            _cancelCommand.NotifyCommandFinished();
            _pauseCommand.NotifyCommandFinished();
            RaiseCanExecuteChanged();
        }
        public NotifyTaskCompletion<TResult> Execution
        {
            get { return _execution; }
            private set
            {
                if (Equals(value, _execution)) return;
                _execution = value;
                OnPropertyChanged();
            }
        }
    }
}