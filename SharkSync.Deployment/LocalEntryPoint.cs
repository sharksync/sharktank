using Amazon;
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
            await new UnpackZipIntoS3BucketFunction(new AmazonS3Client(RegionEndpoint.EUWest1))
                .FunctionHandlerAsync(new UnpackZipIntoS3BucketRequest
                {
                    ResourceProperties = new UnpackZipIntoS3BucketRequest.ResourcePropertiesModel()
                    {
                        ZipS3Bucket = "io.sharksync.builds",
                        ZipS3Key = "v1.0.9/SharkSync.Web.Html.zip",
                        OutputS3Bucket = "io.sharksync.web"
                    }
                }, null);
        }
    }
}
