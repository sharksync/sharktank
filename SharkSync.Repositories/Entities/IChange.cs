using System;

namespace SharkSync.Interfaces.Entities
{
    public interface IChange
    {
        Guid RecordId { get; set; }
        string Group { get; set; }
        string Path { get; set; }
        long Tidemark { get; set; }
        DateTime Modified { get; set; }
        string Value { get; set; }
    }
}
