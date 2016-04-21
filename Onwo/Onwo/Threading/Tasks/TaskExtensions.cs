using System.Runtime.CompilerServices;
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
    }
}
