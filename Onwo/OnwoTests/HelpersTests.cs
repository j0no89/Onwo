using Microsoft.VisualStudio.TestTools.UnitTesting;
using Onwo;

namespace OnwoTests
{
    [TestClass()]
    public class HelpersTests
    {
        [TestMethod]
        public void SwapTest()
        {
            var val1=new ValueClass("First",0);
            var val2 = new ValueClass("Second",1);

            var swap1 = val1;
            var swap2 = val2;
            Helpers.Swap(ref swap1,ref swap2);

            Assert.IsTrue(swap1.Number == val2.Number);
            Assert.IsTrue(swap2.Number == val1.Number);
            Assert.IsTrue(swap1.Text.Equals(val2.Text));
            Assert.IsTrue(swap2.Text.Equals(val1.Text));

            int i0 = 0;
            int i1 = 1;
            Helpers.Swap(ref i0,ref i1);
            Assert.IsTrue(i0 == 1);
            Assert.IsTrue(i1 == 0);
        }
    }
}