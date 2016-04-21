using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Onwo.Collections;

namespace OnwoTests.Collections
{
    [TestClass]
    public class ObservableSortedTreeTests_WithNewChildListInstance : ObservableSortedTreeTests
    {
        public override ObservableSortedTree<ValueClass> CreateTestTree()
        {
            var root = base.CreateTestTree();
            var array = root.Children.ToArray();
            root.Children.Clear();
            var obs = new ObservableSortedCollection<ObservableSortedTree<ValueClass>>(root.Comparer);
            foreach (var child in array)
                root.Children.Add(child);
            return root;
        }
    }
}
