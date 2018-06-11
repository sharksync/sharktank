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
            return await ProcessCloudFormationResponse("SUCCESS", data, request, context);
        }

        public static async Task<CloudFormationResponse> FailCloudFormationResponse(Exception ex, CloudFormationRequest request, ILambdaContext context)
        {
            // Limit exceptions strings to 2k
            return await ProcessCloudFormationResponse("FAILED", ex.ToString().Substring(0, Math.Min(2000, ex.ToString().Length)), request, context);
        }

        private static async Task<CloudFormationResponse> ProcessCloudFormationResponse(string status, object data, CloudFormationRequest request, ILambdaContext context)
        {
            var responseBody = new CloudFormationResponse
            {
                Status = status,
                Reason = $"See the details in CloudWatch Log Stream: {context?.LogStreamName}",
                PhysicalResourceId = context?.LogStreamName,
                StackId = request.StackId,
                RequestId = request.RequestId,
                LogicalResourceId = request.LogicalResourceId,
                Data = data
            };

            try
            {
                var jsonPayload = JsonConvert.SerializeObject(responseBody);

                context.Logger.Log($"ProcessCloudFormationResponse: {jsonPayload}");

                var client = new HttpClient();
                var jsonContent = new StringContent(jsonPayload);

                jsonContent.Headers.Remove("Content-Type");

                var postResponse = await client.PutAsync(request.ResponseURL, jsonContent);

                postResponse.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                context.Logger.Log("Exception: " + ex.ToString());

                responseBody.Status = "FAILED";
                responseBody.Data = ex;
            }

            return responseBody;
        }
    }
}