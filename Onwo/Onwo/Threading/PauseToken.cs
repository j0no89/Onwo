using System.Threading;
using System.Threading.Tasks;

namespace Onwo.Threading
{
    public class PauseToken
    {
        public static PauseToken None => new PauseToken(null);
        private readonly PauseTokenSource _source;
        public PauseTokenSource Source => _source;
        internal PauseToken(PauseTokenSource source)
        {
            _source = source;
        }

        public bool IsPaused => _source?.IsPaused ?? false;

        public async Task WaitWhilePausedAsync()
        {
            if (_source == null) return;
            await Source.WaitWhilePausedAsync();
        }
        public async Task WaitWhilePausedAsync(CancellationToken cancellation)
        {
            if (_source == null) return;
            await Source.WaitWhilePausedAsync(cancellation);
        }
        public event PauseTokenSource.PausedStatusEventHandler OnPaused
        {
            add { _source.Paused += value; }
            remove { _source.Paused -= value; }
        }
        public event PauseTokenSource.PausedStatusEventHandler OnResume
        {
            add { _source.Resumed += value; }
            remove { _source.Resumed -= value; }
        }
        public event PauseTokenSource.PauseStausChangedEventHandler OnStatusChanged
        {
            add { _source.PauseStatusChanged += value; }
            remove { _source.PauseStatusChanged -= value; }
        }
    }
}
