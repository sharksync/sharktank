using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.S3;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SharkSync.Deployment
{
    public class UnpackZipIntoS3BucketRequest : CloudFormationRequest
    {
        public ResourcePropertiesModel ResourceProperties { get; set; }

        public class ResourcePropertiesModel
        {
            public string ZipS3Bucket { get; set; }
            public string ZipS3Key { get; set; }
            public string OutputS3Bucket { get; set; }
            public string OutputPrefix { get; set; }
        }
    }

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

        public async Task<CloudFormationResponse> FunctionHandlerAsync(UnpackZipIntoS3BucketRequest request, ILambdaContext context)
        {
            try
            {

                context.Logger.Log("UnpackZipIntoS3BucketFunction invoked: " + JsonConvert.SerializeObject(request));

                if (request.RequestType != "Delete")
                {
                    using (var zipStream = await S3Client.GetObjectStreamAsync(request.ResourceProperties.ZipS3Bucket, request.ResourceProperties.ZipS3Key, null))
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
                                            BucketName = request.ResourceProperties.OutputS3Bucket,
                                            Key = Path.Combine(request.ResourceProperties.OutputPrefix ?? "", entry.FullName),
                                            InputStream = ms
                                        });
                                    }
                                }
                            }
                        }
                    }
                }

                return await CloudFormationResponse.CompleteCloudFormationResponse(null, request, context);
            }
            catch (Exception ex)
            {
                return await CloudFormationResponse.CompleteCloudFormationResponse(ex, request, context);
            }
        }
    }
}
