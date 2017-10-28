using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onwo.IO
{
    public static class FileHelpers
    {
        public static byte[] getHash(string path)
        {
            if (!File.Exists(path))
                return null;
            using (var stream = File.OpenRead(path))
            using (var reader = new BinaryReader(stream))
            {
                
            }
            return null;
        }
    }
}
