using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Onwo;
using Onwo.Collections;

namespace OnwoTests.Collections
{
    [TestClass()]
    public class ComparerExtensionsTests
    {
        [TestMethod()]
        public void InvertTest()
        {
            var comp = Comparer<int>.Default;
            var invertedComp = comp.Invert();
            int seed = 0;
            Enumerable.Range(-10, 20)
                .ForEach(i => Assert.IsTrue(comp.Compare(i, seed) == -1*invertedComp.Compare(i, seed)));
        }
    }
}