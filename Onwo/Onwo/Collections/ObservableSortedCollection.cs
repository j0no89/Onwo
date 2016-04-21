namespace Onwo.Collections
{
   /* public class ObservableSortedCollection<T> : ObservableCollection<T>
    {
        private IComparer<T> _comparer;
        protected IComparer<T> _internalComparer;
        public IComparer<T> Comparer
        {
            get { return _comparer; }
            set
            {
                if (Equals(_comparer, value)) return;
                _comparer = value;
                setInternalComparer();
                OnPropertyChanged(nameof(Comparer));
                OnPropertyChanged(nameof(HasComparer));
                Resort();
            }
        }

        public delegate void ComparerChangedEventHandler(
            ObservableSortedCollection<T> sender);
        public event ComparerChangedEventHandler ComparerChanged;

        protected virtual void OnComparerChanged()
        {
            ComparerChanged?.Invoke(this);
        }
        public bool HasComparer => _comparer != null;
        public ObservableSortedCollection(IComparer<T> comparer = null) : base()
        {
            if (comparer == null)
                setInternalComparer();
            else
                Comparer = comparer;
        }
        public ObservableSortedCollection(IEnumerable<T> collection, IComparer<T> comparer = null) : base(collection)
        {
            if (comparer == null)
                setInternalComparer();
            else
                Comparer = comparer;
        }
        public ObservableSortedCollection(IList<T> list, IComparer<T> comparer = null) : base(list)
        {
            if (comparer == null)
                setInternalComparer();
            else
                Comparer = comparer;
        }

        public virtual void Resort()
        {
           
            if (this.Count <= 1)
                return;
            else if (this.Count == 2)
            {
                if (_internalComparer.Compare(this[0], this[1]) > 0)
                    this.Move(1, 0);
                return;
            }
            for (int cCount = 0; cCount < this.Count; cCount++)
            {
                T cItem = this[cCount];
                int index;
                for (index = 0; index < cCount; index++)
                {
                    int compare = _internalComparer.Compare(cItem, this[index]);
                    if (compare <= 0)
                        break;
                }
                if (index != cCount)
                    Move(cCount, index);
            }

        }
        private void setInternalComparer()
        {
            if (_comparer == null)
                _internalComparer = getDefaultComparer();
            else
                _internalComparer = _comparer;
        }
        private IComparer<T> getDefaultComparer()
        {
            var type = typeof(T);
            if (typeof(IComparable<T>).IsAssignableFrom(type))
            {
                return Comparer<T>.Create((item1, item2) =>
                {
                    var comp1 = (IComparable<T>)item1;
                    return comp1.CompareTo(item2);
                });
            }
            else if (typeof(IComparable).IsAssignableFrom(type))
            {
                return Comparer<T>.Create((item1, item2) =>
                {
                    var comp1 = (IComparable)item1;
                    return comp1.CompareTo(item2);
                });
            }
            return Comparer<T>.Default;
        }
        protected override void InsertItem(int index, T item)
        {
            int i;
            for (i = 0; i < this.Count; i++)
            {
                int compare = _internalComparer.Compare(item, this[i]);
                if (compare <= 0)
                {
                    break;
                }
            }
            base.InsertItem(i, item);
            OnItemAdded(item, i);
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            base.MoveItem(oldIndex, newIndex);
        }

        protected override void RemoveItem(int index)
        {
            T item = this[index];
            base.RemoveItem(index);
            OnItemRemoved(item, index);
        }

        protected override void SetItem(int index, T item)
        {
            var old = this[index];
            base.SetItem(index, item);
            OnItemChanged(old, item, index);

            int ind;
            for (ind = 0; ind < Count; ind++)
            {
                if (ind == index)
                    continue;
                int compare = _internalComparer.Compare(item, this[index]);
                if (compare <= 0)
                    break;
            }
            if (index == ind)
                return;
            this.Move(index, ind);
        }
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        public void AddRange(IEnumerable<T> newItems)
        {
            if (newItems == null)
                return;
            foreach (var item in newItems)
                Add(item);
        }
        #region Events
        public delegate void ItemmChangedEventHandler(ICollection<T> sender, T oldItem, T newItem, int index);
        public event ItemmChangedEventHandler ItemChanged;
        protected virtual void OnItemChanged(T oldItem, T newItem, int index)
        {
            ItemChanged?.Invoke(this, oldItem, newItem, index);
            OnItemDetached(oldItem,index);
            OnItemAttached(newItem,index);
        }
        public delegate void ItemmAddedEventHandler(ICollection<T> sender, T newItem, int index);
        public event ItemmAddedEventHandler ItemAdded;
        protected virtual void OnItemAdded(T item, int index)
        {
            ItemAdded?.Invoke(this, item, index);
            OnItemAttached(item, index);
        }
        public delegate void ItemmRemovedEventHandler(ICollection<T> sender, T oldItem, int index);
        public event ItemmRemovedEventHandler ItemRemoved;
        protected virtual void OnItemRemoved(T item)
        {
            OnItemRemoved(item, -1);
        }
        protected virtual void OnItemRemoved(T item, int index)
        {
            ItemRemoved?.Invoke(this, item, index);
            OnItemDetached(item, index);
        }
        public delegate void ItemAttachedEventHandler(ICollection<T> sender, T newItem, int index);
        public event ItemAttachedEventHandler ItemAttached;
        protected virtual void OnItemAttached(T newItem,int index)
        {
            ItemAttached?.Invoke(this,newItem,index);
        }
        public delegate void ItemDetachedEventHandler(ICollection<T> sender, T oldItem, int index);
        public event ItemDetachedEventHandler ItemDetached;
        protected virtual void OnItemDetached(T oldItem, int index)
        {
            ItemDetached?.Invoke(this, oldItem, index);
        }
        
        #endregion
    }*/
}
