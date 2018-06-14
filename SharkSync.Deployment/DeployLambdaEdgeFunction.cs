using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Core;
using Amazon.Lambda.Model;
using Newtonsoft.Json;

namespace SharkSync.Deployment
{
    public class DeployLambdaEdgeRequest : CloudFormationRequest
    {
        public ResourcePropertiesModel ResourceProperties { get; set; }

        public class ResourcePropertiesModel
        {
            public string FunctionName { get; set; }
            public string EmbeddedFileName { get; set; }
            public string RoleArn { get; set; }
        }
    }

    public class DeployLambdaEdgeFunction
    {
        public IAmazonLambda LambdaClient { get; private set; }

        // Lambda@edge functions must always be put in USEast1
        public DeployLambdaEdgeFunction() : this(new AmazonLambdaClient(RegionEndpoint.USEast1))
        {

        }

        public DeployLambdaEdgeFunction(IAmazonLambda lambdaClient)
        {
            LambdaClient = lambdaClient;
        }

        public async Task FunctionHandlerAsync(DeployLambdaEdgeRequest request, ILambdaContext context)
        {
            try
            {
                context.Logger.Log("LambdaEdgeFunction invoked: " + JsonConvert.SerializeObject(request));

                if (string.IsNullOrWhiteSpace(request.RequestType))
                    throw new ArgumentException($"Missing or empty RequestType");
                
                string lambdaVersionedArn = null;

                if (request.RequestType == "Create" || request.RequestType == "Update")
                {
                    if (string.IsNullOrWhiteSpace(request.ResourceProperties.RoleArn))
                        throw new ArgumentException($"Missing or empty ResourceProperties.RoleArn");

                    if (string.IsNullOrWhiteSpace(request.ResourceProperties.FunctionName))
                        throw new ArgumentException($"Missing or empty ResourceProperties.FunctionName");

                    if (string.IsNullOrWhiteSpace(request.ResourceProperties.EmbeddedFileName))
                        throw new ArgumentException($"Missing or empty ResourceProperties.EmbeddedFileName");
                    
                    Stream lambdaEdgeJsFile = GetLambdaEdgeMemoryStreamFromAssembly(request);
                    MemoryStream zipFile = AddFileStreamToZipAndReturnZipStream(lambdaEdgeJsFile, request.ResourceProperties.EmbeddedFileName);
                    
                    if (request.RequestType == "Create")
                    {
                        var createResponse = await LambdaClient.CreateFunctionAsync(new CreateFunctionRequest
                        {
                            FunctionName = request.ResourceProperties.FunctionName,
                            Handler = $"{Path.GetFileNameWithoutExtension(request.ResourceProperties.EmbeddedFileName)}.handler",
                            Code = new FunctionCode() { ZipFile = zipFile },
                            Runtime = Runtime.Nodejs610,
                            Publish = true,
                            Role = request.ResourceProperties.RoleArn
                        });

                        request.PhysicalResourceId = createResponse.FunctionArn;
                        lambdaVersionedArn = $"{createResponse.FunctionArn}:{createResponse.Version}";
                    }
                    else if (request.RequestType == "Update")
                    {
                        if (!request.PhysicalResourceId.StartsWith("arn:aws:lambda:"))
                        {

                            var updateResponse = await LambdaClient.UpdateFunctionCodeAsync(new UpdateFunctionCodeRequest
                            {
                                FunctionName = $"{request.PhysicalResourceId}",
                                ZipFile = zipFile,
                                Publish = true,
                            });

                            lambdaVersionedArn = $"{updateResponse.FunctionArn}:{updateResponse.Version}";
                        }
                        else
                            context.Logger.LogLine("PhysicalResourceId was not a lambda resource on UPDATE, skipping update command");
                    }
                }
                else if (request.RequestType == "Delete")
                {
                    if (request.PhysicalResourceId.StartsWith("arn:aws:lambda:"))
                    {
                        var updateResponse = await LambdaClient.DeleteFunctionAsync(new DeleteFunctionRequest
                        {
                            FunctionName = $"{request.PhysicalResourceId}"
                        });
                    }
                    else
                        context.Logger.LogLine("PhysicalResourceId was not a lambda resource on DELETE, skipping remove command");
                }
                
                await CloudFormationResponse.CompleteCloudFormation(new { LambdaVersionedArn = lambdaVersionedArn }, request, context);
            }
            catch (Exception ex)
            {
                await CloudFormationResponse.FailCloudFormation(ex, request, context);

                throw;
            }
        }

        private Stream GetLambdaEdgeMemoryStreamFromAssembly(DeployLambdaEdgeRequest request)
        {
            string embeddedResourcePath = $"{GetType().Namespace}.LambdaEdgeFunctions.{request.ResourceProperties.EmbeddedFileName}";
            Stream jsResourceStream = GetType().Assembly.GetManifestResourceStream(embeddedResourcePath);

            if (jsResourceStream == null)
                throw new Exception($"Failed to find an embedded resource at {embeddedResourcePath}");
            
            return jsResourceStream;
        }

        private static MemoryStream AddFileStreamToZipAndReturnZipStream(Stream fileStream, string fileName)
        {
            using (var zipStream = new MemoryStream())
            {
                using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
                {
                    ZipArchiveEntry jsEntry = archive.CreateEntry(fileName);

                    // Lock in a fixed date time to ensure the hash always comes out the same
                    jsEntry.LastWriteTime = new DateTime(2000, 1, 1);

                    // Update the file permissions to give it -rw-rw-r--
                    jsEntry.ExternalAttributes = -2118909952;

                    using (Stream entryStream = jsEntry.Open())
                    {
                        fileStream.CopyTo(entryStream);
                    }
                }
                
                return new MemoryStream(zipStream.ToArray());
            }
        }
    }
}
