using System;

namespace SharkTank.Interfaces.Entities
{
    public interface IAccount
    {
        Guid Id { get; set; }
        string Name { get; set; }
    }
}
