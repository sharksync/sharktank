using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.Options;
using SharkSync.Interfaces;
using SharkSync.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SharkSync.Web.Api.Services
{
    public class AmazonSecretXmlRepository : IXmlRepository
    {
        private static readonly XName RepositoryElementName = "repository";
        private readonly IAmazonSecretsManager _secretsManager;
        private readonly string _key;
        private readonly object _readWriteLock = new object();

        /// <summary>
        /// Creates a <see cref="RedisXmlRepository"/> with keys stored at the given directory.
        /// </summary>
        /// <param name="secretsManager">An instance of the AWS IAmazonSecretsManager</param>
        /// <param name="key">The <see cref="RedisKey"/> used to store key list.</param>
        public AmazonSecretXmlRepository(IAmazonSecretsManager secretsManager, IOptions<AppSettings> appSettingsOptions)
        {
            _secretsManager = secretsManager;
            _key = appSettingsOptions?.Value?.DataProtectionSecretId;

            if (string.IsNullOrWhiteSpace(_key))
                throw new Exception("Missing DataProtectionSecretId in appsettings");
        }

        /// <inheritdoc />
        public IReadOnlyCollection<XElement> GetAllElements()
        {
            lock (_readWriteLock)
            {
                var elements = Task.Run(() => GetAllElementsAsync()).GetAwaiter().GetResult();
                return elements.ToList().AsReadOnly();
            }
        }

        /// <inheritdoc />
        public void StoreElement(XElement element, string friendlyName)
        {
            lock (_readWriteLock)
            {
                Task.Run(() => StoreElementAsync(element)).GetAwaiter().GetResult();
            }
        }

        private async Task<List<XElement>> GetAllElementsAsync()
        {
            var secretStream = await TryGetSecretAsync();

            if (secretStream == null)
                return new List<XElement>();

            var doc = CreateDocumentFromStream(secretStream);
            return doc.Root.Elements().ToList();
        }

        private async Task StoreElementAsync(XElement element)
        {
            var secretStream = await TryGetSecretAsync();
            XDocument doc = null;

            if (secretStream == null)
                doc = new XDocument(new XElement(RepositoryElementName));
            else
                doc = CreateDocumentFromStream(secretStream);
            doc.Root.Add(element);

            var serializedDoc = new MemoryStream();
            doc.Save(serializedDoc, SaveOptions.DisableFormatting);

            if (secretStream == null)
                await _secretsManager.CreateSecretAsync(new CreateSecretRequest { Name = _key, SecretBinary = serializedDoc });
            else
                await _secretsManager.UpdateSecretAsync(new UpdateSecretRequest { SecretId = _key, SecretBinary = serializedDoc });
        }

        private async Task<MemoryStream> TryGetSecretAsync()
        {
            try
            {
                var secretValue = await _secretsManager.GetSecretValueAsync(new GetSecretValueRequest { SecretId = _key });
                return secretValue?.SecretBinary;
            }
            catch (ResourceNotFoundException)
            {
                // Key has not been created yet

                return null;
            }
        }

        private XDocument CreateDocumentFromStream(MemoryStream memoryStream)
        {
            var xmlReaderSettings = new XmlReaderSettings()
            {
                DtdProcessing = DtdProcessing.Prohibit,
                IgnoreProcessingInstructions = true
            };

            using (var xmlReader = XmlReader.Create(memoryStream, xmlReaderSettings))
            {
                return XDocument.Load(xmlReader);
            }
        }

    }
}
