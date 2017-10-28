using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onwo
{
    public static class DisposableExt
    {
        public static void DisposeAll(this IEnumerable<IDisposable> list)
        {
            if (list == null) return;
            foreach (var disposable in list)
            {
                disposable.Dispose();
            }
        }
    }
}
