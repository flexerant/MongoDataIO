using CliWrap;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Flexerant.MongoDataIO.Core
{
    public class FileTarget
    {
        private readonly FileInfo _mongodumpExe;
        private readonly FileInfo _mongorestoreExe;

        public FileTarget(string mongoBinFolder)
        {
            _mongodumpExe = new FileInfo(Path.Combine(mongoBinFolder, "mongodump.exe"));
            _mongorestoreExe = new FileInfo(Path.Combine(mongoBinFolder, "mongorestore.exe"));
        }

        public FileTarget(DirectoryInfo directory) : this(directory.FullName) { }

        public FileInfo DumpToFile(string mongoConnectionString, string databaseName, string outputFolder, Action<string> outputData = null)
        {
            StreamTarget streamTarget = new StreamTarget(_mongodumpExe.Directory);
            DirectoryInfo diOutput = new DirectoryInfo(outputFolder);
            DateTime now = DateTime.UtcNow;
            FileInfo fiOutput = new FileInfo(Path.Combine(diOutput.FullName, $"{now.ToString("s").Replace(':', '-')}_{databaseName}.bak"));

            using (FileStream fs = new FileStream(fiOutput.FullName, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                streamTarget.DumpToStream(fs, mongoConnectionString, databaseName, outputData);
            }

            return fiOutput;
        }

        public FileInfo DumpToFile(string mongoConnectionString, string databaseName, Action<string> outputData = null)
        {
            FileInfo fiOutput = new FileInfo(Path.GetTempFileName());
            string uri = Helpers.FormatUri(mongoConnectionString);
            CommandResult result;
            StringBuilder sb = new StringBuilder();

            var cmd = Cli.Wrap(_mongodumpExe.FullName)
                .WithArguments($"--uri {uri}/{databaseName} --archive")
                .WithStandardErrorPipe(PipeTarget.ToDelegate(text =>
                {
                    sb.AppendLine(text);
                    outputData?.Invoke(text);
                }, Encoding.ASCII))
                .WithStandardOutputPipe(PipeTarget.ToFile(fiOutput.FullName))
                .WithValidation(CommandResultValidation.None);

            result = Task.Run(async () => await cmd.ExecuteAsync()).Result;

            if (result.ExitCode != 0)
            {
                throw new Exception(sb.ToString());
            }

            return fiOutput;
        }

        //public FileInfo DumpToFile(string mongoConnectionString, string databaseName, Action<string> outputData = null)
        //{
        //    StreamTarget streamTarget = new StreamTarget(_mongodumpExe.Directory);
        //    //DirectoryInfo diOutput = new DirectoryInfo(Path.GetTempPath());
        //    //DateTime now = DateTime.UtcNow;
        //    FileInfo fiOutput = new FileInfo(Path.GetTempFileName());

        //    using (FileStream fs = new FileStream(fiOutput.FullName, FileMode.Create, FileAccess.Write, FileShare.Write))
        //    {
        //        streamTarget.DumpToStream(fs, mongoConnectionString, databaseName, outputData);
        //    }

        //    return fiOutput;
        //}

        public void RestoreFromFile(string mongoConnectionString, string archivePath, string sourceDatabaseName, string destinationDatabaseName, Action<string> outputData = null)
        {
            StreamTarget streamTarget = new StreamTarget(_mongorestoreExe.Directory);
            FileInfo fiArchivePath = new FileInfo(archivePath);

            using (FileStream fs = new FileStream(fiArchivePath.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                streamTarget.RestoreFromStream(fs, mongoConnectionString, sourceDatabaseName, destinationDatabaseName, outputData);
            }
        }
    }
}
