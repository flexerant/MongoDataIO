using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using CliWrap;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Flexerant.MongoDataIO.Core
{
    public class StreamTarget
    {
        private readonly FileInfo _mongodumpExe;
        private readonly FileInfo _mongorestoreExe;

        public StreamTarget(string mongoBinFolder)
        {
            _mongodumpExe = new FileInfo(Path.Combine(mongoBinFolder, "mongodump.exe"));
            _mongorestoreExe = new FileInfo(Path.Combine(mongoBinFolder, "mongorestore.exe"));
        }

        public StreamTarget(DirectoryInfo mongoBinFolder) : this(mongoBinFolder.FullName) { }

        public void DumpToStream(Stream stream, string mongoConnectionString, string databaseName, Action<string> outputData = null)
        {
            if (!stream.CanWrite) throw new IOException("The stream must be writeable.");

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
                .WithStandardOutputPipe(PipeTarget.ToStream(stream))
                .WithValidation(CommandResultValidation.None);

            result = Task.Run(async () => await cmd.ExecuteAsync()).Result;

            if (result.ExitCode != 0)
            {
                throw new Exception(sb.ToString());
            }
        }

        public void RestoreFromStream(Stream stream, string mongoConnectionString, string sourceDatabaseName, string destinationDatabaseName, Action<string> outputData = null)
        {
            if (!stream.CanRead) throw new IOException("The stream must be readable.");

            string nsFrom = $"{sourceDatabaseName}.*";
            string nsTo = $"{destinationDatabaseName}.*";

            string uri = Helpers.FormatUri(mongoConnectionString);
    
            var cmd = Cli.Wrap(_mongorestoreExe.FullName)
                .WithArguments($"--uri {uri} --archive --nsFrom \"{nsFrom}\" --nsTo \"{nsTo}\"")
                .WithStandardInputPipe(PipeSource.FromStream(stream))
                .WithStandardErrorPipe(PipeTarget.ToDelegate(text => outputData?.Invoke(text), Encoding.ASCII));

            Task.Run(async () => await cmd.ExecuteAsync()).Wait();
        }
    }
}
