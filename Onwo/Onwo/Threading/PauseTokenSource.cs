using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Onwo;
namespace Onwo.Threading
{
    public class PauseTokenSource:IDisposable
    {
        public delegate void PausedStatusEventHandler();

        public delegate void PauseStausChangedEventHandler(bool isPaused);

        public event PausedStatusEventHandler Paused;
        public event PausedStatusEventHandler Resumed;
        public event PauseStausChangedEventHandler PauseStatusChanged;
        private volatile TaskCompletionSource<bool> m_paused;
        public bool IsPaused
        {
            get { return m_paused != null; }
            set
            {
                if (value)
                    Pause();
                else Resume();
            }
        }
        public void Pause()
        {
            if (Interlocked.CompareExchange(ref m_paused, new TaskCompletionSource<bool>(), null)==null)
            {
                Paused?.Invoke();
                PauseStatusChanged?.Invoke(true);
            }
        }
        public void Resume()
        {
            while (true)
            {
                var tcs = m_paused;
                if (tcs == null) return;
                if (Interlocked.CompareExchange(ref m_paused, null, tcs) == tcs)
                {
                    tcs.SetResult(true);
                    Resumed?.Invoke();
                    PauseStatusChanged?.Invoke(false);
                    break;
                }
            }
        }
        public void Toggle()
        {
            IsPaused = !IsPaused;
        }
        public PauseToken Token => new PauseToken(this);

        internal async Task WaitWhilePausedAsync()
        {
            var cur = m_paused;
            if (cur == null)
                return;
            await cur.Task;
        }
        internal async Task WaitWhilePausedAsync(CancellationToken cancellation)
        {
            var cur = m_paused;
            if (cur == null)
                return;
            if (cur.Task.IsCompleted)
            {
                await cur.Task;
                return;
            }
            using (var waitHandle = new ManualResetEvent(false))
            {
                Action cancellationCallback = () =>
                {
                    waitHandle.Set();
                };
                // ReSharper disable AccessToDisposedClosure
                using (var registration = cancellation.Register(()=>waitHandle.Set(), false))
                {
                    try
                    {
                        var cancellationTask = Task.Run(() => waitHandle.WaitOne(), cancellation);
                        // ReSharper restore AccessToDisposedClosure
                        await Task.WhenAny(cur.Task, cancellationTask);
                    }
                    finally
                    {
                        //call set on waithandle to make sure the cancellation task completes 
                        //(otherwise it will just sit in the background waiting for a cancellation that may never happen)
                        waitHandle.Set();
                    }
                }
            }
                
        }
        public void RegisterCancellationToken(CancellationToken token)
        {
            var reg = token.Register(CancellationRequested);
            _cancellationTokenRegistrationList.Add(reg);
        }
        public void RegisterCancellationToken(CancellationToken token,Action onCancelledAction)
        {
            var reg = token.Register(() =>
            {
                CancellationRequested();
                onCancelledAction();
            });
            _cancellationTokenRegistrationList.Add(reg);
        }
        private readonly List<CancellationTokenRegistration> _cancellationTokenRegistrationList =
            new List<CancellationTokenRegistration>();
        private void CancellationRequested()
        {
            if (this.IsPaused)
                this.Resume();
            
            _cancellationTokenRegistrationList.ForEach(reg=>reg.Dispose());
            _cancellationTokenRegistrationList.Clear();
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
            if (this.IsPaused) Resume();
            _cancellationTokenRegistrationList.ForEach(reg => reg.Dispose());
            _cancellationTokenRegistrationList.Clear();
            PauseStatusChanged = null;
            Resumed = null;
            Paused = null;
        }
    }
}