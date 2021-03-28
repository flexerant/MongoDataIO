using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tests
{
    public class TempEnvironment : IDisposable
    {
        public TempEnvironment(DirectoryInfo mongodumpExeFolder)
        {
            Environment.SetEnvironmentVariable("MOGODB_EXE_DIRECTORY", mongodumpExeFolder.FullName);
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable("MOGODB_EXE_DIRECTORY", null);
        }
    }
}
