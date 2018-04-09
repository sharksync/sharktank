using System;

namespace SharkTank.Interfaces.Entities
{
    public interface IAccount
    {
        Guid Id { get; set; }
        string Name { get; set; }
        string EmailAddress { get; set; }
        int? GitHubId { get; set; }
        string AvatarUrl { get; set; }
    }
}
