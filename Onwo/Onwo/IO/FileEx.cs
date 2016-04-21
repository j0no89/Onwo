using System;
using System.IO;

namespace Onwo.IO
{
    public static class FileEx
    {
        public static bool TryDelete(string path)
        {
            try
            {
                File.Delete(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
