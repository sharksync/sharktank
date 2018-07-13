using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SharkSync.Interfaces;
using System;
using System.Threading.Tasks;

namespace SharkSync.Services
{
    public class SettingsService : ISettingsService
    {
        IAmazonSecretsManager SecretsManager { get; set; }

        AppSettings AppSettings { get; set; }

        public SettingsService(IOptions<AppSettings> appSettingsOptions, IAmazonSecretsManager secretManager)
        {
            SecretsManager = secretManager;
            AppSettings = appSettingsOptions?.Value;
        }

        public async Task<T> Get<T>() where T : class
        {
            string secretId;

            if (typeof(T) == typeof(ConnectionStringSettings))
                secretId = AppSettings.ConnectionSecretId;
            else if (typeof(T) == typeof(OAuthSettings))
                secretId = AppSettings.AuthSecretId;
            else
                throw new Exception($"Unknown settings class of type: {typeof(T).Name}");

            var result = await SecretsManager.GetSecretValueAsync(new GetSecretValueRequest { SecretId = secretId });

            if (result == null || string.IsNullOrWhiteSpace(result.SecretString))
                throw new Exception($"Missing AWS SecretsManager value for \"{secretId}\" secret");

            var oAuthSecret = JsonConvert.DeserializeObject<T>(result.SecretString);
            return oAuthSecret;
        }
    }

    public class AppSettings
    {
        public string AuthSecretId { get; set; }
        public string ConnectionSecretId { get; set; }
        public string DataProtectionSecretId { get; set; }
    }
}
