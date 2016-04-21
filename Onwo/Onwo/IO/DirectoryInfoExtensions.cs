using System;
using System.IO;

namespace Onwo.IO
{
    public static class DirectoryInfoExtensions
    {
       
        private static readonly char[] directorySplitChars = new[] { '\\' };

        public static string[] SplitDirectoryPath(this DirectoryInfo di)
        {
            return di.FullName.Split(directorySplitChars, StringSplitOptions.RemoveEmptyEntries);
        }

        public static DirectoryCompareResult CompareDirectories(this DirectoryInfo sourceDirectory,
            DirectoryInfo compareDirectory)
        {
            string path1 = sourceDirectory.FullName.TrimEnd('\\');
            string path2 = compareDirectory?.FullName.TrimEnd('\\') ?? "";
            int minLen = path1.Length;
            bool short1 = true;
            if (path1.Length > path2.Length)
            {
                minLen = path2.Length;
                short1 = false;
            }
            for (int i = minLen - 1; i >= 0; i--)
            {
                if (!char.ToUpperInvariant(path1[i]).Equals(char.ToUpperInvariant(path2[i])))
                    return DirectoryCompareResult.Unrelated;
            }
            if (path1.Length == path2.Length)
                return DirectoryCompareResult.Equal;
            if (short1)
                return DirectoryCompareResult.ParentDirectory;
            else
                return DirectoryCompareResult.SubDirectory;
        }

        public static DirectoryCompareResult CompareDirectories2(this DirectoryInfo sourceDirectory,
            DirectoryInfo compareDirectory)
        {
            var split1 = SplitDirectoryPath(sourceDirectory);
            var split2 = SplitDirectoryPath(compareDirectory);
            int minLen = split1.Length;
            bool short1 = true;
            if (split1.Length > split2.Length)
            {
                minLen = split2.Length;
                short1 = false;
            }
            int matchingPaths = 0;
            for (int i = minLen - 1; i >= 0; i--)
            {
                if (!string.Equals(split1[i], split2[i], StringComparison.InvariantCultureIgnoreCase))
                    return DirectoryCompareResult.Unrelated;
            }
            if (split1.Length == split2.Length)
                return DirectoryCompareResult.Equal;
            if (short1)
                return DirectoryCompareResult.ParentDirectory;
            else
                return DirectoryCompareResult.SubDirectory;
        }
        public static bool IsSubDirectoryOf(this DirectoryInfo di, DirectoryInfo parent)
        {
            var compare = di.CompareDirectories(parent);
            return compare.HasFlag(DirectoryCompareResult.SubDirectory);
        }

    }
}
