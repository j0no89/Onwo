using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Onwo;
using Onwo.Collections;

namespace OnwoTests.Collections
{
    [TestClass]
    public class MyObservableCollectionTests
    {
        [TestMethod]
        public void MyObservableCollectionTest()
        {
            char xChar = '-';
            char xChar2 = '-';
            int xIndex = -1;
            var collection = new MyObservableCollection<char>();
            bool attachedEvent = false;
            collection.ItemAttached += (sender, newItem, index) =>
            {
                Assert.IsTrue(ReferenceEquals(sender, collection));
                Assert.AreEqual(xChar, newItem);
                Assert.AreEqual(xIndex, index);
                attachedEvent = true;
            };
            bool detachedEvent = false;
            collection.ItemDetached += (sender, oldItem, index) =>
            {
                Assert.IsTrue(ReferenceEquals(sender, collection));
                Assert.AreEqual(xChar2, oldItem);
                Assert.AreEqual(xIndex, index);
                detachedEvent = true;
            };
            bool addedEvent = false;
            collection.ItemAdded += (sender, newItem, index) =>
            {
                Assert.IsTrue(ReferenceEquals(sender, collection));
                Assert.AreEqual(xChar, newItem);
                Assert.AreEqual(xIndex, index);
                addedEvent = true;
            };
            bool removedEvent = false;
            collection.ItemRemoved += (sender, oldItem, index) =>
            {
                Assert.IsTrue(ReferenceEquals(sender, collection));
                Assert.AreEqual(xChar2, oldItem);
                Assert.AreEqual(xIndex, index);
                removedEvent = true;
            };
            bool movedEvent = false;
            collection.ItemMoved += (sender, oldIndex, newIndex) =>
            {
                Assert.IsTrue(ReferenceEquals(sender, collection));
                Assert.AreEqual(xChar, oldIndex);
                Assert.AreEqual(xIndex, newIndex);
                movedEvent = true;
            };
            bool changedEvent = false;
            collection.ItemChanged += (sender, oldItem, newItem, index) =>
            {
                Assert.IsTrue(ReferenceEquals(sender, collection));
                Assert.AreEqual(xChar2, oldItem);
                Assert.AreEqual(xChar, newItem);
                Assert.AreEqual(xIndex, index);
                changedEvent = true;
            };

            xChar = 'a';
            xChar2 = xChar;
            xIndex = 0;
            collection.Add('a');
            Assert.IsTrue(addedEvent);
            Assert.IsTrue(attachedEvent);
            xChar = 'b';
            xChar2 = xChar;
            xIndex = 1;
            collection.Add('b');

            collection.Remove('b');
            Assert.IsTrue(removedEvent);
            Assert.IsTrue(detachedEvent);
            removedEvent = false;
            detachedEvent = false;
            collection.Add('b');
            addedEvent = false;
            attachedEvent = false;
            xChar = 'c';
            collection[1] = 'c';
            Assert.IsTrue(changedEvent);
            Assert.IsTrue(detachedEvent);
            Assert.IsTrue(attachedEvent);
            changedEvent = false;
            detachedEvent = false;
            attachedEvent = false;

            int removedCount = 0;

        }
        [TestMethod]
        public void ClearTest()
        {
            var collection=new MyObservableCollection<int>();
            var newItems = Enumerable.Range(0, 5).ToArray();
            collection.AddRange(newItems);
            int detachedCount = 0;
            collection.ItemDetached += (sender, oldItem, index) =>
            {
                Assert.IsTrue(ReferenceEquals(sender, collection));
                detachedCount++;
            };
            int removeCount = 0;
            collection.ItemRemoved += (sender, oldItem, index) =>
            {
                Assert.IsTrue(ReferenceEquals(sender, collection));
                removeCount++;
            };

            collection.Clear();
            Assert.AreEqual(detachedCount, newItems.Length);
            Assert.AreEqual(removeCount, newItems.Length);
        }
        [TestMethod()]
        public void AddRangeTest()
        {
            var collection=new MyObservableCollection<int>();
            int attachedCount = 0;
            collection.ItemAttached += (sender, newItem, index) =>
            {
                Assert.IsTrue(ReferenceEquals(sender, collection));
                attachedCount++;
            };
            int addedCount = 0;
            collection.ItemAdded += (sender, newItem, index) =>
            {
                Assert.IsTrue(ReferenceEquals(sender, collection));
                addedCount++;
            };

            var newItems = Enumerable.Range(0, 5).ToArray();
            collection.AddRange(newItems);
            Assert.AreEqual(attachedCount,newItems.Length);
            Assert.AreEqual(addedCount, newItems.Length);
            Assert.AreEqual(newItems.Length,collection.Count);
            foreach (var item in newItems)
            {
                Assert.IsTrue(collection.Contains(item));
            }
            bool exceptionThrow = false;
            try
            {
                collection.AddRange(null);
            }
            catch (Exception)
            {
                exceptionThrow = true;
            }
            Assert.IsFalse(exceptionThrow);
        }
        [TestMethod()]
        public void AddRangeTest2()
        {
            var collection = new MyObservableCollection<int>();
            Enumerable.Range(10,15).ForEach(val=>collection.Add(val));
            int initCount = collection.Count;
            int attachedCount = 0;
            collection.ItemAttached += (sender, newItem, index) =>
            {
                Assert.IsTrue(ReferenceEquals(sender, collection));
                attachedCount++;
            };
            int addedCount = 0;
            collection.ItemAdded += (sender, newItem, index) =>
            {
                Assert.IsTrue(ReferenceEquals(sender, collection));
                addedCount++;
            };

            var newItems = Enumerable.Range(0, 5).ToArray();
            collection.AddRange(newItems);
            Assert.AreEqual(attachedCount, newItems.Length);
            Assert.AreEqual(addedCount, newItems.Length);
            Assert.AreEqual(newItems.Length, collection.Count - initCount);
            foreach (var item in newItems)
            {
                Assert.IsTrue(collection.Contains(item));
            }
        }
        [TestMethod]
        public void RemoveWhereTest()
        {
            var collection=new MyObservableCollection<int>();
            collection.AddRange(Enumerable.Range(0,31));
            collection.RemoveWhere(val =>
            {
                int remainder;
                int div = Math.DivRem(val, 3, out remainder);
                return remainder == 0;
            });
            collection.ForEach(val =>
            {
                int remainder;
                int div = Math.DivRem(val, 3, out remainder);
                Assert.IsFalse(remainder == 0);
            });
        }
        [TestMethod]
        public void RemoveWhereTest2()
        {
            removeWhere_numElements(5);
        }
        [TestMethod]
        public void RemoveWhereTest3()
        {
            removeWhere_numElements(1);
        }
        public void removeWhere_numElements(int numElementsToRemove)
        {
            var collection = new MyObservableCollection<int>();
            List<int> removedItems=new List<int>();
            collection.ItemDetached += (sender, oldItem, index) =>
            {
                removedItems.Add(oldItem);
            };
             int maxCount = 30;
            collection.AddRange(Enumerable.Range(0, maxCount));
            int initCount = collection.Count;
            int multiple = 3;
            collection.RemoveWhere(val =>
            {
                int remainder;
                int div = Math.DivRem(val, multiple, out remainder);
                return remainder == 0;
            }, numElementsToRemove);
            Assert.IsTrue(collection.Count == initCount - numElementsToRemove);
            var xRemovedItems = Enumerable.Range(0, numElementsToRemove)
                .Select(val => val*multiple)
                .ToArray();
            Assert.AreEqual(removedItems.Count, xRemovedItems.Length);
            Assert.IsTrue(!removedItems.Except(xRemovedItems).Any());
            Assert.IsTrue(!xRemovedItems.Except(removedItems).Any());
            Enumerable.Range(0, maxCount)
                .Except(xRemovedItems)
                .ForEach(val => Assert.IsTrue(collection.Contains(val)));
            xRemovedItems.ForEach(val => Assert.IsFalse(collection.Contains(val)));
        }
    }
}