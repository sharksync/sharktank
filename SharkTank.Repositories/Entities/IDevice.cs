using System;

namespace SharkTank.Interfaces.Entities
{
    public interface IDevice
    {
        Guid Id { get; set; }
        Guid AppId { get; set; }
        Guid AccountId { get; set; }
        Guid SyncId { get; set; }
        string LastSeen { get; set; }
    }
}
