using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Onwo.Collections
{
    public class MyObservableCollection<T>:ObservableCollection<T>
    {
        public MyObservableCollection():this(null) {}

        public MyObservableCollection(IEnumerable<T> items):base()
        {
            this.AddRange(items);
        } 

        public void AddRange(IEnumerable<T> items)
        {
            items?.ForEach(this.Add);
        }
        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            OnItemAdded(item, index);
        }

        protected override void SetItem(int index, T item)
        {
            var old = base[index];
            base.SetItem(index, item);
            OnItemChanged(old, item, index);
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            base.MoveItem(oldIndex, newIndex);
            OnItemMoved(oldIndex,newIndex);
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];
            base.RemoveItem(index);
            OnItemRemoved(item,index);
        }

        public void RemoveWhere(Predicate<T> pred)
        {
            RemoveWhere(pred,-1);
        }
        public void RemoveWhere(Predicate<T> pred, int max)
        {
            if (max == 0)
                return;
            int count = 0;
            List<int> indices = new List<int>();
            for (int i = 0; i < this.Count; i++)
            {
                if (pred(this[i]))
                {
                    indices.Add(i);
                    if (max >= 0)
                    {
                        count++;
                        if (count >= max)
                            break;
                    }
                }
            }
            int start = Math.Max(max, indices.Count) - 1;
            for (int i = start; i >= 0; i--)
            {
                this.RemoveAt(indices[i]);
            }
        }
        protected override void ClearItems()
        {
            var items = this.ToArray();
            base.ClearItems();
            items.ForEach((item, index) => this.OnItemRemoved(item, index));
        }
        
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
        }
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        #region Events
        public delegate void ItemmChangedEventHandler(ICollection<T> sender, T oldItem, T newItem, int index);
        public event ItemmChangedEventHandler ItemChanged;
        protected virtual void OnItemChanged(T oldItem, T newItem, int index)
        {
            ItemChanged?.Invoke(this, oldItem, newItem, index);
            OnItemDetached(oldItem, index);
            OnItemAttached(newItem, index);
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
        protected virtual void OnItemAttached(T newItem, int index)
        {
            ItemAttached?.Invoke(this, newItem, index);
        }
        public delegate void ItemDetachedEventHandler(ICollection<T> sender, T oldItem, int index);
        public event ItemDetachedEventHandler ItemDetached;
        protected virtual void OnItemDetached(T oldItem, int index)
        {
            ItemDetached?.Invoke(this, oldItem, index);
        }
        public delegate void ItemMovedEventHandler(ICollection<T> sender, int oldIndex, int newIndex);
        public event ItemMovedEventHandler ItemMoved;
        protected virtual void OnItemMoved(int oldIndex, int newIndex)
        {
            ItemMoved?.Invoke(this, oldIndex, newIndex);
        }
        #endregion
    }
}
