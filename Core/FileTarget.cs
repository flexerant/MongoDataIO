using CliWrap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public void DumpToFile(string clusterUri, string databaseName, string outputFolder, Action<string> outputData = null)
        {
            string uri = Helpers.FormatUri(clusterUri);
            DirectoryInfo diOutput = new DirectoryInfo(outputFolder);
            DateTime now = DateTime.UtcNow;
            FileInfo fiOutput = new FileInfo(Path.Combine(diOutput.FullName, $"{now.ToString("s").Replace(':', '-')}_{databaseName}.bak"));
            StringBuilder sb = new StringBuilder();

            var cmd = Cli.Wrap(_mongodumpExe.FullName)
                   .WithArguments($"--uri {uri}/{databaseName} --archive=\"{fiOutput.FullName}\"")
                   .WithStandardErrorPipe(PipeTarget.ToDelegate(text =>
                   {
                       sb.AppendLine(text);
                       outputData?.Invoke(text);
                   }, Encoding.ASCII))
                   .WithStandardOutputPipe(PipeTarget.ToDelegate(text =>
                   {
                       sb.AppendLine(text);
                       outputData?.Invoke(text);
                   }, Encoding.ASCII))
                   .WithValidation(CommandResultValidation.None);

            CommandResult result = Task.Run(async () => await cmd.ExecuteAsync()).Result;

            if (result.ExitCode != 0)
            {
                throw new Exception(sb.ToString());
            }


            //using (Process process = new Process())
            //{
            //    process.StartInfo.FileName = _mongodumpExe.FullName;
            //    process.StartInfo.Arguments = $"--uri {uri}/{databaseName} --archive=\"{fiOutput.FullName}\"";
            //    process.OutputDataReceived += (object sender, DataReceivedEventArgs e) => outputData?.Invoke(e.Data);
            //    process.ErrorDataReceived += (sendingProcess, errorLine) => outputData?.Invoke(errorLine.Data);
            //    process.Start();
            //    process.WaitForExit();
            //}
        }

        public void RestoreFromFile(string clusterUri, string databaseName, string archivePath, Action<string> outputData = null)
        {
            string uri = Helpers.FormatUri(clusterUri);
            FileInfo fiArchivePath = new FileInfo(archivePath);
            string nsFrom = $"{Path.GetFileNameWithoutExtension(fiArchivePath.Name)}.*";
            string nsTo = $"{databaseName}.*";
            StringBuilder sb = new StringBuilder();

            var cmd = Cli.Wrap(_mongorestoreExe.FullName)
                  .WithArguments($"--uri {uri} --archive=\"{fiArchivePath.FullName}\" --nsFrom \"{nsFrom}\" --nsTo \"{nsTo}\"")
                  .WithStandardErrorPipe(PipeTarget.ToDelegate(text =>
                  {
                      sb.AppendLine(text);
                      outputData?.Invoke(text);
                  }, Encoding.ASCII))
                  .WithStandardOutputPipe(PipeTarget.ToDelegate(text =>
                  {
                      sb.AppendLine(text);
                      outputData?.Invoke(text);
                  }, Encoding.ASCII))
                  .WithValidation(CommandResultValidation.None);

            CommandResult result = Task.Run(async () => await cmd.ExecuteAsync()).Result;

            if (result.ExitCode != 0)
            {
                throw new Exception(sb.ToString());
            }


            //using (Process process = new Process())
            //{
            //    process.StartInfo.FileName = _mongorestoreExe.FullName;
            //    process.StartInfo.Arguments = $"--uri {uri} --archive=\"{fiArchivePath.FullName}\" --nsFrom \"{nsFrom}\" --nsTo \"{nsTo}\"";
            //    process.OutputDataReceived += (object sender, DataReceivedEventArgs e) => outputData?.Invoke(e.Data);
            //    process.ErrorDataReceived += (sendingProcess, errorLine) => outputData?.Invoke(errorLine.Data);
            //    process.Start();
            //    process.WaitForExit();
            //}
        }
    }
}
