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

        public static async Task CompleteCloudFormation(object data, CloudFormationRequest request, ILambdaContext context)
        {
            await ProcessCloudFormationResponse(new CloudFormationResponse
            {
                Status = "SUCCESS",
                Reason = null,
                PhysicalResourceId = context?.LogStreamName,
                StackId = request.StackId,
                RequestId = request.RequestId,
                LogicalResourceId = request.LogicalResourceId,
                Data = data
            }, request, context);
        }

        public static async Task FailCloudFormation(Exception ex, CloudFormationRequest request, ILambdaContext context)
        {
            await ProcessCloudFormationResponse(new CloudFormationResponse
            {
                Status = "FAILED",
                Reason = ex.Message,
                PhysicalResourceId = context?.LogStreamName,
                StackId = request.StackId,
                RequestId = request.RequestId,
                LogicalResourceId = request.LogicalResourceId,
                Data = null
            }, request, context);
        }

        private static async Task ProcessCloudFormationResponse(CloudFormationResponse response, CloudFormationRequest request, ILambdaContext context)
        {
            try
            {
                var jsonPayload = JsonConvert.SerializeObject(response);

                context.Logger.Log($"ProcessCloudFormationResponse: {jsonPayload}");

                var client = new HttpClient();
                var jsonContent = new StringContent(jsonPayload);

                jsonContent.Headers.Remove("Content-Type");

                var postResponse = await client.PutAsync(request.ResponseURL, jsonContent);

                postResponse.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                context.Logger.Log("Exception in ProcessCloudFormationResponse: " + ex.ToString());
            }
        }
    }
}