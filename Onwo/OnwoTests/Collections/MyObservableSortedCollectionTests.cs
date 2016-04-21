using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Onwo;
using Onwo.Collections;

namespace OnwoTests.Collections
{
    [TestClass]
    public class MyObservableSortedCollectionTests
    {
        private bool areIntegersInOrder(IList<int> items)
        {
            for (int i = 1; i < items.Count; i++)
            {
                if (items[i - 1] > items[i])
                    return false;
            }
            return true;
        }
        private bool areIntegersInReverseOrder(IList<int> items)
        {
            for (int i = 1; i < items.Count; i++)
            {
                if (items[i - 1] < items[i])
                    return false;
            }
            return true;
        }
        [TestMethod]
        public void MyObservableSortedCollectionTest_int()
        {
            var collection=new ObservableSortedCollection<int>();
            var array = Enumerable.Range(0, 5).Select(val => val*10)
                .SelectMany(val => Enumerable.Range(val, 10).Reverse())
                .ToArray();
            collection.AddRange(array);
            Assert.IsTrue(areIntegersInOrder(collection));
            Assert.IsTrue(array.Length==collection.Count);
            array.ForEach(val=>Assert.IsTrue(collection.Contains(val)));
            collection[5] = 99;
            Assert.IsTrue(areIntegersInOrder(collection));

            collection.Insert(5,100);
            Assert.IsTrue(areIntegersInOrder(collection));
            Assert.IsTrue(collection.Count == array.Length + 1);

            collection.RemoveAt(5);
            Assert.IsTrue(collection.Count == array.Length);
            collection.Remove(10);
            Assert.IsTrue(collection.Count == array.Length - 1);
            Assert.IsTrue(areIntegersInOrder(collection));

            bool exceptionThrown = false;
            try
            {
                collection.Move(0,1);
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }
            Assert.IsTrue(exceptionThrown);

            int oldCount = collection.Count;
            var oldItems = collection.ToArray();
            var reverseComparer = Comparer<int>.Create((i1, i2) => i2.CompareTo(i1));
            collection.Comparer = reverseComparer;
            Assert.AreEqual(oldCount, collection.Count);
            oldItems.ForEach(item=>Assert.IsTrue(collection.Contains(item)));
            Assert.IsTrue(areIntegersInReverseOrder(collection));
        }
        [TestMethod]
        public void MyObservableSortedCollectionTest_ref()
        {
            var defaultComparer = Comparer<ValueClass>.Default;
            var collection = new ObservableSortedCollection<ValueClass>();
            var array = Enumerable.Range(0, 5).Select(val => val * 10)
                .SelectMany(val => Enumerable.Range(val, 10).Reverse())
                .Select(val=>new ValueClass("Text"+val,val))
                .ToArray();
            collection.AddRange(array);
            Assert.IsTrue(areItemsInOrder(collection, defaultComparer));
            Assert.IsTrue(array.Length == collection.Count);
            array.ForEach(val => Assert.IsTrue(collection.Contains(val)));
            collection[5] = new ValueClass(99);
            Assert.IsTrue(areItemsInOrder(collection, defaultComparer));

            collection.Insert(5, new ValueClass(100));
            Assert.IsTrue(areItemsInOrder(collection, defaultComparer));
            Assert.IsTrue(collection.Count == array.Length + 1);

            collection.RemoveAt(5);
            Assert.IsTrue(collection.Count == array.Length);
            collection.Remove(collection.First(item=>item.Number>=10));
            Assert.IsTrue(collection.Count == array.Length - 1);
            Assert.IsTrue(areItemsInOrder(collection,defaultComparer));

            bool exceptionThrown = false;
            try
            {
                collection.Move(0, 1);
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }
            Assert.IsTrue(exceptionThrown);

            int oldCount = collection.Count;
            var oldItems = collection.ToArray();
            var reverseComparer = Comparer<ValueClass>.Create((i1, i2) => i2.CompareTo(i1));
            collection.Comparer = reverseComparer;
            Assert.AreEqual(oldCount, collection.Count);
            oldItems.ForEach(item => Assert.IsTrue(collection.Contains(item)));
            Assert.IsTrue(areItemsInOrder(collection, reverseComparer));
        }

        [TestMethod]
        public void ResortItemTest()
        {
            var comparer = Comparer<ValueClass>.Create((v1, v2) =>
            {
                int comp = v1.Number.CompareTo(v2.Number);
                if (comp == 0)
                    comp = String.Compare(v1.Text, v2.Text, StringComparison.InvariantCulture);
                return comp;
            });
            var collection=new ObservableSortedCollection<ValueClass>(comparer);
            var array = Enumerable.Range(0, 5).Select(val => val * 10)
               .SelectMany(val => Enumerable.Range(val, 10).Reverse())
               .Select(val=>new ValueClass($"Text{val}",val))
               .ToArray();
            collection.AddRange(array);

            Assert.IsTrue(areItemsInOrder(collection, comparer));
            collection[5].Number = -50;
            Assert.IsFalse(areItemsInOrder(collection,comparer));
            collection.ResortItem(5);
            Assert.IsTrue(areItemsInOrder(collection,comparer));
        }
        [TestMethod]
        public void ComparerChangedTest()
        {
            var collection=new ObservableSortedCollection<int>();
            bool comparerChangedEvent = false;
            collection.ComparerChanged += (sender, old) =>
            {
                Assert.IsTrue(ReferenceEquals(sender,collection));
                Assert.IsNull(old);
                comparerChangedEvent = true;
            };
            collection.Comparer = Comparer<int>.Create((i1, i2) => i2.CompareTo(i1));
            Assert.IsTrue(comparerChangedEvent);
            
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
    }
}