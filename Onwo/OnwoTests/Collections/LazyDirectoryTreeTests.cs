using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Onwo;
using Onwo.Collections.Special;

namespace OnwoTests.Collections
{
    [TestClass]
    public class LazyDirectoryTreeTests
    {
        public static LazyDirectoryTree CreateTestTree()
        {
            return new LazyDirectoryTree(@"C:\");
        }
        //when all folders are selected and then 1 is deselected
        //when no folders are selected and then 1 selected
        //when all but one are selected and it is selected
        //when only 1 is selected and it is deselected

        //when all folders are selected except 1 is partially selected, and then it is selected

        //IsSelected states to test:
        //True => False
        //  If atleast 1 sibling tree is still selected the parent trees shall be partially selected
        //  if no sibling trees are selected but atleast 1 sibling is partially selected, parent tree shall be partially selected
        //  If no sibling trees are selected, the parent tree is not selected
        //False => True
        //  If atleast 1 sibling tree is still selected the parent trees shall be partially selected
        //  if no sibling trees are selected but atleast 1 sibling is partially selected, parent tree shall be partially selected
        //  If no sibling trees are selected, the parent tree is not selected
        //when a subtree is changed selcted => deselected
        //  when atleast 1 sibling tree is stil selected
        //  when no other siblings are selected
        [TestMethod]
        public void IsSelected1()
        {
            var root = new LazyDirectoryTree(@"C:\");
            var downloadsTree = root.Children.Cast<LazyDirectoryTree>()
                .FirstOrDefault(child =>
                    string.Equals("Downloads", child?.Value?.Name, StringComparison.InvariantCultureIgnoreCase));
            Assert.IsFalse(downloadsTree == null);
            var completedTree = downloadsTree.Children.Cast<LazyDirectoryTree>()
                .FirstOrDefault(child =>
                    string.Equals("Completed", child?.Value?.Name, StringComparison.InvariantCultureIgnoreCase));
            Assert.IsFalse(completedTree == null);
            var completedSibling1 = downloadsTree.Children.Cast<LazyDirectoryTree>()
                .FirstOrDefault(child =>
                    string.Equals("tmp", child?.Value?.Name, StringComparison.InvariantCultureIgnoreCase));
            Assert.IsFalse(completedSibling1 == null);
            var completedSibling2 = downloadsTree.Children.Cast<LazyDirectoryTree>()
                .FirstOrDefault(child =>
                    string.Equals("Torrents", child?.Value?.Name, StringComparison.InvariantCultureIgnoreCase));
            Assert.IsFalse(completedSibling2 == null);

            var allDownloadsDescendants = downloadsTree.EnumerateDescendantsAndSelf(false).ToArray();

            completedTree.IsSelected = false;
            root.Children.Where(child => !ReferenceEquals(child, downloadsTree))
                .SelectMany(child => child.EnumerateDescendantsAndSelf())
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == true));
            downloadsTree.Children.Where(child => !ReferenceEquals(child, completedTree))
                .SelectMany(child => child.EnumerateDescendantsAndSelf())
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == true));
            completedTree.EnumerateDescendantsAndSelf(true)
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == false));
            completedTree.EnumerateAncestors()
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == null));

            completedTree.IsSelected = true;
            root.EnumerateDescendantsAndSelf(true)
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == true));

            completedTree.IsSelected = false;
            root.Children.Where(child => !ReferenceEquals(child, downloadsTree))
                .SelectMany(child => child.EnumerateDescendantsAndSelf())
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == true));
            downloadsTree.Children.Where(child => !ReferenceEquals(child, completedTree))
                .SelectMany(child => child.EnumerateDescendantsAndSelf())
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == true));
            completedTree.EnumerateDescendantsAndSelf(true)
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == false));
            completedTree.EnumerateAncestors()
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == null));

            completedSibling1.IsSelected = false;
            root.Children.Where(child => !ReferenceEquals(child, downloadsTree))
                .SelectMany(child => child.EnumerateDescendantsAndSelf())
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == true));
            downloadsTree.Children.Where(child => !ReferenceEquals(child, completedTree) && !ReferenceEquals(child, completedSibling1))
                .SelectMany(child => child.EnumerateDescendantsAndSelf())
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == true));
            completedTree.EnumerateDescendantsAndSelf(true)
                .Concat(completedSibling1.EnumerateDescendantsAndSelf(true))
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == false));
            completedTree.EnumerateAncestors()
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == null));

            completedSibling2.IsSelected = false;
            root.Children.Where(child => !ReferenceEquals(child, downloadsTree))
                .SelectMany(child => child.EnumerateDescendantsAndSelf())
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == true));
            downloadsTree.EnumerateDescendantsAndSelf(true)
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == false));
            Assert.IsTrue(root.IsSelected == null);

            completedSibling1.IsSelected = true;
            root.Children.Where(child => !ReferenceEquals(child, downloadsTree))
                .SelectMany(child => child.EnumerateDescendantsAndSelf())
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == true));
            downloadsTree.Children.Where(child => !ReferenceEquals(child, completedSibling1))
                .SelectMany(child => child.EnumerateDescendantsAndSelf())
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == false));
            completedSibling1.EnumerateDescendantsAndSelf(true)
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == true));
            completedSibling1.EnumerateAncestors()
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == null));

            completedSibling2.IsSelected = true;
            root.Children.Where(child => !ReferenceEquals(child, downloadsTree))
                .SelectMany(child => child.EnumerateDescendantsAndSelf())
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == true));
            downloadsTree.Children.Where(child => !ReferenceEquals(child, completedSibling1) && !ReferenceEquals(child, completedSibling2))
                .SelectMany(child => child.EnumerateDescendantsAndSelf())
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == false));
            completedSibling1.EnumerateDescendantsAndSelf(true)
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == true));
            completedSibling2.EnumerateDescendantsAndSelf(true)
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == true));
            completedSibling2.EnumerateAncestors()
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == null));

            completedTree.IsSelected = true;
            root.EnumerateDescendantsAndSelf(true)
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == true));

            ////////////////////////////////////////////////////////////////////////////////

            root.Children.Where(child => !ReferenceEquals(child, downloadsTree))
                .SelectMany(child => child.EnumerateDescendantsAndSelf())
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => tree.IsSelected = false);
            root.Children.Where(child => !ReferenceEquals(child, downloadsTree))
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == false));
            downloadsTree.EnumerateDescendantsAndSelf(true)
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == true));
            Assert.IsTrue(root.IsSelected == null);

            ////////////////////////////////////////////////////////////////////////////////

            completedTree.IsSelected = false;
            root.Children.Where(child => !ReferenceEquals(child, downloadsTree))
                .SelectMany(child => child.EnumerateDescendantsAndSelf())
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == false));
            downloadsTree.Children.Where(child => !ReferenceEquals(child, completedTree))
                .SelectMany(child => child.EnumerateDescendantsAndSelf())
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == true));
            completedTree.EnumerateDescendantsAndSelf(true)
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == false));
            completedTree.EnumerateAncestors()
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == null));

            completedTree.IsSelected = true;
            root.Children.Where(child => !ReferenceEquals(child, downloadsTree))
                .SelectMany(child => child.EnumerateDescendantsAndSelf())
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => tree.IsSelected = false);
            root.Children.Where(child => !ReferenceEquals(child, downloadsTree))
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == false));
            downloadsTree.EnumerateDescendantsAndSelf(true)
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == true));
            Assert.IsTrue(root.IsSelected == null);

            completedTree.IsSelected = false;
            root.Children.Where(child => !ReferenceEquals(child, downloadsTree))
                .SelectMany(child => child.EnumerateDescendantsAndSelf())
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == false));
            downloadsTree.Children.Where(child => !ReferenceEquals(child, completedTree))
                .SelectMany(child => child.EnumerateDescendantsAndSelf())
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == true));
            completedTree.EnumerateDescendantsAndSelf(true)
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == false));
            completedTree.EnumerateAncestors()
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == null));

            completedSibling1.IsSelected = false;
            root.Children.Where(child => !ReferenceEquals(child, downloadsTree))
                .SelectMany(child => child.EnumerateDescendantsAndSelf())
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == false));
            downloadsTree.Children.Where(child => !ReferenceEquals(child, completedTree) && !ReferenceEquals(child, completedSibling1))
                .SelectMany(child => child.EnumerateDescendantsAndSelf())
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == true));
            completedTree.EnumerateDescendantsAndSelf(true)
                .Concat(completedSibling1.EnumerateDescendantsAndSelf(true))
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == false));
            completedSibling1.EnumerateDescendantsAndSelf(true)
                .Concat(completedSibling1.EnumerateDescendantsAndSelf(true))
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == false));
            completedTree.EnumerateAncestors()
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == null));

            completedSibling2.IsSelected = false;
            root.EnumerateDescendantsAndSelf(true)
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == false));

            completedSibling1.IsSelected = true;
            root.Children.Where(child => !ReferenceEquals(child, downloadsTree))
                .SelectMany(child => child.EnumerateDescendantsAndSelf())
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == false));
            downloadsTree.Children.Where(child => !ReferenceEquals(child, completedSibling1))
                .SelectMany(child => child.EnumerateDescendantsAndSelf())
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == false));
            completedSibling1.EnumerateDescendantsAndSelf(true)
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == true));
            completedSibling1.EnumerateAncestors()
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == null));

            completedSibling2.IsSelected = true;
            root.Children.Where(child => !ReferenceEquals(child, downloadsTree))
                .SelectMany(child => child.EnumerateDescendantsAndSelf())
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == false));
            downloadsTree.Children.Where(child => !ReferenceEquals(child, completedSibling1) && !ReferenceEquals(child, completedSibling2))
                .SelectMany(child => child.EnumerateDescendantsAndSelf())
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == false));
            completedSibling1.EnumerateDescendantsAndSelf(true)
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == true));
            completedSibling2.EnumerateDescendantsAndSelf(true)
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == true));
            completedSibling2.EnumerateAncestors()
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == null));

            completedTree.IsSelected = true;
            root.Children.Where(child => !ReferenceEquals(child, downloadsTree))
                .SelectMany(child => child.EnumerateDescendantsAndSelf())
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => tree.IsSelected = false);
            root.Children.Where(child => !ReferenceEquals(child, downloadsTree))
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == false));
            downloadsTree.EnumerateDescendantsAndSelf(true)
                .Cast<LazyDirectoryTree>()
                .ForEach(tree => Assert.IsTrue(tree.IsSelected == true));
            Assert.IsTrue(root.IsSelected == null);
        }
    }
}
