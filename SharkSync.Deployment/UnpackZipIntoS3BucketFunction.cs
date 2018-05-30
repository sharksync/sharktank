using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.S3;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SharkSync.Deployment
{
    public class UnpackZipIntoS3BucketFunction
    {
        public IAmazonS3 S3Client { get; private set; }

        public UnpackZipIntoS3BucketFunction() : this(new AmazonS3Client())
        {

        }

        public UnpackZipIntoS3BucketFunction(IAmazonS3 s3Client)
        {
            S3Client = s3Client;
        }

        public async Task FunctionHandlerAsync(string zipS3Bucket, string zipS3Key, string outputS3Bucket, string outputPrefix, ILambdaContext context)
        {
            using (var zipStream = await S3Client.GetObjectStreamAsync(zipS3Bucket, zipS3Key, null))
            {
                using (var zipArchive = new ZipArchive(zipStream))
                {
                    foreach (var entry in zipArchive.Entries)
                    {
                        using (var entryStream = entry.Open())
                        {
                            using (var ms = new MemoryStream())
                            {
                                entryStream.CopyTo(ms);

                                await S3Client.PutObjectAsync(new Amazon.S3.Model.PutObjectRequest
                                {
                                    BucketName = outputS3Bucket,
                                    Key = Path.Combine(outputPrefix ?? "", entry.FullName),
                                    InputStream = ms
                                });
                            }
                        }
                    }
                }
            }

        }
    }
}
