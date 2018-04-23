using System;

namespace SharkSync.Interfaces.Entities
{
    public interface IAccount
    {
        Guid Id { get; set; }
        string Name { get; set; }
        string EmailAddress { get; set; }
        string AvatarUrl { get; set; }
        string GitHubId { get; set; }
        string GoogleId { get; set; }
        string MicrosoftId { get; set; }
    }
}
