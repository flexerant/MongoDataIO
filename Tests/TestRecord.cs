using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    public class TestRecord
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string Value { get; set; }
    }
}
