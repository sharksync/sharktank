using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Core;
using Amazon.Lambda.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SharkSync.Interfaces;
using SharkSync.PostgreSQL;
using SharkSync.Services;

namespace SharkSync.Deployment
{
    public class MigrateDatabaseRequest : CloudFormationRequest
    {
        public ResourcePropertiesModel ResourceProperties { get; set; }

        public class ResourcePropertiesModel
        {
            public string DatabaseServer { get; set; }
            public string DatabaseName { get; set; }
            public string DatabaseUsername { get; set; }
            public string DatabasePassword { get; set; }

            // This isn't used but we can use it to force Cloudformation to update our resource every version
            public string Version { get; set; }
        }
    }

    public class MigrateDatabaseFunction
    {
        public IAmazonLambda LambdaClient { get; private set; }

        public MigrateDatabaseFunction()
        {

        }

        public async Task FunctionHandlerAsync(MigrateDatabaseRequest request, ILambdaContext context)
        {
            try
            {
                context.Logger.Log("LambdaEdgeFunction invoked: " + JsonConvert.SerializeObject(request));

                if (string.IsNullOrWhiteSpace(request.RequestType))
                    throw new ArgumentException($"Missing or empty RequestType");

                string lambdaVersionedArn = null;

                if (request.RequestType == "Create" || request.RequestType == "Update")
                {
                    if (string.IsNullOrWhiteSpace(request.ResourceProperties.DatabaseServer))
                        throw new ArgumentException($"Missing or empty ResourceProperties.DatabaseServer");

                    if (string.IsNullOrWhiteSpace(request.ResourceProperties.DatabaseName))
                        throw new ArgumentException($"Missing or empty ResourceProperties.DatabaseName");

                    if (string.IsNullOrWhiteSpace(request.ResourceProperties.DatabaseUsername))
                        throw new ArgumentException($"Missing or empty ResourceProperties.DatabaseUsername");

                    if (string.IsNullOrWhiteSpace(request.ResourceProperties.DatabasePassword))
                        throw new ArgumentException($"Missing or empty ResourceProperties.DatabasePassword");


                    DataContext db = new DataContext(new DbContextOptions<DataContext>(), new FixedSettingsService(request.ResourceProperties));
                    db.Database.Migrate();
                }

                await CloudFormationResponse.CompleteCloudFormation(new { LambdaVersionedArn = lambdaVersionedArn }, request, context);
            }
            catch (Exception ex)
            {
                await CloudFormationResponse.FailCloudFormation(ex, request, context);

                throw;
            }
        }

        public class FixedSettingsService : ISettingsService
        {
            public MigrateDatabaseRequest.ResourcePropertiesModel Properties { get; set; }

            public FixedSettingsService(MigrateDatabaseRequest.ResourcePropertiesModel dbProperties)
            {
                Properties = dbProperties;
            }

            public Task<T> Get<T>() where T : class
            {
                return (Task<T>)(object)Task.FromResult(new ConnectionStringSettings()
                {
                    Host = Properties.DatabaseServer,
                    DBName = Properties.DatabaseName,
                    Username = Properties.DatabaseUsername,
                    Password = Properties.DatabasePassword
                });
            }
        }
    }
}
