using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tests
{
    class Helpers
    {
        public static DirectoryInfo GetTempFolder()
        {
            DirectoryInfo di = new DirectoryInfo(Path.Combine(new DirectoryInfo(Directory.GetCurrentDirectory()).FullName, "Temp"));

            if (!di.Exists) di.Create();

            return di;
        }

        public static void ClearTempFolder()
        {
            DirectoryInfo di = GetTempFolder();

            di.Delete(true);
        }

        public static FileInfo CreateTempFile(string ext, string fileName = null)
        {
            if (string.IsNullOrWhiteSpace(fileName)) fileName = Guid.NewGuid().ToString();
            ext = ext.TrimStart('.');

            string filename = $"{fileName}.{ext}";

            return new FileInfo(Path.Combine(GetTempFolder().FullName, filename));
        }

        public static DirectoryInfo GetMongoToolesFolder()
        {
            return new DirectoryInfo(Path.Combine(new DirectoryInfo(Directory.GetCurrentDirectory()).FullName, "mongodb_db_tools", "bin"));
        }
    }
}
