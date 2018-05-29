using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SharkSync.Deployment
{
    public class UnpackZipIntoS3BucketFunction
    {
        public void Handler(string zipS3Bucket, string zipS3Key, string outputS3Bucket, string outputPrefix, ILambdaContext context)
        {
            
            // TODO: Download zip
            // TODO: Unpack zip
            // TODO: Copy files into S3 bucket

        }
    }
}
