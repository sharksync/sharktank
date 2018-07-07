using System;

namespace SharkSync.Interfaces.Entities
{
    public interface IChange
    {
        long Id { get; set; }
        Guid AccountId { get; set; }
        Guid ApplicationId { get; set; }
        string GroupId { get; set; }
        string Entity { get; set; }
        Guid RecordId { get; set; }
        string Property { get; set; }
        long ClientModified { get; set; }
        string RecordValue { get; set; }
    }
}
