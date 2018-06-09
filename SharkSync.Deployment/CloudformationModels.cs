using Amazon.Lambda.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SharkSync.Deployment
{
    public class CloudFormationRequest
    {
        public string StackId { get; set; }
        public string ResponseURL { get; set; }
        public string RequestType { get; set; }
        public string ResourceType { get; set; }
        public string RequestId { get; set; }
        public string LogicalResourceId { get; set; }
    }

    public class CloudFormationResponse
    {
        public string Status { get; set; }
        public string Reason { get; set; }
        public string PhysicalResourceId { get; set; }
        public string StackId { get; set; }
        public string RequestId { get; set; }
        public string LogicalResourceId { get; set; }
        public object Data { get; set; }

        public static async Task<CloudFormationResponse> CompleteCloudFormationResponse(object data, CloudFormationRequest request, ILambdaContext context)
        {
            var responseBody = new CloudFormationResponse
            {
                Status = data is Exception ? "FAILED" : "SUCCESS",
                Reason = "See the details in CloudWatch Log Stream: " + context.LogStreamName,
                PhysicalResourceId = context.LogStreamName,
                StackId = request.StackId,
                RequestId = request.RequestId,
                LogicalResourceId = request.LogicalResourceId,
                Data = data
            };

            try
            {
                HttpClient client = new HttpClient();

                var jsonContent = new StringContent(JsonConvert.SerializeObject(responseBody));
                jsonContent.Headers.Remove("Content-Type");

                var postResponse = await client.PutAsync(request.ResponseURL, jsonContent);

                postResponse.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                LambdaLogger.Log("Exception: " + ex.ToString());

                responseBody.Status = "FAILED";
                responseBody.Data = ex;
            }

            return responseBody;
        }
    }
}