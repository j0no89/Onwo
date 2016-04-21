using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Onwo.Input;

namespace Onwo.Collections.Special
{
    using DirectoryFactoryFunc = Func<LazyObservableTree<DirectoryInfo>, IEnumerable<LazyObservableTree<DirectoryInfo>>>;

    public class CategoryDirectoryList : ObservableSortedCollection<CategoryDirectoryInfo>
    {
        private static int CompareCategoryInfo(CategoryDirectoryInfo cat1, CategoryDirectoryInfo cat2)
        {
            var str1 = cat1.SourceRoot.Value.FullName;
            var str2 = cat2.SourceRoot.Value.FullName;
            return string.Compare(str1, str2, StringComparison.InvariantCultureIgnoreCase);
        }

        private static IComparer<CategoryDirectoryInfo> categoryDirectoryComparer =
            Comparer<CategoryDirectoryInfo>.Create((cat1, cat2) => CompareCategoryInfo(cat1, cat2));
        public CategoryDirectoryList()
            :base(categoryDirectoryComparer)
        {
            
        }
    }
    public class CategoryDirectoryInfo2 : INotifyPropertyChanged
    {
        private ObservableSortedCollection<LinkedLayTree> _linkedRootList;
        private LazyDirectoryTree _selectedSourceTree;
        private LinkedLayTree _selectedLinkTree;
        private ObservableSortedCollection<DirectoryInfo> _dirList;
        //public LazyDirectoryTree SourceRoot => LinkedRoot.Source;

        public ObservableSortedCollection<LinkedLayTree> LinkedRootList
        {
            get { return _linkedRootList; }
            set
            {
                if (ReferenceEquals(value, _linkedRootList)) return;
                _linkedRootList = value;
                OnPropertyChanged(nameof(LinkedRootList));
                //OnPropertyChanged(nameof(SourceRoot));
            }
        }

        private ObservableSortedCollection<LazyDirectoryTree> _sourceRootList;
        

        public ObservableSortedCollection<LazyDirectoryTree> SourceRootList
        {
            get { return _sourceRootList; }
            set
            {
                if (ReferenceEquals(value, _sourceRootList)) return;
                _sourceRootList = value;
                OnPropertyChanged(nameof(SourceRootList));
                //OnPropertyChanged(nameof(SourceRoot));
            }
        }

        public LazyDirectoryTree SelectedSourceTree
        {
            get { return _selectedSourceTree; }
            set
            {
                if (ReferenceEquals(value, _selectedSourceTree)) return;
                _selectedSourceTree = value;
                OnPropertyChanged(nameof(SelectedSourceTree));
            }
        }

        public LinkedLayTree SelectedLinkTree
        {
            get { return _selectedLinkTree; }
            set
            {
                if (ReferenceEquals(value, _selectedLinkTree)) return;
                _selectedLinkTree = value;
                OnPropertyChanged(nameof(SelectedLinkTree));
                //_selectedLinkTree.SelectedFiles=new ObservableCollection<FileInfo>();
            }
        }

        public ObservableSortedCollection<DirectoryInfo> DirList
        {
            get { return _dirList; }
            set
            {
                if (ReferenceEquals(value, _dirList)) return;
                var old = _dirList;
                _dirList = value;
                OnPropertyChanged(nameof(DirList));
                onDirListChanged(old);
            }
        }

        private void onDirListChanged(ObservableSortedCollection<DirectoryInfo> old)
        {
            if (old != null)
            {
                old.ItemAdded -= DirList_ItemAdded;
                old.ItemRemoved -= DirList_ItemRemoved;
                old.ItemChanged -= DirList_ItemChanged;
            }
            if (this.DirList != null)
            {
                this.DirList.ItemAdded += DirList_ItemAdded;
                this.DirList.ItemRemoved += DirList_ItemRemoved;
                this.DirList.ItemChanged += DirList_ItemChanged;
            }
            
        }

        private void DirList_ItemChanged(ICollection<DirectoryInfo> sender, DirectoryInfo oldItem, DirectoryInfo newItem, int index)
        {
            DirList_ItemRemoved(sender, oldItem, index);
            DirList_ItemAdded(sender, newItem, index);
        }

        private void DirList_ItemRemoved(ICollection<DirectoryInfo> sender, DirectoryInfo oldItem, int index)
        {
            int ind = -1;
            for (ind = 0; ind < this.LinkedRootList.Count; ind++)
            {
                bool isEqual = string.Equals(this.LinkedRootList[ind].Value.FullName, oldItem.FullName,
                    StringComparison.InvariantCultureIgnoreCase);
                if (isEqual)
                {
                    break;
                }
            }
            if (ind < this.LinkedRootList.Count)
                this.LinkedRootList.RemoveAt(ind);
            for (ind = 0; ind < this.SourceRootList.Count; ind++)
            {
                bool isEqual = string.Equals(this.SourceRootList[ind].Value.FullName, oldItem.FullName,
                    StringComparison.InvariantCultureIgnoreCase);
                if (isEqual)
                {
                    break;
                }
            }
            if (ind < this.SourceRootList.Count)
                this.SourceRootList.RemoveAt(ind);
        }

        private void DirList_ItemAdded(ICollection<DirectoryInfo> sender, DirectoryInfo newItem, int index)
        {
            var newLazyDirectoryTree = new LazyDirectoryTree(newItem);
            var newLinkedTree = new LinkedLayTree(newLazyDirectoryTree);
            this.LinkedRootList.Add(newLinkedTree);
            this.SourceRootList.Add(newLazyDirectoryTree);
        }

        public CategoryDirectoryInfo2()
        {
            this.LinkedRootList=new ObservableSortedCollection<LinkedLayTree>(LinkedTreeComparer);
            this.SourceRootList=new ObservableSortedCollection<LazyDirectoryTree>(LazyDirectoryTree.DirectoryComparer);
            this.DirList=new ObservableSortedCollection<DirectoryInfo>(DirectoryComparer);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static readonly IComparer<LinkedLayTree> LinkedTreeComparer =
            Comparer<LinkedLayTree>.Create((link1, link2) => CompareLinkedTrees(link1, link2));
        private static int CompareLinkedTrees(LinkedLayTree link1, LinkedLayTree link2)
        {
            return string.Compare(link1.Source.Value.FullName,
                link2.Source.Value.FullName,
                StringComparison.InvariantCultureIgnoreCase);
        }
        private static readonly IComparer<DirectoryInfo> DirectoryComparer =
            Comparer<DirectoryInfo>.Create((d1, d2) => CompareDirectoryInfo(d1, d2));
        private static int CompareDirectoryInfo(DirectoryInfo d1, DirectoryInfo d2)
        {
            return string.Compare(d1.FullName,
                d2.FullName,
                StringComparison.InvariantCultureIgnoreCase);
        }
    }
    public class CategoryDirectoryInfo:INotifyPropertyChanged
    {
        private LinkedLayTree _linkedRoot;
        private LazyDirectoryTree _selectedSourceTree;
        private LinkedLayTree _selectedLinkTree;
        public LazyDirectoryTree SourceRoot => LinkedRoot.Source;

        public LinkedLayTree LinkedRoot
        {
            get { return _linkedRoot; }
            set
            {
                if (ReferenceEquals(value, _linkedRoot)) return;
                _linkedRoot = value;
                OnPropertyChanged(nameof(LinkedRoot));
                OnPropertyChanged(nameof(SourceRoot));
            }
        }

        public LazyDirectoryTree SelectedSourceTree
        {
            get { return _selectedSourceTree; }
            set
            {
                if (ReferenceEquals(value, _selectedSourceTree)) return;
                _selectedSourceTree = value;
                OnPropertyChanged(nameof(SelectedSourceTree));
            }
        }

        public LinkedLayTree SelectedLinkTree
        {
            get { return _selectedLinkTree; }
            set
            {
                if (ReferenceEquals(value, _selectedLinkTree)) return;
                _selectedLinkTree = value;
                OnPropertyChanged(nameof(SelectedLinkTree));
            }
        }

        public CategoryDirectoryInfo(LazyDirectoryTree sourceRoot)
        {
            this.LinkedRoot=new LinkedLayTree(sourceRoot);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class LinkedLayTree:LazyDirectoryTree
    {
        private static readonly DirectoryFactoryFunc slaveChilFunc = root => getChildrenFromSource(root);
        private LazyDirectoryTree _source;
        private ICommand _commandTest;
        private IList<object> _selectedFiles;
        private ICommand _selectedFilesChangedCommand;

        public ICommand CommandTest
        {
            get { return _commandTest; }
            set
            {
                if (ReferenceEquals(value, _commandTest)) return;
                _commandTest = value;
                OnPropertyChanged(nameof(CommandTest));
            }
        }
        private static IEnumerable<LazyObservableTree<DirectoryInfo>> getChildrenFromSource(LazyObservableTree<DirectoryInfo> root)
        {
            Logger.WriteLine($"GenerateCildren: this= {root.Value.FullName}");
            var linkTree = root as LinkedLayTree;
            if (linkTree==null)
                return new LinkedLayTree[0];
            if (!linkTree.Source.AreChildrenGenerated)
            {
                //linkTree.RemoveSourceEvents(linkTree.Source);
                linkTree.Source.GenerateChildren();
                //linkTree.AddSourceEvents(linkTree.Source);
                return new LinkedLayTree[0];
            }
            var selectedChildren = linkTree.Source.Children.Cast<LazyDirectoryTree>()
                .Where(child => child.IsSelected != false)
                .ToArray();
            var newChildren = selectedChildren.Select(child =>
                {
                    var newTree = new LinkedLayTree(child);
                    return newTree;
                });
            return newChildren;
        }

        public LazyDirectoryTree Source
        {
            get { return _source; }
            set
            {
                if (ReferenceEquals(value, _source)) return;
                var old = _source;
                _source = value;
                OnPropertyChanged(nameof(Source));
                onSourceChanged(old);
            }
        }

        private void AddSourceEvents(LazyDirectoryTree source)
        {
            if (source == null)
                return;
            source.ComparerChanged += Source_ComparerChanged;
            source.ChildTreeAttached += Source_ChildTreeAttached;
            source.ChildTreeDetached += Source_ChildTreeDetached;
            source.IsSelectedChanged += Source_IsSelectedChanged;
            if (this.AreChildrenGenerated)
            {
                foreach (var child in this.Children.Cast<LazyDirectoryTree>())
                {
                    child.IsSelectedChanged += SourceChild_IsSelectedChanged;
                }
            }
        }

        private void RemoveSourceEvents(LazyDirectoryTree source)
        {
            if (source == null)
                return;
            source.ComparerChanged -= Source_ComparerChanged;
            source.ChildTreeAttached -= Source_ChildTreeAttached;
            source.ChildTreeDetached -= Source_ChildTreeDetached;
            source.IsSelectedChanged -= Source_IsSelectedChanged;
            if (this.AreChildrenGenerated)
            {
                foreach (var child in this.Children.Cast<LazyDirectoryTree>())
                {
                    child.IsSelectedChanged -= SourceChild_IsSelectedChanged;
                }
            }
        }
        protected virtual void onSourceChanged(LazyDirectoryTree oldSource)
        {
            var oldSourceString = "null";
            if (oldSource != null)
                oldSourceString = oldSource.ToString();
            Logger.WriteLine($"{nameof(onSourceChanged)}: old= {oldSourceString}  this= {this.Value.FullName}");
            RemoveSourceEvents(oldSource);
            AddSourceEvents(this.Source);
        }
        private void Source_ComparerChanged(ObservableSortedTree<DirectoryInfo> sender, IComparer<ObservableSortedTree<DirectoryInfo>> oldCOmparer)
            => this.Comparer = Source.Comparer;
        private void Source_IsSelectedChanged(LazyDirectoryTree sender, bool? oldVal)
        {
            Logger.WriteLine($"{nameof(Source_IsSelectedChanged)}: Sender= {sender.Value.FullName}  old= {oldVal.ToString()}  this= {this.Value.FullName}");
            if (sender.IsSelected == false)
            {
                this.Prune();
            }
        }
        private void SourceChild_IsSelectedChanged(LazyDirectoryTree sender, bool? oldVal)
        {
            Logger.WriteLine($"{nameof(SourceChild_IsSelectedChanged)}: Sender= {sender.Value.FullName}  old= {oldVal.ToString()}  this= {this.Value.FullName}");
            if (sender.IsSelected == true)
            {
                if(oldVal == null)
                {
                    return;
                }
                else if (oldVal == false)
                {
                    if (this.AreChildrenGenerated)
                        this.Children.Add(new LinkedLayTree(sender));
                }
            }
            else if (sender.IsSelected == null)
            {
                if (oldVal == true)
                {
                    return;
                }
                else if (oldVal == false)
                {
                    if (this.AreChildrenGenerated)
                        this.Children.Add(new LinkedLayTree(sender));
                }
            }
            /*if (oldVal == false && sender.IsSelected != false)
            {
                FindOrAddDirectoryToDest(sender.Value);
            }
            else if (oldVal != false && sender.IsSelected == false)
            {
                RemoveDirectoryFromDest(sender.Value);
            }*/
        }
        private void Source_ChildTreeDetached(ObservableSortedTree<DirectoryInfo> sender, ObservableSortedTree<DirectoryInfo> child)
        {
            Logger.WriteLine($"{nameof(Source_ChildTreeDetached)}: Sender= {sender.Value.FullName}  Child= {child.Value.FullName}  this= {this.Value.FullName}");
            //child.ChildTreeAttached -= Source_ChildTreeAttached;
            //child.ChildTreeDetached -= Source_ChildTreeDetached;
            var lchild = child as LazyDirectoryTree;
            if (lchild != null)
            {
                lchild.IsSelectedChanged -= SourceChild_IsSelectedChanged;
                if (lchild.IsSelected != false)
                {
                    if (this.AreChildrenGenerated)
                    {
                        int index;
                        for (index = 0; index < this.Children.Count; index++)
                        {
                            var linkChild = this.Children[index] as LinkedLayTree;
                            if (linkChild == null)
                                continue;
                            if (ReferenceEquals(lchild, linkChild.Source))
                                break;
                        }
                        if (index == this.Children.Count)
                            return;
                        this.Children.RemoveAt(index);
                    }
                }
            }
        }

        private void Source_ChildTreeAttached(ObservableSortedTree<DirectoryInfo> sender, ObservableSortedTree<DirectoryInfo> child)
        {
            Logger.WriteLine($"{nameof(Source_ChildTreeAttached)}: Sender= {sender.Value.FullName}  Child= {child.Value.FullName}  this= {this.Value.FullName}");

            var lchild = child as LazyDirectoryTree;
            if (lchild != null)
            {
                lchild.IsSelectedChanged += SourceChild_IsSelectedChanged;
                if (lchild.IsSelected != false)
                {
                    if (this.AreChildrenGenerated)
                    {
                        var newChild = new LinkedLayTree(lchild);
                        this.Children.Add(newChild);
                    }
                    //FindOrAddDirectoryToDest(child.Value);
                }
                //child.ChildTreeAttached += Source_ChildTreeAttached;
                //child.ChildTreeDetached += Source_ChildTreeDetached;
            }
            
        }

        public IList<object> SelectedFiles
        {
            get { return _selectedFiles; }
            set
            {
                if (value == null)
                {
                    int yui = 0;
                }
                if (ReferenceEquals(value, _selectedFiles)) return;
                _selectedFiles = value;
                OnPropertyChanged(nameof(SelectedFiles));
            }
        }

        public ICommand SelectedFilesChangedCommand
        {
            get { return _selectedFilesChangedCommand; }
            set
            {
                if (ReferenceEquals(value, _selectedFilesChangedCommand))
                    return;
                _selectedFilesChangedCommand = value;
                OnPropertyChanged(nameof(SelectedFilesChangedCommand));
            }
        }

        public LinkedLayTree(LazyDirectoryTree source)
            : base(source.Value, slaveChilFunc)
        {
            Logger.WriteLine($"new {typeof(LinkedLayTree).Name}; {source.Value.FullName}");
            this.Source = source;
            //this.SelectedFiles=new ObservableCollection<FileInfo>();
            this.CommandTest = new DelegateCommand(obj =>
            {

            }); //new DelegateCommand
            this.SelectedFilesChanged += LinkedLayTree_SelectedFilesChanged;
            this.SelectedFilesChangedCommand = new DelegateCommand((obj) => { return; });
        }

        private void LinkedLayTree_SelectedFilesChanged(object sender, SelectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public event SelectionChangedEventHandler SelectedFilesChanged;
        
    }
}
