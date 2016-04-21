using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;

namespace Onwo.Collections
{
    public class ObservableSortedTree<T> : INotifyPropertyChanged, IEnumerable<ObservableSortedTree<T>>, IComparable, IComparable<ObservableSortedTree<T>>
    {
        #region Properties

        public int Depth => this.EnumerateAncestors().Count();


        /// <summary>
        /// Gets or sets the value of the current Tree. 
        /// Event = TreeValueChanged; Override = OnTreeValueChanged
        /// </summary>
        public T Value
        {
            get { return _value; }
            set
            {
                if (Equals(value, _value)) return;
                var old = _value;
                _value = value;
                OnPropertyChanged(nameof(Value));
                OnTreeValueChanged(old);
            }
        }
        private T _value;
        public delegate void TreeValueChangedEventHandler(ObservableSortedTree<T> semder, T oldVal);
        public event TreeValueChangedEventHandler TreeValueChanged;
        protected virtual void OnTreeValueChanged(T oldVal)
        {
            if (this.Parent == null)
                return;
            int index = this.Parent.Children.IndexOfFirst(item => ReferenceEquals(item, this));
            if (index < 0)
                throw new Exception("Error - could not find self in parents children");
            this.Parent.Children.ResortItem(index);
            TreeValueChanged?.Invoke(this, oldVal);
        }
        /// <summary>
        /// Gets parent tree of the current tree. Can only be changed internally by adding tree to another tree's children
        /// Note: Root is automatically updated in this and and all descendant trees
        /// Event = ParentChanged; Override = OnParentChanged
        /// </summary>
        public ObservableSortedTree<T> Parent
        {
            get { return _parent; }
            protected set
            {
                if (Equals(value, _parent)) return;
                var old = _parent;
                _parent = value;
                //updateParentChildConnections(old);
                OnParentChanged(old);
                updateRoot();

                OnPropertyChanged(nameof(Parent));
            }
        }
        private ObservableSortedTree<T> _parent;
        public delegate void ParentChangedEventHandler(ObservableSortedTree<T> sender, ObservableSortedTree<T> oldParent);
        public event ParentChangedEventHandler ParentChanged;
        private void updateParentChildConnections(ObservableSortedTree<T> oldParent)
        {
            oldParent?.Children.Remove(this);
            if (_parent == null)
                return;
            if (!_parent.Children.Any(tree => ReferenceEquals(tree, this)))
                _parent.Children.Add(this);
        }
        private void updateRoot()
        {
            if (this.Parent == null)
                this.Root = this;
            else
                this.Root = this.Parent.Root;
            foreach (var child in this.AsEnumerable())
                child.Root = this.Root;
        }
        private void OnParentChanged(ObservableSortedTree<T> oldParent)
        {
            ParentChanged?.Invoke(this, oldParent);
        }
        /// <summary>
        /// Gets the root of the current tree. 
        /// Note: This value Is linked to Parent Changes, and is therefore stored, not calculated
        /// Event = RootChanged; Override = OnRootChanged
        /// </summary>
        public ObservableSortedTree<T> Root
        {
            get
            {
                if (_root == null)
                    _root = this;
                return _root;
            }
            protected set
            {
                if (ReferenceEquals(_root, value)) return;
                var old = _root;
                _root = value;
                OnPropertyChanged(nameof(Root));
                OnRootChanged(old);
            }
        }
        private ObservableSortedTree<T> _root;
        public delegate void RootChangedEventHandler(ObservableSortedTree<T> sender, ObservableSortedTree<T> oldRoot);
        public event RootChangedEventHandler RootChanged;
        protected virtual void OnRootChanged(ObservableSortedTree<T> oldRoot)
        {
            RootChanged?.Invoke(this, oldRoot);
        }
        /// <summary>
        /// Gets or sets the the collection of child trees for the current tree
        /// </summary>
        public ObservableSortedCollection<ObservableSortedTree<T>> Children
        {
            get { return getChildren(); }
            protected set { setChildren(value); }
        }
        /// <summary>
        /// Overrideable method to return the current list of child tree elements
        /// </summary>
        /// <returns>observable collection of child tree elements</returns>
        protected virtual ObservableSortedCollection<ObservableSortedTree<T>> getChildren()
        {
            return _children;
        }
        /// <summary>
        /// Overrideable method for setting the collection of current child tree elements
        /// Note: Parent, and hence Root, automatically updated for all new children
        /// 
        /// </summary>
        /// <param name="newChildren">the list of child tree elements</param>
        protected virtual void setChildren(ObservableSortedCollection<ObservableSortedTree<T>> newChildren)
        {
            if (ReferenceEquals(newChildren, _children)) return;
            var old = _children;
            _children = newChildren;
            // _children.Comparer = this.Comparer;
            manageChildListEvents(old, _children);
            OnChildListChanged(old);
            OnPropertyChanged(nameof(Children));
            //OnPropertyChanged(nameof(HasChildren));
        }
        protected ObservableSortedCollection<ObservableSortedTree<T>> _children;
        /*public delegate void ChildListChangedEventHandler<T2>(
            ObservableSortedCollection<ObservableSortedTree<T2>> oldChildren,
            ObservableSortedCollection<ObservableSortedTree<T2>> newChildren);
        public event ChildListChangedEventHandler<T> ChildListChanged;
        protected virtual void OnChildListChanged(ObservableSortedCollection<ObservableSortedTree<T>> oldChildren,
            ObservableSortedCollection<ObservableSortedTree<T>> newChildren)
        {
            ChildListChanged?.Invoke(oldChildren, newChildren);
        }*/

        public delegate void ChildListChangedEventHandler(ObservableSortedTree<T> sender,
            ObservableSortedCollection<ObservableSortedTree<T>> oldChildren);
        public event ChildListChangedEventHandler ChildListChanged;
        protected virtual void OnChildListChanged(ObservableSortedCollection<ObservableSortedTree<T>> oldChildren)
        {
            /*if (this._children != null)
                this._children.Comparer = this.Comparer;*/

            ChildListChanged?.Invoke(this, oldChildren);
            bool oldHasChild = oldChildren != null && oldChildren.Count > 0;
            bool newHasChild = this._children != null && this._children.Count > 0;
            if (oldChildren != null && this.Children!=null)
            {
                this.Children.Comparer = oldChildren.Comparer;
            }
            if (oldHasChild != newHasChild)
                OnPropertyChanged(nameof(HasChildren));
        }
        /// <summary>
        /// attaches and detaches events for the child list when it is changed. Triggered by setting this.Children
        /// </summary>
        /// <param name="oldChildren">the old child tree list</param>
        /// <param name="newChildren">the new child tree list</param>
        private void manageChildListEvents(ObservableSortedCollection<ObservableSortedTree<T>> oldChildren,
            ObservableSortedCollection<ObservableSortedTree<T>> newChildren)
        {
            if (oldChildren != null)
            {
                oldChildren.ItemAdded -= OnChildTreeAdded;
                oldChildren.ItemRemoved -= OnChildTreeRemoved;
                oldChildren.ItemChanged -= OnChildTreeChanged;
                oldChildren.ComparerChanged -= OnChildListComparerChanged;
                foreach (var child in oldChildren)
                {
                    OnDetachChild(child);
                }
            }
            //_children = newChildren;
            if (newChildren != null)
            {
                newChildren.ItemAdded += OnChildTreeAdded;
                newChildren.ItemRemoved += OnChildTreeRemoved;
                newChildren.ItemChanged += OnChildTreeChanged;
                newChildren.ComparerChanged += OnChildListComparerChanged;
                foreach (var child in newChildren)
                {
                    OnAttachChild(child);
                }
            }
        }
        /// <summary>
        /// Gets a value indicating whether the current tree has any children
        /// </summary>
        public bool HasChildren => _children != null && _children.Count > 0;
        #endregion


        #region Comparer
        public static MyLazy<IComparer<ObservableSortedTree<T>>> DefaultComparer = new MyLazy<IComparer<ObservableSortedTree<T>>>(
            () => Comparer<ObservableSortedTree<T>>.Default);
        public static MyLazy<IComparer<T>> DefaultValueComparer = new MyLazy<IComparer<T>>(
            () => Comparer<T>.Default);
        //ObservableSortedCollection<ObservableSortedTree<T>>.DefaultComparer;
        /// <summary>
        /// Gets or sets the IComparer used for auto-sorting the tree.
        /// Note: All descendants tree comparers are updated, and child list comparers are auto updated to match each tree.
        /// Event = ComparerChanged, Override = OnComparerChanged
        /// </summary>
        public IComparer<ObservableSortedTree<T>> Comparer
        {
            get { return getComparer(); }
            set { setComparer(value); }
        }
        protected virtual IComparer<ObservableSortedTree<T>> getComparer()
            => this.Children.Comparer;

        protected virtual void setComparer(IComparer<ObservableSortedTree<T>> comparer)
            => this.Children.Comparer = comparer;
        //private IComparer<ObservableSortedTree<T>> _comparer;
        /// <summary>
        /// Comparer for internal use. If this.Comparer is null, returns a default comparer
        /// </summary>
        protected IComparer<ObservableSortedTree<T>> InternalComparer
        {
            get
            {
                if (Comparer == null)
                    return DefaultComparer.Value;
                return Comparer;
                ;
            }
        }
        
        /// <summary>
        /// Converts a value comparer to a tree comparer and assigns it to the comparer for this tree
        /// </summary>
        /// <param name="comparer">The value comparer to convert</param>
        public void SetComparerFromValueComparer(IComparer<T> comparer)
        {
            if (comparer == null)
                Comparer = null;
            else Comparer = TransformValueToTreeComparer(comparer);
        }
        
        
        
        public delegate void ComparerChangedEventHandler(ObservableSortedTree<T> sender, IComparer<ObservableSortedTree<T>> oldCOmparer);
        public event ComparerChangedEventHandler ComparerChanged;
        protected virtual void OnComparerChanged(IComparer<ObservableSortedTree<T>> oldComparer)
        {
            if (this.Children != null)
            {
                foreach (var child in this.Children)
                {
                    child.Comparer = this.Comparer;
                }
            }
            ComparerChanged?.Invoke(this, oldComparer);
        }
        /// <summary>
        /// This method is raised when Children.Comparer is chamged
        /// </summary>
        /// <param name="sender">The collection in which the comparer was changed</param>
        protected void OnChildListComparerChanged(ObservableSortedCollection<ObservableSortedTree<T>> sender, 
            IComparer<ObservableSortedTree<T>> oldComparer)
        {
            //this.Comparer = sender.Comparer;
            OnPropertyChanged(nameof(Comparer));
            OnComparerChanged(oldComparer);
        }
        public int CompareTo(ObservableSortedTree<T> other)
        {
            if (other==null)
                throw new NullReferenceException("cannot compare to a null tree");
            return DefaultValueComparer.Value.Compare(this.Value, other.Value);
            //return InternalComparer.Compare(this, other);
        }
        public int CompareTo(object obj)
        {
            return this.CompareTo(obj as ObservableSortedTree<T>);
        }
        public static IComparer<ObservableSortedTree<T>> TransformValueToTreeComparer(IComparer<T> valueComparer)
        {
            return Comparer<ObservableSortedTree<T>>.Create((tree1, tree2) =>
            {
                return valueComparer.Compare(tree1.Value, tree2.Value);
            });
        }
        #endregion
        #region Constructors
        public ObservableSortedTree()
            : this(default(T), DefaultComparer.Value)
        { }
        public ObservableSortedTree(IComparer<ObservableSortedTree<T>> comparer)
            : this(default(T), comparer)
        { }
        public ObservableSortedTree(IComparer<T> comparer)
            : this(default(T), comparer)
        { }
        public ObservableSortedTree(T value, IComparer<T> comparer)
            : this(value, TransformValueToTreeComparer(comparer))
        { }
        public ObservableSortedTree(T value)
            : this(value, DefaultComparer.Value)
        { }
        public ObservableSortedTree(T value, IComparer<ObservableSortedTree<T>> comparer)
            : this(value, comparer, new ObservableSortedCollection<ObservableSortedTree<T>>(comparer))
        { }
        protected ObservableSortedTree(T value, IComparer<ObservableSortedTree<T>> comparer, ObservableSortedCollection<ObservableSortedTree<T>> children)
        {
            Value = value;
            Children = children;
            Comparer = comparer;
            Root = this;
        }
        #endregion
        #region eventMethods
        private void Child_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnChildTreePropertyChangedOverride(sender, e?.PropertyName ?? "");
        }
        protected virtual void OnChildTreePropertyChangedOverride(object sender, string name)
        {
        }
        public delegate void TreeItemAttachedEventHandler(ObservableSortedTree<T> sender, ObservableSortedTree<T> child);
        public event TreeItemAttachedEventHandler ChildTreeAttached;
        private void OnAttachChild(ObservableSortedTree<T> child)
        {
            child.Parent = this;
            child.Comparer = this.Comparer;
            child.PropertyChanged += Child_PropertyChanged;
            ChildTreeAttached?.Invoke(this, child);
            OnAttachChild(this, child);
        }
        protected virtual void OnAttachChild(ObservableSortedTree<T> sender, ObservableSortedTree<T> child)
        { }
        public delegate void TreeItemDetachedEventHandler(ObservableSortedTree<T> sender, ObservableSortedTree<T> child);
        public event TreeItemDetachedEventHandler ChildTreeDetached;
        private void OnDetachChild(ObservableSortedTree<T> child)
        {
            var oldParent = child.Parent;
            child.Parent = null;
            child.PropertyChanged -= Child_PropertyChanged;
            ChildTreeDetached?.Invoke(this, child);
            OnDetachChildOverride(this, child);
        }
        protected virtual void OnDetachChildOverride(ObservableSortedTree<T> sender, ObservableSortedTree<T> child)
        { }
        public delegate void ChildTreeChangedEventHandler(ObservableSortedTree<T> sender, ObservableSortedTree<T> newChild, ObservableSortedTree<T> oldChild, int index);
        public event ChildTreeChangedEventHandler ChildTreeChanged;
        //public event ItemmChangedEventHandler<ObservableSortedTree<T>> ChildTreeChanged;
        private void OnChildTreeChanged(ICollection<ObservableSortedTree<T>> sender, ObservableSortedTree<T> oldItem, ObservableSortedTree<T> newItem, int index)
        {
            OnDetachChild(oldItem);
            OnAttachChild(newItem);
            ChildTreeChanged?.Invoke(this, oldItem, newItem, index);
            OnChildTreeChangedOverride(sender, oldItem, newItem, index);
        }
        protected virtual void OnChildTreeChangedOverride(ICollection<ObservableSortedTree<T>> sender, ObservableSortedTree<T> oldItem, ObservableSortedTree<T> newItem, int index)
        {
        }
        public delegate void ChildTreeRemovedEventHandler(ObservableSortedTree<T> sender, ObservableSortedTree<T> newCHild, int index);
        public event ChildTreeRemovedEventHandler ChildTreeRemoved;
        // public event ItemmAddedEventHandler<ObservableSortedTree<T>> ChildTreeRemoved;
        private void OnChildTreeRemoved(ICollection<ObservableSortedTree<T>> sender, ObservableSortedTree<T> oldItem, int index)
        {
            OnDetachChild(oldItem);
            ChildTreeRemoved?.Invoke(this, oldItem, index);
            OnChildTreeRemovedOverride(sender, oldItem, index);
            if (this.Children.Count == 0)
                OnPropertyChanged(nameof(HasChildren));
        }
        protected virtual void OnChildTreeRemovedOverride(ICollection<ObservableSortedTree<T>> sender, ObservableSortedTree<T> oldItem, int index)
        {
        }
        public delegate void ChildTreeAddedEventHandler(ObservableSortedTree<T> sender, ObservableSortedTree<T> newCHild, int index);
        public event ChildTreeAddedEventHandler ChildTreeAdded;
        //public event ItemmAddedEventHandler<ObservableSortedTree<T>> ChildTreeAdded;
        private void OnChildTreeAdded(ICollection<ObservableSortedTree<T>> sender, ObservableSortedTree<T> newItem, int index)
        {
            OnAttachChild(newItem);
            ChildTreeAdded?.Invoke(this, newItem, index);
            OnChildTreeAddedOverride(this, newItem, index);
            if (this.Children.Count == 1)
                OnPropertyChanged(nameof(HasChildren));
        }
        protected virtual void OnChildTreeAddedOverride(ObservableSortedTree<T> sender, ObservableSortedTree<T> newItem, int index)
        {
        }
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        #region Overrides & Interface Methods
        public override string ToString()
        {
            string name = Value?.ToString();
            if (string.IsNullOrEmpty(name))
                name = base.ToString();
            if (string.IsNullOrEmpty(name))
                name = this.GetType().ToString();

            int count = _children?.Count ?? 0;
            return $"{name}[{count.ToString("0")}]";
        }
        
        #endregion
        #region Enumerators
        public virtual IEnumerator<ObservableSortedTree<T>> GetEnumerator()
        {
            return this.EnumerateDescendantsAndSelf().GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public virtual IEnumerable<ObservableSortedTree<T>> EnumerateDescendantsAndSelf()
        {
            yield return this;
            if (this._children == null)
                yield break;
            foreach (var child in this.Children)
            {
                foreach (var desc in child.EnumerateDescendantsAndSelf())
                {
                    yield return desc;
                }
            }
        }
        public virtual IEnumerable<ObservableSortedTree<T>> EnumerateDescendantsAndSelf(Predicate<ObservableSortedTree<T>> pred)
        {
            if (!pred(this))
                yield break;
            yield return this;
            if (this._children == null)
                yield break;
            foreach (var child in this.Children)
            {
                foreach (var desc in child.EnumerateDescendantsAndSelf(pred))
                {
                    yield return desc;
                }
            }
        }
        public virtual IEnumerable<ObservableSortedTree<T>> EnumerateDescendants()
        {
            //yield return this;
            if (this._children == null)
                yield break;
            foreach (var child in this.Children)
            {
                foreach (var desc in child.EnumerateDescendantsAndSelf())
                {
                    yield return desc;
                }
            }
        }
        public virtual IEnumerable<ObservableSortedTree<T>> EnumerateDescendants(Predicate<ObservableSortedTree<T>> pred)
        {
            if (this._children == null)
                yield break;
            foreach (var child in this.Children)
            {
                foreach (var desc in child.EnumerateDescendantsAndSelf(pred))
                {
                    yield return desc;
                }
            }
        }
        public virtual IEnumerable<ObservableSortedTree<T>> EnumerateAncestorsAndSelf()
        {
            var cTree = this;
            while (cTree != null)
            {
                yield return cTree;
                cTree = cTree.Parent;
            }
        }
        public virtual IEnumerable<ObservableSortedTree<T>> EnumerateAncestors()
        {
            var cTree = this.Parent;
            while (cTree != null)
            {
                yield return cTree;
                cTree = cTree.Parent;
            }
        }
        #endregion
        #region Methods
        /// <summary>
        /// Gets the index of the current tree in its parent tree
        /// </summary>
        /// <returns></returns>
        public int IndexInParent()
        {
            for (int index = 0; index < this.Parent?.Children.Count; index++)
            {
                if (ReferenceEquals(this, this.Parent.Children[index]))
                {
                    return index;
                }
            }
            return -1;
        }
        /// <summary>
        /// Gets an array of positions indicating the position of current tree from the root
        /// </summary>
        /// <returns></returns>
        public int[] PositionFromRoot()
        {
            var posArray = this.EnumerateAncestorsAndSelf()
                .Select(ancestor => ancestor.IndexInParent())
                .ToArray();
            var newArray = new int[posArray.Length - 1];
            Array.Copy(posArray, 0, newArray, 0, newArray.Length);
            return newArray;
        }
        /// <summary>
        /// Removes the current tree from its parent tree
        /// </summary>
        public void Prune()
        {
            for (int index = 0; index < this.Parent?.Children.Count; index++)
            {
                if (ReferenceEquals(this, this.Parent.Children[index]))
                {
                    this.Parent.Children.RemoveAt(index);
                    return;
                }
            }
        }

        /// <summary>
        /// Creates a clone of the current tree and all its children
        /// </summary>
        /// <returns></returns>
        public virtual ObservableSortedTree<T> DeepClone()
        {
            var rootClone = this.ShallowClone();
            if (this.HasChildren)
                rootClone.Children.AddRange(this.Children.Select(child => child.DeepClone()));
            return rootClone;
        }
        /// <summary>
        /// Creates a clone of the current tree only, without any of its children
        /// </summary>
        /// <returns></returns>
        public virtual ObservableSortedTree<T> ShallowClone()
        {
            return new ObservableSortedTree<T>(this.Value, this.Comparer);
        }
        #endregion
    }
}
