using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LapTrinhMang.Utils
{
    public class FileHelper
    {
        public static byte[] ReadFile(string path) => File.ReadAllBytes(path);
        public static void SaveFile(string path, byte[] data) => File.WriteAllBytes(path, data);
    }
}
