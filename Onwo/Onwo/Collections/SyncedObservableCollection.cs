using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Onwo.Collections
{
    public class SyncedObservableCollection2<T> : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public ObservableCollection<T> Source { get; protected set; }

        public SyncedObservableCollection2(ObservableCollection<T> source)
        {
        }
    }
    public class SyncedObservableCollection_View<T> : SyncedObservableCollection_Simple<T>
    {
        public SyncedObservableCollection_View(MyObservableCollection<T> source):this(source,null)
        { }

        public SyncedObservableCollection_View(MyObservableCollection<T> source, IEqualityComparer<T> equalityComparer)
            :base(source,equalityComparer)
        {
        }
    }
    public class SyncedObservableCollection_Simple<T> : INotifyPropertyChanged
    {
        public SyncedObservableCollection_Simple(MyObservableCollection<T> source):this(source,null)
        { }

        
        public SyncedObservableCollection_Simple(MyObservableCollection<T> source, IEqualityComparer<T> equalityComparer )
            :this(source,new ObservableCollection<T>(),equalityComparer)
        {
            
        }
        public SyncedObservableCollection_Simple(MyObservableCollection<T> source, ObservableCollection<T> synced, IEqualityComparer<T> equalityComparer)
        {
            this.Synced = synced;
            this.Source = source;
            this.EqualityComparer = equalityComparer;
        }
        private MyObservableCollection<T> _source;
        private ObservableCollection<T> _synced;
        private IEqualityComparer<T> _equalityComparer;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MyObservableCollection<T> Source
        {
            get { return _source; }
            set
            {
                if (ReferenceEquals(value, _source)) return;
                if (value == null)
                    throw new ArgumentNullException(nameof(Source));
                var old = _source;
                _source = value;
                OnPropertyChanged(nameof(Source));
                OnSourceChanged(old);
            }
        }
        public delegate void SourceChangedEventHandler(
           SyncedObservableCollection_Simple<T> sender, MyObservableCollection<T> oldSource);
        public event SourceChangedEventHandler SourceChanged;
        protected virtual void OnSourceChanged(MyObservableCollection<T> oldSource)
        {
            if (oldSource != null)
            {
                oldSource.ItemAttached -= Source_ItemAttached;
                oldSource.ItemDetached -= Source_ItemDetached;
            }
            this.Synced.Clear();
            this.Source.ItemAttached += Source_ItemAttached;
            this.Source.ItemDetached += Source_ItemDetached;
            this.Source.ForEach(item => this.Synced.Add(item));

            SourceChanged?.Invoke(this, oldSource);
        }
        private void Source_ItemAttached(ICollection<T> sender, T newItem, int index)
        {
            this.Synced.Add(newItem);
        }
        private void Source_ItemDetached(ICollection<T> sender, T oldItem, int index)
        {
            if (EqualityComparer == null)
                this.Synced.Remove(oldItem);
            else RemoveFirstInSynced(item=>EqualityComparer.Equals(item,oldItem));
        }

        protected void RemoveFirstInSynced(Predicate<T> pred)
        {
            for (int i = 0; i < this.Synced.Count; i++)
            {
                if (pred(this.Synced[i]))
                {
                    this.Synced.RemoveAt(i);
                    return;
                }
            }
        }
        public ObservableCollection<T> Synced
        {
            get { return _synced; }
            protected set
            {
                if (ReferenceEquals(value, _synced)) return;
                if (value == null)
                    throw new ArgumentNullException(nameof(Synced));
                var old = _synced;
                _synced = value;
                OnPropertyChanged(nameof(Synced));
                OnSyncedListChanged(old);
            }
        }
        public delegate void SyncedListChangedEventHandler(
            SyncedObservableCollection_Simple<T> sender, ObservableCollection<T> oldSource);
        public event SyncedListChangedEventHandler SyncedListChanged;
        protected virtual void OnSyncedListChanged(ObservableCollection<T> oldSource)
        {
            SyncedListChanged?.Invoke(this, oldSource);
        }
        public IEqualityComparer<T> EqualityComparer
        {
            get { return _equalityComparer; }
            set
            {
                if (ReferenceEquals(value, _equalityComparer)) return;
                _equalityComparer = value;
                OnPropertyChanged();
            }
        }
        public static readonly IEqualityComparer<T> DefaultComparer = EqualityComparer<T>.Default;
        protected IEqualityComparer<T> InternalEqualityComparer
        {
            get
            {
                if (EqualityComparer == null)
                    return DefaultComparer;
                else
                    return this.EqualityComparer;
            }
        }
    }
    public class SyncedObservableCollection<T>:INotifyPropertyChanged
    {
        private MyObservableCollection<T> _source;
        private ObservableCollection<T> _synced;
        private IEqualityComparer<T> _equalityComparer;
        private Predicate<T> _filter;
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MyObservableCollection<T> Source
        {
            get { return _source; }
            set
            {
                if (ReferenceEquals(value, _source)) return;
                if (value == null)
                    throw new ArgumentNullException(nameof(Source));
                var old = _source;
                _source = value;
                OnPropertyChanged(nameof(Source));
                OnSourceChanged(old);
            }
        }

        public ObservableCollection<T> Synced
        {
            get { return _synced; }
            set
            {
                if (ReferenceEquals(value, _synced)) return;
                if (value == null)
                    throw new ArgumentNullException(nameof(Synced));
                var old = _synced;
                _synced = value;
                OnPropertyChanged(nameof(Synced));
                OnSyncedListChanged(old);
            }
        }

        public IEqualityComparer<T> EqualityComparer
        {
            get { return _equalityComparer; }
            set
            {
                if (ReferenceEquals(value, _equalityComparer)) return;
                _equalityComparer = value;
                OnPropertyChanged();
            }
        }

        public Predicate<T> Filter
        {
            get { return _filter; }
            set
            {
                if (ReferenceEquals(value, _filter)) return;
                var old = _filter;
                _filter = value;
                OnPropertyChanged();
                onFilterChanged(old);
            }
        }

        private IList<bool> sourceFilterResults;
        protected virtual void onFilterChanged(Predicate<T> oldFilter)
        {
            var oldSourceFilterResults = sourceFilterResults;
            var internalFilter = this.InternalFilter;
            sourceFilterResults = Source.Select(item => internalFilter(item)).ToList();
            if (oldFilter == null)
            {
                //the items in the synced list were not filtered previously, so they are all there
                //if th current filter is null nothing needs to be done
                if (this.Filter == null)
                    return;
                RemoveFromSyncedWhere(this.Filter);
                return;
            }
            if (this.Filter == null)
            {
                //old filter was not null, but new filter is, 
                //so all items that where excluded by the old filter should be added to the synced list
                this.Source.ForEach((item, i) =>
                {
                    //if (!oldFilter(item))
                    if (!oldSourceFilterResults[i])
                        this.AddToSynced(item,i);
                });
                return;
            }
            //old filter was not null, and new filter is not null
            //all items that where included by the old filter should be removed
            //but only if they fail the new filter 
            //(do not need to test against oldFilter as all items in synced list have already passed)
            RemoveFromSyncedWhere(item=> oldFilter(item) && !this.Filter(item));
            //all items that where excluded by the old filter should be added to the synced list
            //but only if they pass the new filter
            this.Source.ForEach((item, i) =>
            {
                //if (!oldFilter(item) && this.Filter(item))
                if (!oldSourceFilterResults[i] && sourceFilterResults[i])
                    this.AddToSynced(item, i);
            });
        }

        private void RemoveFromSyncedWhere(Predicate<T> pred)
        {
            for (int i = this.Synced.Count - 1; i >= 0; i--)
            {
                if (pred(this.Synced[i]))
                    this.Synced.RemoveAt(i);
            }
        }

        private static readonly IEqualityComparer<T> DefaultComparer = EqualityComparer<T>.Default;
        private IEqualityComparer<T> InternalEqualityComparer
        {
            get
            {
                if (EqualityComparer == null)
                    return DefaultComparer;
                else
                    return this.EqualityComparer;
            }
        }
        private static readonly Predicate<T> DefaultFilter = (T arg)=> true;
        private Predicate<T> InternalFilter
        {
            get
            {
                if (this.Filter == null)
                    return DefaultFilter;
                else
                    return this.Filter;
            }
        }
        private void AddToSynced(T newItem, int index)
        {
            if (!TrackIndexChanges|| index >= this.Source.Count - 1 || index < 0)
            {
                this.Synced.Add(newItem);
                return;
            }
            var filter = this.InternalFilter;
            int count = sourceFilterResults.Take(index).Count(item => item);
            //int count = this.Source.Take(index).Count(item => filter(item));
           
            if (count==this.Synced.Count-1)
                this.Synced.Add(newItem);
            else
                this.Synced.Insert(count, newItem);
        }
        public SyncedObservableCollection(MyObservableCollection<T> source):this(source,new ObservableCollection<T>())
        { }

        public SyncedObservableCollection(MyObservableCollection<T> source, ObservableCollection<T> synced)
        {
        }

        public delegate void SourceChangedEventHandler(
            SyncedObservableCollection<T> sender, MyObservableCollection<T> oldSource);
        public event SourceChangedEventHandler SourceChanged;
        protected virtual void OnSourceChanged(MyObservableCollection<T> oldSource)
        {
            oldSource.ItemAttached -= Source_ItemAttached;
            oldSource.ItemDetached -= Source_ItemDetached;
            oldSource.ItemMoved -= Source_ItemMoved;

            this.Synced.Clear();
            var internalFilter = this.InternalFilter;
            this.sourceFilterResults = this.Source.Select(item => internalFilter(item)).ToList();
            this.Source.ItemAttached += Source_ItemAttached;
            this.Source.ItemDetached += Source_ItemDetached;
            this.Source.ItemMoved += Source_ItemMoved;
            this.Source.Where(item => InternalFilter(item))
                .ForEach(item => this.Synced.Add(item));

            SourceChanged?.Invoke(this, oldSource);
        }

        private void Source_ItemMoved(ICollection<T> sender, int oldIndex, int newIndex)
        {
            //this.sourceFilterResults.
            throw new NotImplementedException();
        }

        private void Source_ItemDetached(ICollection<T> sender, T oldItem, int index)
        {
            this.sourceFilterResults.RemoveAt(index);
            if (TrackIndexChanges)
            {
                
            }

            if (EqualityComparer == null)
            {
                this.Synced.Remove(oldItem);
                return;
            }
            int ind = -1;
            for (int i = 0; i < this.Synced.Count; i++)
            {
                if (!EqualityComparer.Equals(this.Synced[i], oldItem))
                    continue;
                ind = i;
                break;
            }
            if (ind < 0)
                return;
            this.Synced.RemoveAt(ind);
        }
        /// <summary>
        /// If true, items in the synced list will be moved and inserted to match the source list
        /// If false, items in the synced list are simply added and newItem moves are not tracked
        /// </summary>
        public bool TrackIndexChanges { get; set; }
        private void Source_ItemAttached(ICollection<T> sender, T newItem, int index)
        {
            if (index==this.sourceFilterResults.Count)
                this.sourceFilterResults.Add(InternalFilter(newItem));
            else this.sourceFilterResults.Insert(index, InternalFilter(newItem));
            AddToSynced(newItem,index);
        }

        public delegate void SyncedListChangedEventHandler(
            SyncedObservableCollection<T> sender, ObservableCollection<T> oldSource);
        public event SyncedListChangedEventHandler SyncedListChanged;
        protected virtual void OnSyncedListChanged(ObservableCollection<T> oldSource)
        {
            SyncedListChanged?.Invoke(this, oldSource);
        }
    }
}
