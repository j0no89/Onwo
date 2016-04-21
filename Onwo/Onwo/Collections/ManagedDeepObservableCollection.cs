using System.Collections.Generic;
using System.ComponentModel;

namespace Onwo.Collections
{
    public class ManagedDeepObservableCollection<T> : ObservableSortedCollection<T>
        where T : class, INotifyPropertyChanged
    {
        public ManagedDeepObservableCollection(IEnumerable<T> collection, IComparer<T> comparer = null) :
            base(collection, comparer)
        {
        }
        public ManagedDeepObservableCollection(IList<T> list, IComparer<T> comparer = null)
            : base(list, comparer)
        {
        }
        public delegate void ItemPropertyChangedEventHandler(object sender, string propertyName);

        public event ItemPropertyChangedEventHandler ItemPropertyChanged;
        protected virtual void OnItemPropertyChanged(T sender, string propertyName)
        {
            ItemPropertyChanged?.Invoke(sender, propertyName);
        }
        protected virtual void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnItemPropertyChanged(sender as T, e?.PropertyName);
        }
        protected override void OnItemAdded(T item, int index)
        {
            item.PropertyChanged += OnItemPropertyChanged;
            base.OnItemAdded(item, index);
        }

        protected override void OnItemRemoved(T item)
        {
            item.PropertyChanged -= OnItemPropertyChanged;
            base.OnItemRemoved(item);
        }

        protected override void OnItemChanged(T oldItem, T newItem, int index)
        {
            oldItem.PropertyChanged -= OnItemPropertyChanged;
            newItem.PropertyChanged += OnItemPropertyChanged;
            base.OnItemChanged(oldItem, newItem, index);
        }
    }
}
