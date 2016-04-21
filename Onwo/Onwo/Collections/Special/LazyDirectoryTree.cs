using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Onwo.Collections.Special
{
    public class LazyDirectoryTree : LazyObservableTree<DirectoryInfo>
    {
        public static readonly IComparer<ObservableSortedTree<DirectoryInfo>> DirectoryComparer =
            Comparer<ObservableSortedTree<DirectoryInfo>>.Create(compareDirectoryTrees);
        private static int compareDirectoryTrees(ObservableSortedTree<DirectoryInfo> tree1,
            ObservableSortedTree<DirectoryInfo> tree2)
        {
            if (ReferenceEquals(tree1, tree2))
                return 0;
            else if (tree1 == null)
                return -1;
            else if (tree2 == null)
                return 1;

            if (ReferenceEquals(tree1.Value, tree2.Value))
                return 0;
            else if (tree1.Value == null)
                return -1;
            else if (tree2.Value == null)
                return 1;

            return string.Compare(tree1.Value.Name, tree2.Value.Name, StringComparison.InvariantCultureIgnoreCase);
        }
        private static int compareFileInfo_simple(FileInfo f1, FileInfo f2)
            => string.Compare(f1.Name, f2.Name, StringComparison.InvariantCultureIgnoreCase);
        private static readonly IComparer<FileInfo> fileComparer = Comparer<FileInfo>.Create(
            (f1, f2) => compareFileInfo_simple(f1, f2));
        private static int compareFileSystemInfos(FileSystemInfo fsi1, FileSystemInfo fsi2)
        {
            bool isDirectory1 = fsi1 is DirectoryInfo;
            bool isDirectory2 = fsi2 is DirectoryInfo;
            if (isDirectory1 != isDirectory2)
            {
                if (isDirectory1)
                    return 1;
                return -1;
            }
            return string.Compare(fsi1.Name, fsi2.Name, StringComparison.InvariantCultureIgnoreCase);
        }
        private static readonly IComparer<FileSystemInfo> fileSystemInfoComparer =
            Comparer<FileSystemInfo>.Create((fsi1, fsi2) => compareFileSystemInfos(fsi1, fsi2));
        private static readonly Func<LazyObservableTree<DirectoryInfo>, IEnumerable<LazyObservableTree<DirectoryInfo>>> directoryFactory = tree => childFactory_Run(tree);
        private static IEnumerable<LazyDirectoryTree> childFactory_Run(
            LazyObservableTree<DirectoryInfo> parent)
        {
            if (parent.Value == null)
                throw new NullReferenceException("Error: Directory cannot be null");
            try
            {
                var dirs = parent.Value.EnumerateDirectories("*", SearchOption.TopDirectoryOnly).ToArray();
                var children = dirs.Select(dir =>
                {
                    var ty = new LazyDirectoryTree(dir);
                    return ty;
                }).ToArray();
                return children;
            }
            catch (Exception)
            {
                var dirTree = parent as LazyDirectoryTree;
                if (dirTree != null)
                    dirTree.AccessAuthorised = false;
                return new LazyDirectoryTree[0];
            }
        }
        #region Properties
        /// <summary>
        /// Gets a bool indicating whether or not access to the directory was granted
        /// </summary>
        public bool AccessAuthorised
        {
            get { return _accessAuthorised; }
            protected set
            {
                if (_accessAuthorised == value) return;
                _accessAuthorised = value;
                OnPropertyChanged(nameof(AccessAuthorised));
                OnPropertyChanged(nameof(UnauthorisedAccess));
                IsSelected = false;
            }
        }
        private bool _accessAuthorised;
        /// <summary>
        /// Gets a bool indicating whether or not access to the directory was denied
        /// </summary>
        public bool UnauthorisedAccess => !_accessAuthorised;
        /// <summary>
        /// Gets or sets a bool indicating if the tree is expanded in a tree view
        /// Note: Intended for use with WPF only
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded == value) return;
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }
        private bool _isExpanded;
        /// <summary>
        /// Gets or sets the selection status of the current directory
        /// Note: Is A Nullable bool, null represents partial selection, and cannot be directly set
        /// </summary>
        public bool? IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (IsSelected == value) return;
                var old = _isSelected;
                _isSelected = value;
                OnIsSelectedChanged(old);
                OnPropertyChanged(nameof(IsSelected));
                OnIsSelectedChangedOverride(old);
            }
        }
        private bool? _isSelected;
        public ObservableSortedCollection<FileInfo> FileList
        {
            get { return _fileList; }
            protected set
            {
                if (ReferenceEquals(_fileList, value)) return;
                _fileList = value;
                OnPropertyChanged(nameof(FileList));
            }
        }
        #endregion
        #region IsSelected event methods
        public delegate void IsSelectedChangedEventHandler(LazyDirectoryTree sender, bool? oldVal);
        /// <summary>
        /// This event is triggered when the value of <see cref="IsSelected"/> is changed
        /// </summary>
        public event IsSelectedChangedEventHandler IsSelectedChanged;
        //private bool isSelectedChangePropogating = false;
        private bool inheritIsSelectedFromParent = false;
        private void OnIsSelectedChanged(bool? oldVal)
        {
            if (suppress_IsSelected_Changed)
                return;
            if (UnauthorisedAccess)
            {
                _isSelected = false;
                return;
            }
            if (inheritIsSelectedFromParent)
            {
                if (this.AreChildrenGenerated)
                {
                    suppress_Child_IsSelected_Changed = true;
                    foreach (var child in this.Children.Cast<LazyDirectoryTree>())
                    {
                        child.inheritIsSelectedFromParent = true;
                        child.IsSelected = this._isSelected;
                        inheritIsSelectedFromParent = false;
                    }
                    suppress_Child_IsSelected_Changed = false;
                }

            }
            else
            {
                if (oldVal == null)
                    this._isSelected = true;
                else if (oldVal == true)
                    this._isSelected = false;
                else if (oldVal == false)
                    this._isSelected = true;
                if (AreChildrenGenerated && Children.Count > 0)
                {
                    var lastChild = Children.OfType<LazyDirectoryTree>().Last();
                    bool? lastChild_oldIsSelected = lastChild.IsSelected;
                    suppress_Child_IsSelected_Changed = true;
                    foreach (var child in this.Children.Cast<LazyDirectoryTree>())
                    {
                        child.inheritIsSelectedFromParent = true;
                        child.IsSelected = this._isSelected;
                        child.inheritIsSelectedFromParent = false;
                    }
                    suppress_Child_IsSelected_Changed = false;
                    this.OnChildIsSelectedChanged(lastChild, lastChild_oldIsSelected);
                }
            }
        }
        protected virtual void OnIsSelectedChangedOverride(bool? oldVal)
        {
            IsSelectedChanged?.Invoke(this, oldVal);
        }
        private bool suppress_Child_IsSelected_Changed = false;
        private bool suppress_IsSelected_Changed = false;
        private ObservableSortedCollection<FileInfo> _fileList;

        protected virtual void OnChildIsSelectedChanged(LazyDirectoryTree sender, bool? oldVal)
        {
            if (suppress_Child_IsSelected_Changed)
                return;
            suppress_IsSelected_Changed = true;
            bool areAllChildrenSelected = Children.Cast<LazyDirectoryTree>()
                    .All(child => child.IsSelected == true || child.UnauthorisedAccess);
            bool areAnyChildrenSelected = Children.Cast<LazyDirectoryTree>()
                   .Any(child => child.IsSelected != false);
            if (areAllChildrenSelected)
            {
                IsSelected = true;
            }
            else if (!areAnyChildrenSelected)
            {
                IsSelected = false;
            }
            else
            {
                IsSelected = null;
            }
            suppress_IsSelected_Changed = false;
            /*if (suppress_Child_IsSelected_Changed)
                return;
            suppress_IsSelected_Changed = true;
            bool all = true;
            bool any = false;
            int count = Children.Count;
            for (int i = 0; i < count; i++)
            {
                var child = Children[i] as LazyDirectoryTree;
                if (child == null)
                    continue;
                if (child.IsSelected == false && child.AccessAuthorised)
                    all = false;
                if (child.IsSelected != false)
                    any = true;
                if (any & !all)
                    break;
            }
            if (all)
            {
                IsSelected = true;
            }
            else if (!any)
            {
                IsSelected = false;
            }
            else
            {
                IsSelected = null;
            }
            suppress_IsSelected_Changed = false;
            return;*/
            /*if (suppress_Child_IsSelected_Changed)
                return;
            suppress_IsSelected_Changed = true;
            if (sender.IsSelected == true)
            {
                bool areAllChildrenSelected = Children.Cast<LazyDirectoryTree>()
                    .All(child => child.IsSelected == true || child.UnauthorisedAccess);
                if (areAllChildrenSelected)
                    IsSelected = true;
                else
                    IsSelected = null;
            }
            else if (sender.IsSelected == null)
            {
                
            }
            else if (sender.IsSelected == false || sender.IsSelected == null)
            {
                bool areAnyChildrenSelected = Children.Cast<LazyDirectoryTree>()
                   .Any(child => child.IsSelected != false);
                if (areAnyChildrenSelected)
                    IsSelected = null;
                else
                    IsSelected = false;
            }
            suppress_IsSelected_Changed = false;*/
        }
        #endregion
        #region Constructors
        public LazyDirectoryTree()
            : this(Path.GetPathRoot(Environment.SystemDirectory))
        { }
        public LazyDirectoryTree(string path)
            : this(new DirectoryInfo(path))
        { }

        public LazyDirectoryTree(DirectoryInfo root)
            : this(root, directoryFactory)
        {
        }

        protected LazyDirectoryTree(DirectoryInfo root,
            Func<LazyObservableTree<DirectoryInfo>, IEnumerable<LazyObservableTree<DirectoryInfo>>> dirFactory)
            : base(root, DirectoryComparer, dirFactory)
        {
            AccessAuthorised = true;
            IsSelected = true;
        }
        #endregion
        #region Overrides
        protected override void OnAttachChild(ObservableSortedTree<DirectoryInfo> sender, ObservableSortedTree<DirectoryInfo> child)
        {
            base.OnAttachChild(sender, child);
            var dirTree = child as LazyDirectoryTree;
            if (dirTree == null)
                return;
            if (this.IsSelected == false)
                dirTree.IsSelected = false;
            else
                dirTree.IsSelected = true;
            dirTree.IsSelectedChanged += OnChildIsSelectedChanged;
        }
        protected override void OnDetachChildOverride(ObservableSortedTree<DirectoryInfo> sender, ObservableSortedTree<DirectoryInfo> child)
        {
            base.OnDetachChildOverride(sender, child);
            var dirTree = sender as LazyDirectoryTree;
            if (dirTree == null)
                return;
            dirTree.IsSelectedChanged -= OnChildIsSelectedChanged;
        }
        protected override void setChildFactory(Func<LazyObservableTree<DirectoryInfo>, IEnumerable<LazyObservableTree<DirectoryInfo>>> value)
        {
            if (initialising)
                base.setChildFactory(value);
            else
                throw new Exception($"Error: '{nameof(ChildFactory)}' cannot be set directly in a {this.GetType().Name}");
        }
        public override ObservableSortedTree<DirectoryInfo> ShallowClone()
        {
            var clone = new LazyDirectoryTree(this.Value)
            {
                IsSelected = this.IsSelected,
                IsExpanded = this.IsExpanded
            };
            return clone;
        }
        protected override void onChildrenGenerated()
        {
            base.onChildrenGenerated();
            this.FileList = new ObservableSortedCollection<FileInfo>(fileComparer);
            this.FileList.AddRange(this.getFileInfoList2());
        }
        private FileInfo[] getFileInfoList2()
        {
            try
            {
                return this.Value.EnumerateFiles("*", SearchOption.TopDirectoryOnly).ToArray();
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
            catch (Exception)
            {
                throw;
            }

        }
        //private MyFileInfo[] getFileInfoList()

        #endregion
    }
}
