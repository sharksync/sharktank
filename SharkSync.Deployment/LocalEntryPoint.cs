using Amazon;
using Amazon.Lambda.TestUtilities;
using Amazon.S3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SharkSync.Deployment
{
    /// <summary>
    /// The Main function can be used to run lambda locally
    /// </summary>
    public class LocalEntryPoint
    {
        public static async Task Main(string[] args)
        {
            await new DeployLambdaEdgeFunction()
                .FunctionHandlerAsync(new DeployLambdaEdgeRequest
                {
                    RequestType = "Update",
                    PhysicalResourceId = "arn:aws:lambda:us-east-1:429810410321:function:shark-sync-origin-request",
                    ResourceProperties = new DeployLambdaEdgeRequest.ResourcePropertiesModel()
                    {
                        EmbeddedFileName = "origin-response.js",
                        FunctionName = "shark-sync-origin-response",
                        RoleArn = "arn:aws:iam::429810410321:role/lambda-edge-role",
                        ReplacementPairs = new List<string>() { "ServiceUrl=https://localhost:44325" }
                    }
                }, new TestLambdaContext());
        }
    }
}
