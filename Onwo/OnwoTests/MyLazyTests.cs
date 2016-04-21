using Microsoft.VisualStudio.TestTools.UnitTesting;
using Onwo;

namespace OnwoTests
{
    [TestClass()]
    public class MyLazyTests
    {
        [TestMethod()]
        public void MyLazyTest()
        {
            //tests HasValue, ValueCreated and ValueDestroyed
            int expectedLazyVal = 1;
            var lazy=new MyLazy<int>(()=> { return 1; });
            bool createdEvent = true;
            bool destroyedEvent = true;
            lazy.ValueCreated += sender =>
            {
                createdEvent = true;
                Assert.IsTrue(ReferenceEquals(sender, lazy));
            };
            lazy.ValueDestroyed += (sender, old) =>
            {
                destroyedEvent = true;
                Assert.IsTrue(ReferenceEquals(sender,lazy));
                Assert.IsTrue(old==expectedLazyVal);
            };
            Assert.IsFalse(lazy.HasValue);
            Assert.IsTrue(lazy.Value== expectedLazyVal);
            Assert.IsTrue(createdEvent);
            createdEvent = false;
            Assert.IsTrue(lazy.HasValue);
            int newVal = 2;
            lazy.ValueFactory = () => { return newVal; };
            Assert.IsTrue(destroyedEvent);
            Assert.IsTrue(lazy.Value== newVal);
            Assert.IsTrue(createdEvent);
        }
    }
}