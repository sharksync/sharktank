
using System.Threading.Tasks;

namespace SharkSync.Interfaces
{
    public interface ISettingsService
    {
        Task<T> Get<T>() where T : class;
    }

    public class ConnectionStringSettings
    {
        public string Username { get; set; }
        public string Engine { get; set; }
        public string DBName { get; set; }
        public string Host { get; set; }
        public string Password { get; set; }
        public string Port { get; set; }
        public string DBInstanceIdentifier { get; set; }

        public string GetConnectionString()
        {
            return $"Host={Host};Database={DBName};Username={Username};Password={Password}";
        }
    }

    public class OAuthSettings
    {
        public string GitHubClientId { get; set; }
        public string GitHubClientSecret { get; set; }
        public string GoogleClientId { get; set; }
        public string GoogleClientSecret { get; set; }
        public string MicrosoftApplicationId { get; set; }
        public string MicrosoftPassword { get; set; }

        public string ClientAppRootUrl { get; set; }
    }

}
