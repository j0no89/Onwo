using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Onwo;

namespace OnwoTests
{
    [TestClass()]
    public class EnumerableExtensionsTests
    {
        [TestMethod()]
        public void ForEachTest()
        {
            var items=Enumerable.Repeat(1, 10)
                .Select(i => new ValueClass(i))
                .ToArray();
            items.ForEach(item => item.Number = item.Number + 1);
            foreach (var item in items)
            {
                Assert.IsTrue(item.Number == 2);
            }
            items.ForEach((item, index)=>item.Number=index);
            for (int i = 0; i < items.Length; i++)
            {
                Assert.AreEqual(items[i].Number,i);
            }
        }
        [TestMethod()]
        public void EnumerateFromTest()
        {
            var items = Enumerable.Range(0, 10)
                .Select(i => new ValueClass(i))
                .ToArray();
            var items2 = items.EnumerateFrom(5)
                .ToArray();
            for (int i = 0; i < items2.Length; i++)
            {
                Assert.AreEqual(items2[i].Number,i+5);
            }
        }
        [TestMethod()]
        public void IndexOfFirstTest()
        {
            var items = Enumerable.Range(0, 10)
                .Select(i => new ValueClass(i))
                .ToArray();
            int index = items.IndexOfFirst(item => item.Number >= 5);
            Assert.AreEqual(index,5);
        }
    }
}