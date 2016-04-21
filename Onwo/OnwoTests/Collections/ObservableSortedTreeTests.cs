using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Onwo;
using Onwo.Collections;

namespace OnwoTests.Collections
{
    [TestClass]
    public class ObservableSortedTreeTests
    {
        public virtual ObservableSortedTree<ValueClass> CreateTestTree()
        {
            Random rand = new Random(0x11111111);

            var comparer = Comparer<ValueClass>.Create((v1, v2) =>
            {
                int compare = v1.Number.CompareTo(v2.Number); 
                if (compare == 0)
                    compare = string.Compare(v1.Text, v2.Text, StringComparison.InvariantCulture);
                return compare;
            });
            var root = new ObservableSortedTree<ValueClass>(
                new ValueClass(rand), comparer);
            int numChildren = rand.Next(5, 10);
            for (int i = 0; i < numChildren; i++)
                root.Children.Add(new ObservableSortedTree<ValueClass>(new ValueClass(rand)));
            var stack = new Stack<ObservableSortedTree<ValueClass>>(root.Children.Reverse());

            while (stack.Count > 0)
            {
                var cTree = stack.Pop();
                int maxChildren = 5;
                int depth = cTree.EnumerateAncestorsAndSelf().Count() - 1;
                if (depth > 5)
                    continue;
                if (depth > 4)
                    maxChildren = 2;
                else if (depth > 3)
                    maxChildren = 3;
                numChildren = rand.Next(0, maxChildren);
                for (int i = 0; i < numChildren; i++)
                {
                    cTree.Children.Add(new ObservableSortedTree<ValueClass>(new ValueClass(rand)));
                    //stack.Push(cTree.Children[i]);
                }
                foreach (var child in cTree.Children.Reverse())
                {
                    stack.Push(child);
                }
            }
            return root;
        }
        [TestMethod]
        public void TestAllChildElementsHaveCorrectParent()
        {
            var root = CreateTestTree();
            foreach (var tree in root.AsEnumerable())
            {
                foreach (var child in tree.Children)
                {
                    Assert.IsTrue(ReferenceEquals(child.Parent, tree));
                }
            }
        }
        [TestMethod]
        public void TestAllChildElementsHaveCorrectRoot()
        {
            var root = CreateTestTree();
            foreach (var tree in root.AsEnumerable())
            {
                Assert.IsTrue(ReferenceEquals(tree.Root, root));
            }
        }
        [TestMethod]
        public void TestComparersAllEqual()
        {
            var root = CreateTestTree();
            foreach (var tree in root.AsEnumerable())
            {
                Assert.IsTrue(ReferenceEquals(tree.Comparer, root.Comparer));
                Assert.IsTrue(ReferenceEquals(tree.Children.Comparer, root.Comparer));
                Assert.IsTrue(ReferenceEquals(tree.Children.Comparer, tree.Comparer));
            }
        }
        [TestMethod]
        public void TestComparersEqualAfterUpdate()
        {
            var root = CreateTestTree();
            var oldComparer = root.Comparer;
            var valueComparer = Comparer<ValueClass>.Create((v1, v2) =>
                v1.GetHashCode().CompareTo(v2.GetHashCode()));
            var comparer = ObservableSortedTree<ValueClass>.TransformValueToTreeComparer(valueComparer);
            root.Comparer = comparer;
            foreach (var tree in root.AsEnumerable())
            {
                Assert.IsTrue(ReferenceEquals(tree.Comparer, comparer));
                Assert.IsTrue(ReferenceEquals(tree.Children.Comparer, comparer));
            }
            root.Comparer = oldComparer;
            foreach (var tree in root.AsEnumerable())
            {
                Assert.IsTrue(ReferenceEquals(tree.Comparer, oldComparer));
                Assert.IsTrue(ReferenceEquals(tree.Children.Comparer, oldComparer));
            }
        }
        [TestMethod]
        public void TestChildrenAreInOrder()
        {
            var root = CreateTestTree();
            foreach (var tree in root.AsEnumerable())
            {
                int childCount = tree.Children.Count;
                for (int i = 1; i < childCount; i++)
                {
                    int compare = root.Comparer.Compare(tree.Children[i - 1], tree.Children[i]);
                    Assert.AreNotEqual(compare, 1);
                }
            }
        }
        [TestMethod]
        public void TestRootUpdateOnTreeRemovalAndAdding()
        {
            var root = CreateTestTree();
            var testChild = root.Children[0];
            root.Children.Remove(testChild);
            Assert.IsNull(testChild.Parent);
            foreach (var tree in testChild.AsEnumerable())
            {
                Assert.IsTrue(ReferenceEquals(tree.Root, testChild));
            }
            root.Children.Add(testChild);
            Assert.IsTrue(ReferenceEquals(root, testChild.Parent));
            foreach (var tree in root.AsEnumerable())
            {
                Assert.IsTrue(ReferenceEquals(tree.Root, root));
            }
        }

        [TestMethod]
        public void TestParentAndRootEventsOnTreeAdded()
        {
            var root = CreateTestTree();
            var testChild = root.Children[0];
            root.Children.Remove(testChild);

            bool parentChanged_raised = false;
            testChild.ParentChanged += (sender, oldParent) =>
            {
                parentChanged_raised = true;
                Assert.IsTrue(ReferenceEquals(sender, testChild));
                Assert.IsTrue(ReferenceEquals(oldParent, null));
            };
            bool rootChanged_raised = false;
            testChild.RootChanged += (sender, oldRoot) =>
            {
                rootChanged_raised = true;
                Assert.IsTrue(ReferenceEquals(sender, testChild));
                Assert.IsTrue(ReferenceEquals(oldRoot, testChild));
            };

            root.Children.Add(testChild);
            Assert.IsTrue(parentChanged_raised);
            Assert.IsTrue(rootChanged_raised);
        }
        [TestMethod]
        public void TestParentAndRootEventsOnTreeRemoval()
        {
            var root = CreateTestTree();
            var testChild = root.Children[0];
            bool parentChanged_raised = false;
            testChild.ParentChanged += (sender, oldParent) =>
            {
                parentChanged_raised = true;
                Assert.IsTrue(ReferenceEquals(sender, testChild));
                Assert.IsTrue(ReferenceEquals(oldParent, root));
            };
            bool rootChanged_raised = false;
            testChild.RootChanged += (sender, oldRoot) =>
            {
                rootChanged_raised = true;
                Assert.IsTrue(ReferenceEquals(sender, testChild));
                Assert.IsTrue(ReferenceEquals(oldRoot, root));
            };
            root.Children.Remove(testChild);
            Assert.IsTrue(parentChanged_raised);
            Assert.IsTrue(rootChanged_raised);
        }

        [TestMethod]
        public void TestChildAddedAndRemovedEventsWithAttachAndDetach()
        {
            var root = CreateTestTree();
            var testChild = root.Children[0];

            bool ChildTreeAdded_raised = false;
            root.ChildTreeAdded += (sender, newItem, index) =>
            {
                ChildTreeAdded_raised = true;
                Assert.IsTrue(ReferenceEquals(sender, root));
                Assert.IsTrue(ReferenceEquals(newItem, testChild));
                Assert.IsTrue(index == 0);
            };
            bool ChildTreeRemoved_raised = false;
            root.ChildTreeRemoved += (sender, oldItem, index) =>
            {
                ChildTreeRemoved_raised = true;
                Assert.IsTrue(ReferenceEquals(sender, root));
                Assert.IsTrue(ReferenceEquals(oldItem, testChild));
                Assert.IsTrue(index == 0);
            };
            bool ChildTreeAttached_raised = false;
            root.ChildTreeAttached += (sender, child) =>
            {
                ChildTreeAttached_raised = true;
                Assert.IsTrue(ReferenceEquals(sender, root));
                Assert.IsTrue(ReferenceEquals(child, testChild));
            };
            bool ChildTreeDetached_raised = false;
            root.ChildTreeDetached += (sender, child) =>
            {
                ChildTreeDetached_raised = true;
                Assert.IsTrue(ReferenceEquals(sender, root));
                Assert.IsTrue(ReferenceEquals(child, testChild));
            };
            root.Children.Remove(testChild);
            //Assert.IsTrue(parentChanged_raised);
            //Assert.IsTrue(rootChanged_raised);
            Assert.IsTrue(ChildTreeRemoved_raised);
            Assert.IsTrue(ChildTreeDetached_raised);

            root.Children.Add(testChild);
            Assert.IsTrue(ChildTreeAdded_raised);
            Assert.IsTrue(ChildTreeAttached_raised);
        }
        [TestMethod]
        public void TestChildChangedEventsWithAttachAndDetach()
        {
            var root = CreateTestTree();
            var testChild = root.Children[0];
            var tmpChild = root.Children[1];
            root.Children.RemoveAt(1);

            bool ChildTreeChanged_raised = false;
            root.ChildTreeChanged += (sender, oldItem, newItem, index) =>
            {
                ChildTreeChanged_raised = true;
                Assert.IsTrue(ReferenceEquals(sender, root));
                Assert.IsTrue(ReferenceEquals(oldItem, testChild));
                Assert.IsTrue(ReferenceEquals(newItem, tmpChild));
                Assert.IsTrue(index == 0);
            };
            bool ChildTreeAttached_raised = false;
            root.ChildTreeAttached += (sender, child) =>
            {
                ChildTreeAttached_raised = true;
                Assert.IsTrue(ReferenceEquals(sender, root));
                Assert.IsTrue(ReferenceEquals(child, tmpChild));
            };
            bool ChildTreeDetached_raised = false;
            root.ChildTreeDetached += (sender, child) =>
            {
                ChildTreeDetached_raised = true;
                Assert.IsTrue(ReferenceEquals(sender, root));
                Assert.IsTrue(ReferenceEquals(child, testChild));
            };

            root.Children[0] = tmpChild;
            Assert.IsTrue(ChildTreeChanged_raised);
            Assert.IsTrue(ChildTreeAttached_raised);
            Assert.IsTrue(ChildTreeDetached_raised);
        }
       
        [TestMethod]
        public void TestComparerChangedEvent()
        {
            var root = CreateTestTree();
            var oldComparer = root.Comparer;
            var valueComparer = Comparer<ValueClass>.Create((v1, v2) =>
                v1.GetHashCode().CompareTo(v2.GetHashCode()));
            var comparer = ObservableSortedTree<ValueClass>.TransformValueToTreeComparer(valueComparer);
            bool comparerChanged_raised = false;
            root.ComparerChanged += (sender, oldComp) =>
            {
                comparerChanged_raised = true;
                Assert.IsTrue(ReferenceEquals(root, sender));
                Assert.IsTrue(ReferenceEquals(oldComparer, oldComp));
            };
            root.Comparer = comparer;
            Assert.IsTrue(comparerChanged_raised);
        }
        //root.TreeValueChanged
        //root.PropertyChanged += Root_PropertyChanged;
        [TestMethod]
        public void TestTreeValueChangedEvent()
        {
            var root = CreateTestTree();
            var testChild = root.Children[0];
            var oldValue = testChild.Value;
            var newValue = new ValueClass(new Random(0x5588DD99));
            bool treeValueChangedEvent_raised = false;
            testChild.TreeValueChanged += (sender, old) =>
            {
                treeValueChangedEvent_raised = true;
                Assert.IsTrue(ReferenceEquals(sender, testChild));
                Assert.IsTrue(ReferenceEquals(old, oldValue));
            };
            testChild.Value = newValue;
            Assert.IsTrue(treeValueChangedEvent_raised);
        }
        [TestMethod]
        public void TestTreeOrderingAfterTreeValueUpdate()
        {
           var root = CreateTestTree();
            var comparer= Comparer<ObservableSortedTree<ValueClass>>.Default;
            root.Comparer = comparer;
            
            root.EnumerateDescendantsAndSelf()
                .ForEach(tree => Assert.IsTrue(areItemsInOrder(tree.Children, comparer)));
            int cnt = 0;
            foreach (var tree in root.EnumerateDescendantsAndSelf())
            {
                tree.Value.Number = cnt;
                tree.Value.Text = "Text" + cnt.ToString();
                cnt++;
            }
            root.EnumerateDescendantsAndSelf()
                .ForEach(tree => Assert.IsTrue(areItemsInOrder(tree.Children, comparer)));
            root.EnumerateDescendantsAndSelf().ForEach(tree=>tree.Children.Resort());
            root.EnumerateDescendantsAndSelf()
                .ForEach(tree => Assert.IsTrue(areItemsInOrder(tree.Children, comparer)));
            var testChild = root.Children[0];
            var newValue=new ValueClass("Text"+cnt,cnt);

            testChild.Value = newValue;
            root.EnumerateDescendantsAndSelf()
                .ForEach(tree => Assert.IsTrue(areItemsInOrder(tree.Children, comparer)));
        }
        private bool areItemsInOrder<T>(IList<T> items, IComparer<T> comparer)
        {
            for (int i = 1; i < items.Count; i++)
            {
                int comp = comparer.Compare(items[i - 1], items[i]);
                if (comp > 0)
                    return false;
            }
            return true;
        }
        [TestMethod]
        public void TestChildOrderAfterComparerUpdate()
        {
            var root = CreateTestTree();
            var oldComparer = root.Comparer;
            var reverseComparer = Comparer<ObservableSortedTree<ValueClass>>.Create(
                (tree1, tree2) => oldComparer.Compare(tree2, tree1));
            Assert.IsTrue(oldComparer.Compare(root.Children[0], root.Children[1])
                          == -1 * reverseComparer.Compare(root.Children[0], root.Children[1]),
                          "reverse comparer did not give reverse result (will give error if test comparison is equivelant)");
            root.Comparer = reverseComparer;
            foreach (var tree in root.AsEnumerable())
            {
                Assert.IsTrue(ReferenceEquals(tree.Comparer, reverseComparer), "comparer did not update to new value");
                for (int i = 1; i < tree.Children.Count; i++)
                {
                    Assert.IsFalse(reverseComparer.Compare(tree.Children[i - 1], tree.Children[i]) == 1, $"Items are not in order at elements {i - 1} & {i}");
                }
            }
        }
        

        
    }
}
