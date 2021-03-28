using Flexerant.MongoDataIO.Core;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using System.Linq;

namespace Tests
{
    public class BackupAndRestoreTests : IClassFixture<BlobStorageFixture>
    {
        private readonly BlobStorageFixture _blobStorageFixture;

        public BackupAndRestoreTests(BlobStorageFixture fixture)
        {
            _blobStorageFixture = fixture;
        }

        [Fact]
        public void FileTargetWithoutDelegate()
        {
            this.FileTarget(null, null);
        }

        [Fact]
        public void FileTargetWithDelegate()
        {
            StringBuilder sbBackup = new StringBuilder();
            StringBuilder sbRestore = new StringBuilder();

            this.FileTarget(backupResults => sbBackup.AppendLine(backupResults), restoreResults => sbRestore.AppendLine(restoreResults));

            Assert.True(!string.IsNullOrWhiteSpace(sbBackup.ToString()));
            Assert.True(!string.IsNullOrWhiteSpace(sbRestore.ToString()));
        }

        private void FileTarget(Action<string> backupResultsDelegate, Action<string> restoreDelegate)
        {
            Helpers.ClearTempFolder();

            FileTarget target = new FileTarget(Helpers.GetMongoToolesFolder());
            DirectoryInfo outputFolder = Helpers.GetTempFolder();
            List<TestRecord> backupRecords;

            using (var db = new MockMongoDatabase())
            {
                backupRecords = this.InitTestRecords(db);

                target.DumpToFile(db.ConnectionString, db.DatabaseName, outputFolder.FullName, lineData => backupResultsDelegate?.Invoke(lineData));
            }

            FileInfo fiBackup = outputFolder.GetFiles().First();

            using (var db = new MockMongoDatabase())
            {
                var collection = db.GetCollection<TestRecord>();
                var filter = Builders<TestRecord>.Filter.Empty;

                Assert.Empty(collection.Find(filter).ToList());

                target.RestoreFromFile(db.ConnectionString, db.DatabaseName, fiBackup.FullName, lineData => restoreDelegate?.Invoke(lineData));

                var restoredRecords = collection.Find(filter).ToList();

                Assert.Equal(backupRecords.Count(), restoredRecords.Count());

                foreach (var record in backupRecords)
                {
                    Assert.True(restoredRecords.FirstOrDefault(x => x.Id == record.Id & x.Value == record.Value) != null);
                }
            }
        }

        [Fact]
        public void AzureTargetWithDelegate()
        {
            StringBuilder sbBackup = new StringBuilder();
            StringBuilder sbRestore = new StringBuilder();

            this.AzureTarget(backupResults => sbBackup.AppendLine(backupResults), restoreResults => sbRestore.AppendLine(restoreResults));

            Assert.True(!string.IsNullOrWhiteSpace(sbBackup.ToString()));
            Assert.True(!string.IsNullOrWhiteSpace(sbRestore.ToString()));
        }

        [Fact]
        public void AzureTargetWithoutDelegate()
        {
            this.AzureTarget(null, null);
        }

        private void AzureTarget(Action<string> backupResultsDelegate, Action<string> restoreDelegate)
        {
            AzureTarget target = new AzureTarget(Helpers.GetMongoToolesFolder());
            List<TestRecord> backupRecords;
            DumpToAzureResult result;

            using (var db = new MockMongoDatabase())
            {
                backupRecords = this.InitTestRecords(db);
                result = target.DumpToAzure(db.ConnectionString, db.DatabaseName, _blobStorageFixture.ConnectionString, lineData => backupResultsDelegate?.Invoke(lineData));
            }

            using (var db = new MockMongoDatabase())
            {
                var collection = db.GetCollection<TestRecord>();
                var filter = Builders<TestRecord>.Filter.Empty;

                Assert.Empty(collection.Find(filter).ToList());

                target.RestoreFromAzure(db.ConnectionString, _blobStorageFixture.ConnectionString, result.ContainerName, result.BlobName, lineData => restoreDelegate?.Invoke(lineData));

                var restoredRecords = collection.Find(filter).ToList();

                Assert.Equal(backupRecords.Count(), restoredRecords.Count());

                foreach (var record in backupRecords)
                {
                    Assert.True(restoredRecords.FirstOrDefault(x => x.Id == record.Id & x.Value == record.Value) != null);
                }
            }
        }

        private List<TestRecord> InitTestRecords(MockMongoDatabase db)
        {
            var random = new Random();
            var recordCount = random.Next(100, 1000);
            var collection = db.GetCollection<TestRecord>();

            for (int i = 0; i < recordCount; i++)
            {
                collection.InsertOne(new TestRecord() { Id = ObjectId.GenerateNewId(), Value = Guid.NewGuid().ToString() });
            }

            var filter = Builders<TestRecord>.Filter.Empty;

            return collection.Find(filter).ToList();
        }
    }
}
