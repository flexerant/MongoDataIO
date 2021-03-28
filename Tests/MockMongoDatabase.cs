using Mongo2Go;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    public class MockMongoDatabase : IDisposable
    {
        private MongoDbRunner _runner;
        public IMongoDatabase Database { get; private set; }
        public string ConnectionString { get; private set; }
        public string DatabaseName { get; private set; }

        public MockMongoDatabase()
        {
            _runner = MongoDbRunner.Start();

            MongoClient client = new MongoClient(_runner.ConnectionString);

            this.DatabaseName = typeof(MockMongoDatabase).Name;
            this.ConnectionString = _runner.ConnectionString;
            this.Database = client.GetDatabase(this.DatabaseName);
        }

        public IMongoCollection<T> GetCollection<T>()
        {
            return this.Database.GetCollection<T>($"{typeof(T).Name}s");
        }

        public void Dispose()
        {
            ((IDisposable)_runner).Dispose();
        }
    }
}
