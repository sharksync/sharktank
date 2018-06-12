using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Core;
using Amazon.Lambda.Model;
using Newtonsoft.Json;

namespace SharkSync.Deployment
{
    public class LambdaEdgeRequest : CloudFormationRequest
    {
        public ResourcePropertiesModel ResourceProperties { get; set; }

        public class ResourcePropertiesModel
        {
            public string FunctionName { get; set; }
            public string EmbeddedFileName { get; set; }
            public string RoleArn { get; set; }
            public string CloudFrontArn { get; set; }
            public string EventType { get; set; }
        }
    }

    public class LambdaEdgeFunction
    {
        public IAmazonLambda LambdaClient { get; private set; }

        // Lambda@edge functions must always be put in USEast1
        public LambdaEdgeFunction() : this(new AmazonLambdaClient(RegionEndpoint.USEast1))
        {

        }

        public LambdaEdgeFunction(IAmazonLambda lambdaClient)
        {
            LambdaClient = lambdaClient;
        }

        public async Task FunctionHandlerAsync(LambdaEdgeRequest request, ILambdaContext context)
        {
            try
            {
                context.Logger.Log("LambdaEdgeFunction invoked: " + JsonConvert.SerializeObject(request));
                
                if (string.IsNullOrWhiteSpace(request.RequestType))
                    throw new ArgumentException($"Missing or empty RequestType");

                byte[] zipBytes = null;

                if (request.RequestType != "Delete")
                {
                    if (string.IsNullOrWhiteSpace(request.ResourceProperties.RoleArn))
                        throw new ArgumentException($"Missing or empty ResourceProperties.RoleArn");
                
                    if (string.IsNullOrWhiteSpace(request.ResourceProperties.FunctionName))
                        throw new ArgumentException($"Missing or empty ResourceProperties.FunctionName");
                    
                    if (string.IsNullOrWhiteSpace(request.ResourceProperties.EmbeddedFileName))
                        throw new ArgumentException($"Missing or empty ResourceProperties.EmbeddedFileName");
                    
                    string embeddedResourcePath = $"{GetType().Namespace}.LambdaEdgeFunctions.{request.ResourceProperties.EmbeddedFileName}";
                    Stream jsResourceStream = GetType().Assembly.GetManifestResourceStream(embeddedResourcePath);

                    if (jsResourceStream == null)
                        throw new Exception($"Failed to find an embedded resource at {embeddedResourcePath}");

                    using (var zipStream = new MemoryStream())
                    {
                        using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
                        {
                            ZipArchiveEntry jsEntry = archive.CreateEntry(request.ResourceProperties.EmbeddedFileName);
                            using (Stream entryStream = jsEntry.Open())
                            {
                                jsResourceStream.CopyTo(entryStream);
                            }
                        }
                        
                        zipBytes = zipStream.ToArray();
                    }
                }
                
                  //LambdaFunctionAssociations:
                  //  - EventType: origin-request
                  //    LambdaFunctionARN: arn:aws:lambda:us-east-1:429810410321:function:sharksync-web-origin-request:3

                string physicalResourceId = request.PhysicalResourceId;

                if (request.RequestType == "Create")
                {
                    var createResponse = await LambdaClient.CreateFunctionAsync(new CreateFunctionRequest
                    {
                        FunctionName = $"{request.ResourceProperties.FunctionName}",
                        Handler = "index.handler",
                        Code = new FunctionCode() { ZipFile = new MemoryStream(zipBytes) },
                        Runtime = Runtime.Nodejs810,
                        Publish = true,
                        Role = request.ResourceProperties.RoleArn
                    });

                    physicalResourceId = createResponse.FunctionArn;
                }
                else if (request.RequestType == "Update")
                {
                    var updateResponse = await LambdaClient.UpdateFunctionCodeAsync(new UpdateFunctionCodeRequest
                    {
                        FunctionName = $"{physicalResourceId}",
                        ZipFile = new MemoryStream(zipBytes),
                        Publish = true,
                    });
                }
                else if (request.RequestType == "Delete")
                {
                    var updateResponse = await LambdaClient.DeleteFunctionAsync(new DeleteFunctionRequest
                    {
                        FunctionName = $"{physicalResourceId}"
                    });
                }
                else
                    throw new ArgumentException($"Unknown RequestType {request.RequestType}");

                await CloudFormationResponse.CompleteCloudFormation(null, physicalResourceId, request, context);
            }
            catch (Exception ex)
            {
                await CloudFormationResponse.FailCloudFormation(ex, request, context);

                throw;
            }
        }
    }
}
