using System;

namespace SharkSync.Interfaces.Entities
{
    public interface IApplication
    {
        Guid Id { get; set; }
        Guid AccessKey { get; set; }
        string Name { get; set; }
        Guid AccountId { get; set; }
    }
}
