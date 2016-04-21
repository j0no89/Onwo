using System;
using System.Collections.Generic;
using System.Linq;

namespace Onwo.Collections
{
    public class LazyObservableTree<T> : ObservableSortedTree<T>
    {
        public static readonly Func<LazyObservableTree<T>, IEnumerable<LazyObservableTree<T>>> DefaultChildFactory =
            tree => new LazyObservableTree<T>[0];
        public Func<LazyObservableTree<T>, IEnumerable<LazyObservableTree<T>>> ChildFactory
        {
            get { return _childFactory; }
            set { setChildFactory(value); }
        }
        protected Func<LazyObservableTree<T>, IEnumerable<LazyObservableTree<T>>> InternalChildFactory
        {
            get
            {
                if (_childFactory == null)
                    return DefaultChildFactory;
                return _childFactory;
            }
        }
        private Func<LazyObservableTree<T>, IEnumerable<LazyObservableTree<T>>> _childFactory;
        public delegate void ChildFactoryChangedEventHandler(LazyObservableTree<T> sender,
            Func<LazyObservableTree<T>, IEnumerable<LazyObservableTree<T>>> oldChildFactory);
        public event ChildFactoryChangedEventHandler ChildFactoryChanged;
        protected virtual void OnChildFactoryChanged(
            Func<LazyObservableTree<T>, IEnumerable<LazyObservableTree<T>>> oldChildFactory)
        {
            ChildFactoryChanged?.Invoke(this, oldChildFactory);
            if (AreChildrenGenerated)
            {
                regeneratingChildren = true;
                GenerateChildren();
                //OnPropertyChanged(nameof(Children));
            }
            /*if (_areChildrenGenerated)
            {
                AreChildrenGenerated = false;
                OnPropertyChanged(nameof(Children));
            }*/
        }

        protected virtual void setChildFactory(
            Func<LazyObservableTree<T>, IEnumerable<LazyObservableTree<T>>> value)
        {
            if (ReferenceEquals(value, _childFactory)) return;
            var old = _childFactory;
            _childFactory = value;
            OnPropertyChanged(nameof(ChildFactory));
            OnChildFactoryChanged(old);
        }
        private bool regeneratingChildren = false;
        public bool AreChildrenGenerated
        {
            get { return _areChildrenGenerated; }
            protected set
            {
                if (Equals(_areChildrenGenerated, value)) return;
                _areChildrenGenerated = value;
                OnPropertyChanged(nameof(AreChildrenGenerated));
            }
        }
        private bool _areChildrenGenerated;
        protected override void OnPropertyChanged(string propertyName = null)
        {
            /* if (ignoreNextChildrenPropertyChange)
             {
                 if (propertyName != null &&
                     string.Equals(propertyName, nameof(Children), StringComparison.InvariantCulture))
                 {
                     ignoreNextChildrenPropertyChange = false;
                     return;
                 }
             }*/
            base.OnPropertyChanged(propertyName);
        }
        #region Constructors
        public LazyObservableTree()
            : this(default(T), DefaultComparer.Value, null)
        { }

        public LazyObservableTree(IComparer<ObservableSortedTree<T>> comparer)
            : this(default(T), comparer)
        { }
        public LazyObservableTree(IComparer<T> comparer)
            : this(default(T), comparer)
        { }

        public LazyObservableTree(T value)
            : this(value, DefaultComparer.Value, null)
        { }

        public LazyObservableTree(T value, IComparer<T> comparer)
            : this(value, TransformValueToTreeComparer(comparer), null)
        { }
        public LazyObservableTree(T value, IComparer<ObservableSortedTree<T>> comparer)
            : this(value, comparer, null)
        { }

        public LazyObservableTree(T value, Func<LazyObservableTree<T>, IEnumerable<LazyObservableTree<T>>> childFactory)
            : this(value, DefaultComparer.Value, childFactory)
        { }
        public LazyObservableTree(IComparer<T> comparer, Func<LazyObservableTree<T>, IEnumerable<LazyObservableTree<T>>> childFactory)
            : this(default(T), TransformValueToTreeComparer(comparer), childFactory)
        { }
        public LazyObservableTree(IComparer<ObservableSortedTree<T>> comparer, Func<LazyObservableTree<T>, IEnumerable<LazyObservableTree<T>>> childFactory)
            : this(default(T), comparer, childFactory)
        { }

        public LazyObservableTree(T value, IComparer<T> comparer, Func<LazyObservableTree<T>, IEnumerable<LazyObservableTree<T>>> childFactory)
            : this(value, TransformValueToTreeComparer(comparer), childFactory)
        { }
        public LazyObservableTree(T value, IComparer<ObservableSortedTree<T>> comparer, Func<LazyObservableTree<T>, IEnumerable<LazyObservableTree<T>>> childFactory) :
            base(value, comparer, null)
        {
            ChildFactory = childFactory;
            AreChildrenGenerated = false;
            initialising = false;
        }
        #endregion
        #region Methods
        public void GenerateChildren()
        {
            //regenerating children bool is to allow child factory to be called when it has been changed
            //if AreChildrenGenerated is true when the child factory is changed, this function would return immediately, and the old children would remain
            //this allows the old chilren to be destroyed and new children created when needed
            if (AreChildrenGenerated && !regeneratingChildren)
                return;
            regeneratingChildren = false;
            AreChildrenGenerated = true;
            var childCollection=new ObservableSortedCollection<ObservableSortedTree<T>>(_inheritComparer);
            _inheritComparer = null;
            base.setChildren(childCollection);
            //AreChildrenGenerated must be set to true before base.GetChildren, otherwise it will think the children have not yet been generated and start generating them again

            base.getChildren().AddRange(InternalChildFactory(this));
            onChildrenGenerated();
        }

        public delegate void ChildrenGeneratedEventHandler(LazyObservableTree<T> sender);
        public event ChildrenGeneratedEventHandler ChildrenGenerated;
        protected virtual void onChildrenGenerated()
            => ChildrenGenerated?.Invoke(this);
        public void DestroyChildren()
        {
            if (!AreChildrenGenerated)
                return;
            base.setChildren(null);
            AreChildrenGenerated = false;
        }
        #endregion
        #region Overrides

        protected override ObservableSortedCollection<ObservableSortedTree<T>> getChildren()
        {
            GenerateChildren();
            return base.getChildren();
        }

        protected bool initialising { get; } = true;//use this bool to allow constructor set child list and child factory (whhich call setChildren)

        protected override void setChildren(ObservableSortedCollection<ObservableSortedTree<T>> newChildren)
        {
            if (initialising)
                base.setChildren(newChildren);
            else
                throw new NotImplementedException("Cannot manually set the children list in a lazy tree");
        }
        protected override void OnAttachChild(ObservableSortedTree<T> sender, ObservableSortedTree<T> child)
        {
            base.OnAttachChild(sender, child);
            var lazyTree = child as LazyObservableTree<T>;
            if (lazyTree == null) return;
            if (lazyTree.ChildFactory == null)
                lazyTree.ChildFactory = this.ChildFactory;

        }
        protected override void OnTreeValueChanged(T oldVal)
        {
            base.OnTreeValueChanged(oldVal);
            if (AreChildrenGenerated)
            {
                AreChildrenGenerated = false;
                OnPropertyChanged(nameof(Children));
            }
        }

        private IComparer<ObservableSortedTree<T>> _inheritComparer = null;
        protected override IComparer<ObservableSortedTree<T>> getComparer()
        {
            if (AreChildrenGenerated)
                return base.getComparer();
            return _inheritComparer;
        }

        protected override void setComparer(IComparer<ObservableSortedTree<T>> comparer)
        {
            if (AreChildrenGenerated)
                base.setComparer(comparer);
            else _inheritComparer = comparer;
        }

        public override IEnumerator<ObservableSortedTree<T>> GetEnumerator()
        {
            return this.EnumerateDescendantsAndSelf(true).GetEnumerator();
        }
        #endregion
        #region Enumerators
        public IEnumerable<ObservableSortedTree<T>> EnumerateDescendantsAndSelf(bool generatedOnly)
        {
            yield return this;
            if (generatedOnly && !this.AreChildrenGenerated)
            {
                yield break;
            }
            if (!generatedOnly)
            {
                foreach (var child in this.Children)
                {
                    var lazyChild = child as LazyObservableTree<T>;
                    if (lazyChild != null)
                    {
                        foreach (var desc in lazyChild.EnumerateDescendantsAndSelf(false))
                        {
                            yield return desc;
                        }
                    }
                    else
                    {
                        foreach (var desc in child.EnumerateDescendantsAndSelf())
                        {
                            yield return desc;
                        }
                    }
                }
            }
            else
            {
                foreach (var child in this.Children)
                {
                    foreach (var desc in child.EnumerateDescendantsAndSelf())
                    {
                        yield return desc;
                    }
                }
            }
        }
        public IEnumerable<ObservableSortedTree<T>> EnumerateDescendants(bool generatedOnly)
        {
            return this.EnumerateDescendantsAndSelf(generatedOnly).Skip(1);
        }
        public override IEnumerable<ObservableSortedTree<T>> EnumerateDescendantsAndSelf()
        {
            return this.EnumerateDescendantsAndSelf(true);
        }
        public override IEnumerable<ObservableSortedTree<T>> EnumerateDescendantsAndSelf(Predicate<ObservableSortedTree<T>> pred)
        {
            return this.EnumerateDescendantsAndSelf(pred, true);
        }
        public IEnumerable<ObservableSortedTree<T>> EnumerateDescendantsAndSelf(Predicate<ObservableSortedTree<T>> pred, bool generatedOnly)
        {
            if (!pred(this))
                yield break;
            yield return this;
            if (generatedOnly && !this.AreChildrenGenerated)
            {
                yield break;
            }
            if (!generatedOnly)
            {
                foreach (var child in this.Children)
                {
                    var lazyChild = child as LazyObservableTree<T>;
                    if (lazyChild != null)
                    {
                        foreach (var desc in lazyChild.EnumerateDescendantsAndSelf(pred, false))
                        {
                            yield return desc;
                        }
                    }
                    else
                    {
                        foreach (var desc in child.EnumerateDescendantsAndSelf(pred))
                        {
                            yield return desc;
                        }
                    }
                }
            }
            else
            {
                foreach (var child in this.Children)
                {
                    foreach (var desc in child.EnumerateDescendantsAndSelf(pred))
                    {
                        yield return desc;
                    }
                }
            }
        }
        public override IEnumerable<ObservableSortedTree<T>> EnumerateDescendants()
        {
            return this.EnumerateDescendantsAndSelf(true).Skip(1);
        }
        public override IEnumerable<ObservableSortedTree<T>> EnumerateDescendants(Predicate<ObservableSortedTree<T>> pred)
        {
            return this.EnumerateDescendants(pred, true);
        }
        public IEnumerable<ObservableSortedTree<T>> EnumerateDescendants(Predicate<ObservableSortedTree<T>> pred, bool generatedOnly)
        {
            if (generatedOnly && !this.AreChildrenGenerated)
            {
                yield break;
            }
            if (!generatedOnly)
            {
                foreach (var child in this.Children)
                {
                    var lazyChild = child as LazyObservableTree<T>;
                    if (lazyChild != null)
                    {
                        foreach (var desc in lazyChild.EnumerateDescendantsAndSelf(pred, false))
                        {
                            yield return desc;
                        }
                    }
                    else
                    {
                        foreach (var desc in child.EnumerateDescendantsAndSelf(pred))
                        {
                            yield return desc;
                        }
                    }
                }
            }
            else
            {
                foreach (var child in this.Children)
                {
                    foreach (var desc in child.EnumerateDescendantsAndSelf(pred))
                    {
                        yield return desc;
                    }
                }
            }
        }

        #endregion

        public bool IsDescendantOf(LazyObservableTree<T> other)
        {
            if (other == null || !ReferenceEquals(this.Root, other.Root))
                return false;
            return this.EnumerateAncestors().Any(ancestor => ReferenceEquals(ancestor, other));
        }

        public bool IsInSameTreeAs(LazyObservableTree<T> other)
        {
            return ReferenceEquals(this.Root, other.Root);
        }

        public override ObservableSortedTree<T> DeepClone()
        {
            var rootClone = this.ShallowClone();
            if (this.AreChildrenGenerated)
                rootClone.Children.AddRange(this.Children.Select(child => child.DeepClone()));
            return rootClone;
        }

        public override ObservableSortedTree<T> ShallowClone()
        {
            var clone = new LazyObservableTree<T>(this.Value, this.Comparer, this.ChildFactory);
            return clone;
        }
    }
}
