using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Amazon;
using Amazon.CloudFront;
using Amazon.CloudFront.Model;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

namespace SharkSync.Deployment
{
    public class InvalidateCloudFrontRequest : CloudFormationRequest
    {
        public ResourcePropertiesModel ResourceProperties { get; set; }

        public class ResourcePropertiesModel
        {
            public string DistributionId { get; set; }
            public List<string> Paths { get; set; }
            public string CallerReference { get; set; }
        }
    }

    public class InvalidateCloudFrontFunction
    {
        public IAmazonCloudFront CloudFrontClient { get; private set; }

        // CloudFront@edge functions must always be put in USEast1
        public InvalidateCloudFrontFunction() : this(new AmazonCloudFrontClient(RegionEndpoint.USEast1))
        {

        }

        public InvalidateCloudFrontFunction(IAmazonCloudFront cloudFrontClient)
        {
            CloudFrontClient = cloudFrontClient;
        }

        public async Task FunctionHandlerAsync(InvalidateCloudFrontRequest request, ILambdaContext context)
        {
            try
            {
                context.Logger.Log("InvalidateCloudFrontFunction invoked: " + JsonConvert.SerializeObject(request));
                
                if (request.RequestType != "Delete")
                {
                    if (string.IsNullOrWhiteSpace(request.ResourceProperties.DistributionId))
                        throw new ArgumentException($"Missing or empty ResourceProperties.DistributionId");

                    if (request.ResourceProperties.Paths == null || !request.ResourceProperties.Paths.Any())
                        throw new ArgumentException($"Missing or empty ResourceProperties.Paths");

                    await CloudFrontClient.CreateInvalidationAsync(new CreateInvalidationRequest
                    {
                        DistributionId = request.ResourceProperties.DistributionId,
                        InvalidationBatch = new InvalidationBatch
                        {
                            CallerReference = request.ResourceProperties.CallerReference,
                            Paths = new Paths
                            {
                                Quantity = request.ResourceProperties.Paths.Count,
                                Items = request.ResourceProperties.Paths
                            }
                        }
                    });
                }
                
                await CloudFormationResponse.CompleteCloudFormation(null, request, context);
            }
            catch (Exception ex)
            {
                await CloudFormationResponse.FailCloudFormation(ex, request, context);

                throw;
            }
        }
    }
}
