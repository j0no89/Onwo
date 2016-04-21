using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using Onwo.Threading.Tasks;

namespace Onwo.Input
{
    public class AsyncCommand : AsyncCommandBase, INotifyPropertyChanged
    {
        private readonly Func<CancellationToken, Task> _command;
        private readonly CancelAsyncCommand _cancelCommand;
        private NotifyTaskCompletion _execution;
        //private NotifyTaskCompletion<TResult> _execution;
        public AsyncCommand(Func<Task> command) : this(token => command())
        {
        }
        public AsyncCommand(Func<CancellationToken, Task> command)
        {
            _command = command;
            _cancelCommand = new CancelAsyncCommand();
            _execution=new NotifyTaskCompletion(Task.CompletedTask);
        }
        public override bool CanExecute(object parameter)
        {
            return Execution == null || Execution.IsCompleted;
        }
        public override async Task ExecuteAsync(object parameter)
        {
            _cancelCommand.NotifyCommandStarting();
            Execution = new NotifyTaskCompletion(_command(_cancelCommand.Token));
            RaiseCanExecuteChanged();
            await Execution.TaskCompletion.ConfigureAwait(false);
            _cancelCommand.NotifyCommandFinished();
            RaiseCanExecuteChanged();
        }
        public ICommand CancelCommand => _cancelCommand;

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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class AsyncCommand<TResult> : AsyncCommandBase, INotifyPropertyChanged
    {
        private readonly Func<CancellationToken, Task<TResult>> _command;
        private readonly CancelAsyncCommand _cancelCommand;
        private NotifyTaskCompletion<TResult> _execution;
        //private NotifyTaskCompletion<TResult> _execution;
        public AsyncCommand(Func<Task<TResult>> command):this(token=>command())
        {
        }
        public AsyncCommand(Func<CancellationToken, Task<TResult>> command)
        {
            _command = command;
            _cancelCommand = new CancelAsyncCommand();
            _execution=new NotifyTaskCompletion<TResult>(Task.FromResult(default(TResult)));
        }
        public override bool CanExecute(object parameter)
        {
            return Execution == null || Execution.IsCompleted;
        }
        public override async Task ExecuteAsync(object parameter)
        {
            _cancelCommand.NotifyCommandStarting();
            Execution = new NotifyTaskCompletion<TResult>(_command(_cancelCommand.Token));
            RaiseCanExecuteChanged();
            await Execution.TaskCompletion.ConfigureAwait(false);
            _cancelCommand.NotifyCommandFinished();
            RaiseCanExecuteChanged();
        }
        public ICommand CancelCommand => _cancelCommand;

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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}