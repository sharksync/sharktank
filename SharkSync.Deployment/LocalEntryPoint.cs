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
                .FunctionHandlerAsync("io.sharksync.builds", "v1.0.9/SharkSync.Web.Html.zip", "io.sharksync.web", null, null);
        }
    }
}
