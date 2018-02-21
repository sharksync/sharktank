using System;

namespace SharkTank.Interfaces.Entities
{
    public interface IChange
    {
        Guid RecordId { get; set; }
        string Group { get; set; }
        string Path { get; set; }
        Guid DeviceId { get; set; }
        string Tidemark { get; set; }
        DateTime Modified { get; set; }
        string Value { get; set; }
    }
}
