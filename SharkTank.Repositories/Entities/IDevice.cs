using System;

namespace SharkTank.Interfaces.Entities
{
    public interface IDevice
    {
        Guid Id { get; set; }
        Guid ApplicationId { get; set; }
    }
}
