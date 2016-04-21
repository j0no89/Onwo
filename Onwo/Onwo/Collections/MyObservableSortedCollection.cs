using System;
using System.Collections.Generic;

namespace Onwo.Collections
{
    public class ObservableSortedCollection<T> : MyObservableCollection<T>
    {
        public static readonly MyLazy<IComparer<T>> DefaultComparer =new MyLazy<IComparer<T>>(() => Comparer<T>.Default); 
        private IComparer<T> _comparer;

        public IComparer<T> Comparer
        {
            get { return _comparer; }
            set
            {
                if (Equals(value, _comparer)) return;
                var old = _comparer;
                _comparer = value;
                OnPropertyChanged(nameof(Comparer));
                OnComparerChanged(old);
                Resort();
            }
        }

        protected IComparer<T> InternalComparer
        {
            get
            {
                if (Comparer == null)
                    return DefaultComparer.Value;
                return Comparer;
            }
        }// => Comparer ?? DefaultComparer.Value;

        public delegate void ComparerChangedEventHandler(ObservableSortedCollection<T> sender, IComparer<T> oldComparer);
        public event ComparerChangedEventHandler ComparerChanged;
        protected virtual void OnComparerChanged(IComparer<T> oldComparer)
        {
            ComparerChanged?.Invoke(this,oldComparer);
        }
        public ObservableSortedCollection() : this(null)
        {
        }
        public ObservableSortedCollection(IComparer<T> comparer, bool useSorting=true) : this(null,comparer,useSorting)
        { }
        public ObservableSortedCollection(IEnumerable<T> items,IComparer<T> comparer, bool useSorting=true) : base()
        {
            this.Comparer = comparer;
            this.UseSorting = useSorting;
            this.AddRange(items);
        }
        public virtual void Resort()
        {
            if (this.Count <= 1)
                return;
            else if (this.Count == 2)
            {
                if (InternalComparer.Compare(this[0], this[1]) > 0)
                    base.MoveItem(1, 0);
                return;
            }
            for (int cCount = 0; cCount < this.Count; cCount++)
            {
                T cItem = this[cCount];
                int index;
                for (index = 0; index < cCount; index++)
                {
                    int compare = InternalComparer.Compare(cItem, this[index]);
                    if (compare <= 0)
                        break;
                }
                if (index != cCount)
                    base.MoveItem(cCount, index);
            }

        }

        public virtual void ResortItem(int index)
        {
            int ind;
            var item = this[index];
            for (ind = 0; ind < this.Count; ind++)
            {
                if (ind == index)
                    continue;
                int compare = InternalComparer.Compare(item, this[ind]);
                if (compare <= 0)
                    break;
            }
            if (index == ind)
                return;
            else if (ind >= this.Count)
                base.MoveItem(index, this.Count - 1);
            else base.MoveItem(index, ind);
        }
        protected override void InsertItem(int index, T item)
        {
            int i;
            for (i = 0; i < this.Count; i++)
            {
                int compare = InternalComparer.Compare(item, this[i]);
                if (compare <= 0)
                {
                    break;
                }
            }
            base.InsertItem(i, item);
        }

        protected override void SetItem(int index, T item)
        {
            base.SetItem(index, item);
            ResortItem(index);
        }
        public bool UseSorting { get; set; }
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            if (UseSorting)
                throw new Exception($"Error - Items cannot be moved while {nameof(UseSorting)} = True");
            base.MoveItem(oldIndex, newIndex);
        }
    }
}