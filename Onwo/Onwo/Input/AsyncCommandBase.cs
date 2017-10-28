using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation.Peers;
using System.Windows.Input;
using JetBrains.Annotations;
using Onwo.Threading;
using Onwo.Threading.Tasks;

namespace Onwo.Input
{
    public abstract class AsyncCommandBase : IAsyncCommand,IDisposable
    {
        
        protected readonly CancelAsyncCommand _cancelCommand;
        protected readonly PauseAsyncCommand _pauseCommand;
        public ICommand CancelCommand => _cancelCommand;
        public ICommand PauseCommand => _pauseCommand;
        public bool IsPaused => _pauseCommand.IsPaused;
        public PauseToken GetPauseToken() => _pauseCommand.Token;

        public bool CanPause
        {
            get { return _pauseCommand.CanPause; }
            set
            {
                if (value == _pauseCommand.CanPause) return;
                _pauseCommand.CanPause = value;
                OnPropertyChanged();
            }
        }
        public bool CanCancel
        {
            get { return _cancelCommand.CanCancel; }
            set
            {
                if (value == _cancelCommand.CanCancel) return;
                _cancelCommand.CanCancel = value;
                OnPropertyChanged();
            }
        }

        protected AsyncCommandBase()
        {
            _cancelCommand=new CancelAsyncCommand();
            _pauseCommand=new PauseAsyncCommand();
            //_pauseCommand.Exectued += _pauseCommand_Exectued;
            _pauseCommand.Token.OnStatusChanged += PauseToken_OnStatusChanged;
            _pauseCommand.RegisterCancellationToken(_cancelCommand.Token);
        }

        private void PauseToken_OnStatusChanged(bool isPaused)
        {
            OnPropertyChanged(nameof(IsPaused));
        }

        private void _pauseCommand_Exectued(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(IsPaused));
        }
        public abstract bool CanExecute(object parameter);
        public abstract Task ExecuteAsync(object parameter);
        public async void Execute(object parameter)
        {
            await ExecuteAsync(parameter).ConfigureAwait(false);
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        protected void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
        protected sealed class CancelAsyncCommand : CommandBase,IDisposable
        {
            private CancellationTokenSource _cts = new CancellationTokenSource();
            private bool _commandExecuting;
            public CancellationToken Token => _cts.Token;

            public bool CanCancel
            {
                get { return _canCancel; }
                set
                {
                    if (_canCancel == value) return;
                    _canCancel = value;
                    RaiseCanExecuteChanged();
                }
            }
            public CancelAsyncCommand() : this(true) { }

            public CancelAsyncCommand(bool canCancel)
            {
                CanCancel = canCancel;
            }
            public void NotifyCommandStarting()
            {
                _commandExecuting = true;
                if (!_cts.IsCancellationRequested)
                    return;
                _cts = new CancellationTokenSource();
                RaiseCanExecuteChanged();
            }
            public void NotifyCommandFinished()
            {
                _commandExecuting = false;
                RaiseCanExecuteChanged();
            }
            public override bool CanExecute(object parameter)
            {
                return _canCancel && _commandExecuting && !_cts.IsCancellationRequested;
            }

            protected override void ExecuteOverride(object parameter)
            {
                _cts.Cancel();
                RaiseCanExecuteChanged();
            }
            private Bool _isDisposed = Bool.False;
            private bool _canCancel;

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (!disposing) return;
                if (Interlocked.CompareExchange(ref _isDisposed, Bool.True, Bool.False))
                    return;
                base.Dispose();
                _cts.Dispose();
            }
        }
        protected sealed class PauseAsyncCommand : CommandBase,IDisposable
        {
            private bool _canPause;
            public bool CanPause
            {
                get { return _canPause; }
                set
                {
                    if (_canPause == value) return;
                    _canPause = value;
                    RaiseCanExecuteChanged();
                }
            }
            public PauseAsyncCommand() : this(true) { }
            public PauseAsyncCommand(bool canPause)
            {
                CanPause = canPause;
            }
            private PauseTokenSource _pts = new PauseTokenSource();
            private bool _commandExecuting;
            public PauseToken Token { get { return _pts.Token; } }
            public bool IsPaused => _pts.IsPaused;
            public void NotifyCommandStarting()
            {
                _commandExecuting = true;
                RaiseCanExecuteChanged();
            }
            public void NotifyCommandFinished()
            {
                _commandExecuting = false;
                if (IsPaused)
                    _pts.Resume();
                RaiseCanExecuteChanged();
            }
            public override bool CanExecute(object parameter)
            {
                return CanPause && _commandExecuting;
            }

            protected override void ExecuteOverride(object parameter)
            {
                _pts.IsPaused = !_pts.IsPaused;
            }

            public void RegisterCancellationToken(CancellationToken cancellationToken)
            {
                _pts.RegisterCancellationToken(cancellationToken);
            }
            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private Bool _isDisposed = Bool.False;
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (!disposing) return;
                if (Interlocked.CompareExchange(ref _isDisposed, Bool.True, Bool.False))
                    return;
                base.Dispose();
                _pts.Dispose();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Bool _isDisposed = Bool.False;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing) return;
            if (Interlocked.CompareExchange(ref _isDisposed, Bool.True, Bool.False))
                return;
            //base.Dispose();
            _pauseCommand.Token.OnStatusChanged -= PauseToken_OnStatusChanged;
            _cancelCommand.Dispose();
            _pauseCommand.Dispose();
        }
    }
}
