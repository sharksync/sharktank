using SharkSync.Interfaces.Entities;
using System;

namespace SharkSync.PostgreSQL.Entities
{
    public class Change : IChange
    {
        public string Group { get; set; }

        public long Tidemark { get; set; }

        public Guid RecordId { get; set; }

        public string Path { get; set; }

        public DateTime Modified { get; set; }

        public string Value { get; set; }
    }
}
