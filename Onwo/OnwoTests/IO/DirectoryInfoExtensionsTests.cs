using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Onwo.IO;

namespace OnwoTests.IO
{
    [TestClass()]
    public class DirectoryInfoExtensionsTests
    {
        [TestMethod()]
        public void CompareDirectoriesTest()
        {
            var rootDir=new DirectoryInfo("C:\\");
            var subDir=new DirectoryInfo("C:\\Windows");
            var otherDir=new DirectoryInfo("C:\\Program Files");
            Assert.IsTrue(rootDir.Exists);
            Assert.IsTrue(subDir.Exists);
            Assert.IsTrue(otherDir.Exists);

            var result = rootDir.CompareDirectories(subDir);
            Assert.IsTrue(result.HasFlag(DirectoryCompareResult.ParentDirectory));

            result = subDir.CompareDirectories(rootDir);
            Assert.IsTrue(result.HasFlag(DirectoryCompareResult.SubDirectory));

            result = subDir.CompareDirectories(otherDir);
            Assert.IsTrue(result.HasFlag(DirectoryCompareResult.Unrelated));

            result = rootDir.CompareDirectories(new DirectoryInfo(rootDir.FullName));
            Assert.IsTrue(result.HasFlag(DirectoryCompareResult.Equal));
        }
    }
}