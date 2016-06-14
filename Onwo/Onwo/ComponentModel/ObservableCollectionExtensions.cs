using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Data;
using Onwo.Threading.Tasks;

namespace Onwo.ComponentModel
{
    public static class ObservableCollectionExtensions
    {
        public static Task<object> EnableCollectionSynchronisation<T>(this ObservableCollection<T> collection,
            TaskScheduler scheduler)
        {
            return scheduler.RunTask(() =>
            {
                var lockObj = new object();
                BindingOperations.EnableCollectionSynchronization(collection, lockObj);
                return lockObj;
            });
        }

        public static object EnableCollectionSynchronisation<T>(this ObservableCollection<T> collection)
        {
            object lockObj = new object();
            BindingOperations.EnableCollectionSynchronization(collection, lockObj);
            return lockObj;
        }
    }
}
