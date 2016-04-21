using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Onwo.IO;

namespace Onwo.Collections.Special
{
    public class LinkedDirectoryTree2 : INotifyPropertyChanged
    {
        private LazyDirectoryTree _source;
        private ObservableSortedTree<DirectoryInfo> _destRoot;

        public LazyDirectoryTree Source
        {
            get { return _source; }
            set
            {
                if (ReferenceEquals(_source, value)) return;
                var old = _source;
                _source = value;
                OnPropertyChanged(nameof(Source));
                onSourceChanged(old);
            }
        }

        
        private void onSourceChanged(LazyDirectoryTree oldSource)
        {
            if (oldSource != null)
            {
                oldSource.ComparerChanged -= Source_ComparerChanged;
                foreach (var tree in oldSource.EnumerateDescendantsAndSelf(true).Cast<LazyDirectoryTree>())
                {
                    tree.ChildTreeAttached -= Source_ChildTreeAttached;
                    tree.ChildTreeDetached -= Source_ChildTreeDetached;
                    tree.IsSelectedChanged -= Tree_IsSelectedChanged;
                }
            }
            if (this.Source == null)
                this.DestRoot = null;
            else
            {
                this.DestRoot = cloneSource(this.Source);
                Source.ComparerChanged += Source_ComparerChanged;
                foreach (var tree in this.Source.EnumerateDescendantsAndSelf(true).Cast<LazyDirectoryTree>())
                {
                    tree.ChildTreeAttached += Source_ChildTreeAttached;
                    tree.ChildTreeDetached += Source_ChildTreeDetached;
                    tree.IsSelectedChanged += Tree_IsSelectedChanged;
                }
            }
        }
        private void Source_ComparerChanged(ObservableSortedTree<DirectoryInfo> sender, IComparer<ObservableSortedTree<DirectoryInfo>> oldCOmparer)
            => this.DestRoot.Comparer = Source.Comparer;

        /*private ObservableSortedCollection<ObservableSortedTree<DirectoryInfo>> _dest;
        public ObservableSortedCollection<ObservableSortedTree<DirectoryInfo>> Dest
        {
            get { return _dest; }
            set
            {
                if (ReferenceEquals(_dest, value)) return;
                _dest = value;
                OnPropertyChanged(nameof(Dest));
            }
        }*/

        public ObservableSortedTree<DirectoryInfo> DestRoot
        {
            get { return _destRoot; }
            set
            {
                if (ReferenceEquals(_destRoot, value)) return;
                _destRoot = value;
                OnPropertyChanged(nameof(DestRoot));
            }
        }

        public LinkedDirectoryTree2(LazyDirectoryTree sourceTree)
        {
            this.Source = sourceTree;

        }
        private LazyDirectoryTree FindDirectoryInSource(DirectoryInfo dir)
        {
            var sComparer = DefaultComparers.NoCaseStringEquals;
            var splitPath = dir.SplitDirectoryPath();
            ObservableSortedTree<DirectoryInfo> cTree = Source;
            if (!sComparer.Equals(cTree.Value.FullName.TrimEnd('\\'), splitPath[0]))
            {
                return null;
            }
            for (int i = 1; i < splitPath.Length; i++)
            {
                var matchTree = cTree.Children.FirstOrDefault(
                    child => sComparer.Equals(child.Value.Name, splitPath[i]));
                if (matchTree == null)
                {
                    return null;
                }
                cTree = matchTree;
            }
            return cTree as LazyDirectoryTree;
        }
        /// <summary>
        /// Finds and returns the node for a given directory from the destination tree. Creates the node if it doesnt exist.
        /// </summary>
        /// <param name="dirToAdd">The directory to add the destination tree</param>
        private ObservableSortedTree<DirectoryInfo> FindOrAddDirectoryToDest(DirectoryInfo dirToAdd)
        {
            var sComparer = DefaultComparers.NoCaseStringEquals;
            var splitPath = dirToAdd.SplitDirectoryPath();
            var cTree = DestRoot;
            if (!sComparer.Equals(cTree.Value.FullName.TrimEnd('\\'), splitPath[0]))
            {
                return null;
            }
            for (int i = 1; i < splitPath.Length; i++)
            {
                var matchTree = cTree.Children.FirstOrDefault(
                    child => sComparer.Equals(child.Value.Name, splitPath[i]));
                if (matchTree == null)
                {
                    var tmpDir = new DirectoryInfo(splitPath[i]);
                    matchTree = new ObservableSortedTree<DirectoryInfo>(tmpDir);
                    cTree.Children.Add(matchTree);
                }
                cTree = matchTree;
            }
            return cTree;
        }
        private ObservableSortedTree<DirectoryInfo> FindDirectoryInDest(DirectoryInfo dirToAdd)
        {
            var sComparer = DefaultComparers.NoCaseStringEquals;
            var splitPath = dirToAdd.SplitDirectoryPath();
            var cTree = DestRoot;
            if (!sComparer.Equals(cTree.Value.FullName.TrimEnd('\\'), splitPath[0]))
            {
                return null;
            }
            for (int i = 1; i < splitPath.Length; i++)
            {
                var matchTree = cTree.Children.FirstOrDefault(
                    child => sComparer.Equals(child.Value.Name, splitPath[i]));
                if (matchTree == null)
                {
                    return null;
                }
                cTree = matchTree;
            }
            return cTree;
        }
        private void RemoveDirectoryFromDest(DirectoryInfo dirToAdd)
        {
            var sComparer = DefaultComparers.NoCaseStringEquals;
            var splitPath = dirToAdd.SplitDirectoryPath();
            var cTree = DestRoot;
            if (!sComparer.Equals(cTree.Value.FullName.TrimEnd('\\'), splitPath[0]))
            {
                return;
            }
            for (int i = 1; i < splitPath.Length; i++)
            {
                var matchTree = cTree.Children.FirstOrDefault(
                    child => sComparer.Equals(child.Value.Name, splitPath[i]));
                if (matchTree == null)
                {
                    return;
                }
                cTree = matchTree;
            }
            cTree.Prune();
        }
        private void Tree_IsSelectedChanged(LazyDirectoryTree sender, bool? oldVal)
        {
            if (oldVal == false && sender.IsSelected != false)
            {
                FindOrAddDirectoryToDest(sender.Value);
            }
            else if (oldVal != false && sender.IsSelected == false)
            {
                RemoveDirectoryFromDest(sender.Value);
            }
        }

        private void Source_ChildTreeDetached(ObservableSortedTree<DirectoryInfo> sender, ObservableSortedTree<DirectoryInfo> child)
        {
            RemoveDirectoryFromDest(child.Value);
            child.ChildTreeAttached -= Source_ChildTreeAttached;
            child.ChildTreeDetached -= Source_ChildTreeDetached;
            var tmp = child as LazyDirectoryTree;
            if (tmp != null)
                tmp.IsSelectedChanged -= Tree_IsSelectedChanged;
        }

        private void Source_ChildTreeAttached(ObservableSortedTree<DirectoryInfo> sender, ObservableSortedTree<DirectoryInfo> child)
        {
            var tmp = child as LazyDirectoryTree;
            if (tmp != null)
            {
                tmp.IsSelectedChanged += Tree_IsSelectedChanged;
                if (tmp.IsSelected != false)
                    FindOrAddDirectoryToDest(child.Value);
            }
            child.ChildTreeAttached += Source_ChildTreeAttached;
            child.ChildTreeDetached += Source_ChildTreeDetached;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableSortedTree<DirectoryInfo> cloneSource(LazyDirectoryTree cloneTree)
        {
            var root = new ObservableSortedTree<DirectoryInfo>(cloneTree.Value);
            if (!cloneTree.AreChildrenGenerated)
                return root;
            var newChildren = cloneTree.Children
                .Cast<LazyDirectoryTree>()
                .Select(child => cloneSource(child));
            root.Children.AddRange(newChildren);
            return root;
        }
    }
}
