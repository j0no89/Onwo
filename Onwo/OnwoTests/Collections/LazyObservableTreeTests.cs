using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Onwo.Collections;

namespace OnwoTests.Collections
{
    [TestClass]
    public class LazyObservableTreeTests
    {
        public static LazyObservableTree<DirectoryInfo> CreateTestTree()
        {
            var rootDirectory = new DirectoryInfo(@"C:\");
            var directoryComparer = Comparer<DirectoryInfo>.Create((d1, d2) =>
                string.Compare(d1.Name, d2.Name, StringComparison.InvariantCultureIgnoreCase));
            var rootTree = new LazyObservableTree<DirectoryInfo>(rootDirectory, directoryComparer,
                tree =>
                {
                    if (tree.Value == null)
                        return new LazyObservableTree<DirectoryInfo>[0];
                    try
                    {
                        var dirs = tree.Value.EnumerateDirectories("*", SearchOption.TopDirectoryOnly).ToArray();
                        var children = dirs.Select(dir => {
                            var ty = new LazyObservableTree<DirectoryInfo>(dir, tree.Comparer, tree.ChildFactory);
                            return ty;
                        }).ToArray();
                        return children;
                    }
                    catch (Exception)
                    {
                        return new LazyObservableTree<DirectoryInfo>[0];
                    }
                });
            return rootTree;
        }

        [TestMethod]
        public void testChildGeneration()
        {
            var root = CreateTestTree();
            int count = root.AsEnumerable().Count();
            Assert.IsTrue(count == 1);
            Assert.IsFalse(root.AreChildrenGenerated);
            root.GenerateChildren();
            count = root.AsEnumerable().Count();
            Assert.IsFalse(count == 1);
            Assert.IsTrue(root.AreChildrenGenerated);
            Assert.IsTrue(root.Children.Count == root.Value.EnumerateDirectories("*", SearchOption.TopDirectoryOnly).Count());
            foreach (var tree in root.Children)
            {
                var ltree = tree as LazyObservableTree<DirectoryInfo>;
                Assert.IsFalse(ltree == null);
                Assert.IsFalse(ltree.AreChildrenGenerated);
            }
        }
        [TestMethod]
        public void TestChildRegenerationOnChildFactoryChange()
        {
            var root = CreateTestTree();
            var oldChildren = root.Children;
            var expectedNames = root.Value.EnumerateDirectories("*", SearchOption.TopDirectoryOnly)
                .Select(dir => dir.Name + "_Test")
                .Reverse()
                .OrderBy(dir => dir, DefaultComparers.StringComparer_IgnoreCase)
                .ToArray();
            bool isChildListChanged = false;
            root.ChildListChanged += (sender, old) =>
            {
                isChildListChanged = true;
                Assert.IsTrue(ReferenceEquals(sender, root));
                Assert.IsTrue(ReferenceEquals(old, oldChildren));
            };

            root.ChildFactory = tree =>
            {
                var dirs = tree.Value.EnumerateDirectories("*", SearchOption.TopDirectoryOnly).ToArray();
                var children = dirs.Select(dir =>
                {
                    var newDi = new DirectoryInfo(dir.FullName + "_Test");
                    var ty = new LazyObservableTree<DirectoryInfo>(newDi, tree.Comparer, tree.ChildFactory);
                    return ty;
                }).ToArray();
                return children;
            };

            Assert.IsTrue(isChildListChanged);
            var newChildNames = root.Children.Select(child => child.Value.Name)
                .Reverse()
                .OrderBy(dir => dir, DefaultComparers.StringComparer_IgnoreCase)
                .ToArray();
            Assert.AreEqual(expectedNames.Length, newChildNames.Length);
            for (int i = 0; i < expectedNames.Length; i++)
            {
                string name1 = expectedNames[i];
                string name2 = newChildNames[i];
                bool namesAreEqual = string.Equals(name1, name2, StringComparison.InvariantCultureIgnoreCase);
                Assert.IsTrue(namesAreEqual);
            }
            int fgs = 0;
        }
        [TestMethod]
        public void TestChildrenDontGenerateOnComparerChange()
        {
            var root = CreateTestTree();
            root.GenerateChildren();
            var tmpComparer = root.Comparer;
            var comparer =
                Comparer<ObservableSortedTree<DirectoryInfo>>.Create((val1, val2) => tmpComparer.Compare(val2, val1));
            root.Comparer = comparer;
            foreach (var tree in root.EnumerateDescendants(true))
            {
                Assert.IsTrue(ReferenceEquals(comparer, tree.Comparer));
                var lTree = tree as LazyObservableTree<DirectoryInfo>;
                Assert.IsFalse(lTree == null);
                Assert.IsFalse(lTree.AreChildrenGenerated);
            }
        }


        [TestMethod]
        public void TestComparerEqualAndChildReorderingOnAddNewChildList()
        {
            /*var root = CreateTestTree();
            var oldComparer = root.Comparer;
            var reverseComparer = Comparer<ObservableSortedTree<ValueClass>>.Create(
                (tree1, tree2) => oldComparer.Compare(tree2, tree1));
            var obs = new ObservableSortedCollection<ObservableSortedTree<ValueClass>>(oldComparer);
            foreach (var child in root.Children.ToArray())
            {
                root.Children.Remove(child);
                obs.Add(child);
            }
            root.Comparer = reverseComparer;
            root.Children = obs;
            foreach (var tree in root.EnumerateDescendantsAndSelf())
            {
                Assert.IsTrue(ReferenceEquals(tree.Comparer, reverseComparer));
                Assert.IsTrue(ReferenceEquals(tree.Children.Comparer, reverseComparer));
                for (int i = 1; i < tree.Children.Count; i++)
                {
                    Assert.IsFalse(reverseComparer.Compare(tree.Children[i - 1], tree.Children[i]) == 1, $"Items are not in order at elements {i - 1} & {i}");
                }
            }*/
        }
        [TestMethod]
        public void TestChildListChangedEvent()
        {
            var root = CreateTestTree();
            Assert.IsFalse(root.AreChildrenGenerated);

            var oldChildList = (ObservableSortedCollection<ObservableSortedTree<DirectoryInfo>>)null;
            bool childListChanged_raised = false;
            root.ChildListChanged += (sender, oldChildren) =>
            {
                childListChanged_raised = true;
                Assert.IsTrue(ReferenceEquals(sender, root));
                Assert.IsTrue(ReferenceEquals(oldChildren, oldChildList));
            };
            root.GenerateChildren();
            Assert.IsTrue(childListChanged_raised);
            /*var root2 = CreateTestTree();
            var newChildList = new ObservableSortedCollection<ObservableSortedTree<ValueClass>>();
            foreach (var child in root2.Children.ToArray())
            {
                root2.Children.Remove(child);
                newChildList.Add(child);
            }
            root.Children = newChildList;
            Assert.IsTrue(childListChanged_raised);

            foreach (var child in oldChildList)
            {
                Assert.IsNull(child.Parent);
                foreach (var desc in child)
                {
                    Assert.IsTrue(ReferenceEquals(desc.Root, child));
                }
            }
            foreach (var child in root.Children)
            {
                Assert.IsTrue(ReferenceEquals(child.Parent, root));
                foreach (var desc in child)
                {
                    Assert.IsTrue(ReferenceEquals(desc.Root, root));
                }
            }*/
        }
        [TestMethod]
        public void TestHasChildrenPropertyChangedRaisedCorrectly()
        {
            /*var root = CreateTestTree();
            Assert.IsFalse(root.HasChildren);
            var testChild = root.Children[0];
            Assert.IsTrue(root.HasChildren);
            string hasChildrenPropertyName = nameof(ObservableSortedTree<ValueClass>.HasChildren);
            bool hasChildrenPropertyChanged_raised = false;
            testChild.PropertyChanged += (sender, args) =>
            {
                var pName = args?.PropertyName;
                if (string.Equals(pName, hasChildrenPropertyName))
                    hasChildrenPropertyChanged_raised = true;
            };
            while (testChild.Children.Count > 1)
            {
                //var cChild = testChild.Children[testChild.Children.Count - 1];
                testChild.Children.RemoveAt(testChild.Children.Count - 1);
                Assert.IsTrue(testChild.HasChildren);
            }
            //when last item is removed
            var lastChild = testChild.Children[0];
            testChild.Children.RemoveAt(0);
            Assert.IsTrue(hasChildrenPropertyChanged_raised);
            Assert.IsFalse(testChild.HasChildren);
            //when first item is added
            hasChildrenPropertyChanged_raised = false;
            testChild.Children.Add(lastChild);
            Assert.IsTrue(hasChildrenPropertyChanged_raised);
            Assert.IsTrue(testChild.HasChildren);

            //non-empty to null
            hasChildrenPropertyChanged_raised = false;
            testChild.Children = null;
            Assert.IsTrue(hasChildrenPropertyChanged_raised);
            Assert.IsFalse(testChild.HasChildren);

            //null to empty  
            hasChildrenPropertyChanged_raised = false;
            testChild.Children = new ObservableSortedCollection<ObservableSortedTree<ValueClass>>(root.Comparer);
            Assert.IsFalse(hasChildrenPropertyChanged_raised);
            Assert.IsFalse(testChild.HasChildren);

            //empty to empty  
            hasChildrenPropertyChanged_raised = false;
            testChild.Children = new ObservableSortedCollection<ObservableSortedTree<ValueClass>>(root.Comparer);
            Assert.IsFalse(hasChildrenPropertyChanged_raised);
            Assert.IsFalse(testChild.HasChildren);

            //empty to null  
            hasChildrenPropertyChanged_raised = false;
            testChild.Children = null;
            Assert.IsFalse(hasChildrenPropertyChanged_raised);
            Assert.IsFalse(testChild.HasChildren);

            //null to null  
            hasChildrenPropertyChanged_raised = false;
            testChild.Children = null;
            Assert.IsFalse(hasChildrenPropertyChanged_raised);
            Assert.IsFalse(testChild.HasChildren);

            //null to non-empty
            var root2 = CreateTestTree();
            var obs = new ObservableSortedCollection<ObservableSortedTree<ValueClass>>(root.Comparer);
            obs.Comparer = root.Comparer;
            foreach (var child in root2.Children.ToArray())
            {
                root2.Children.Remove(child);
                obs.Add(child);
            }
            hasChildrenPropertyChanged_raised = false;
            testChild.Children = obs;
            Assert.IsTrue(hasChildrenPropertyChanged_raised);
            Assert.IsTrue(testChild.HasChildren);

            //non-empty to empty
            hasChildrenPropertyChanged_raised = false;
            testChild.Children = new ObservableSortedCollection<ObservableSortedTree<ValueClass>>(root.Comparer);
            Assert.IsTrue(hasChildrenPropertyChanged_raised);
            Assert.IsFalse(testChild.HasChildren);

            //empty to non-empty
            root2 = CreateTestTree();
            obs = new ObservableSortedCollection<ObservableSortedTree<ValueClass>>(root.Comparer);
            obs.Comparer = root.Comparer;
            foreach (var child in root2.Children.ToArray())
            {
                root2.Children.Remove(child);
                obs.Add(child);
            }
            hasChildrenPropertyChanged_raised = false;
            testChild.Children = obs;
            Assert.IsTrue(hasChildrenPropertyChanged_raised);
            Assert.IsTrue(testChild.HasChildren);

            //non-empty to non-empty
            root2 = CreateTestTree();
            obs = new ObservableSortedCollection<ObservableSortedTree<ValueClass>>(root.Comparer);
            obs.Comparer = root.Comparer;
            foreach (var child in root2.Children.ToArray())
            {
                root2.Children.Remove(child);
                obs.Add(child);
            }
            hasChildrenPropertyChanged_raised = false;
            testChild.Children = obs;
            Assert.IsFalse(hasChildrenPropertyChanged_raised);
            Assert.IsTrue(testChild.HasChildren);
            */
            //null to non-empty         ***
            //null to empty             ***
            //empty to non-empty        ***
            //non-empty to non-empty    ***
            //empty to empty            ***
            //non-empty to empty        ***
            //empty to null             ***
            //non-empty to null         ***
            //null to null              ***
        }
    }
}
