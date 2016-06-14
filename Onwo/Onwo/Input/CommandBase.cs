using System;
using System.Threading;
using System.Windows.Input;

namespace Onwo.Input
{
    public abstract class CommandBase : ICommand,IDisposable 
    {
        public abstract bool CanExecute(object parameter);
        protected abstract void ExecuteOverride(object parameter);

        public void Execute(object parameter)
        {
            ExecuteOverride(parameter);
            Exectued?.Invoke(this,new EventArgs());
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public event EventHandler Exectued;
        protected void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
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
        }
    }
}