using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharkTank.DynamoDB.Utilities
{
    public static class DynamoHelpers
    {
        public static async Task CreateTableIfNotExistsAndWaitForTableToBecomeAvailable(this IAmazonDynamoDB client, CreateTableRequest createTableRequest)
        {
            TableDescription table = null;

            try
            {
                DescribeTableRequest descRequest = new DescribeTableRequest() { TableName = createTableRequest.TableName };
                table = (await client.DescribeTableAsync(descRequest)).Table;
            }
            catch (AmazonServiceException ase)
            {
                if (!ase.ErrorCode.Equals("ResourceNotFoundException", StringComparison.InvariantCultureIgnoreCase))
                    throw ase;
            }

            // We are all done if it exists already
            if (table != null && table.TableStatus == TableStatus.ACTIVE)
                return;

            if (table == null)
                table = (await client.CreateTableAsync(createTableRequest)).TableDescription;

            if (table.TableStatus != TableStatus.ACTIVE)
                await client.WaitForTableToBecomeAvailable(createTableRequest.TableName);
        }

        public static async Task WaitForTableToBecomeAvailable(this IAmazonDynamoDB client, string tableName)
        {
            TableStatus tableStatus = null;
            DateTime startTime = DateTime.UtcNow;
            DateTime endTime = startTime + new TimeSpan(0, 10, 0);

            while (DateTime.UtcNow < endTime)
            {
                Thread.Sleep(1000 * 20);
                try
                {
                    DescribeTableRequest request = new DescribeTableRequest() { TableName = tableName };
                    TableDescription table = (await client.DescribeTableAsync(request)).Table;

                    if (table == null)
                        continue;

                    tableStatus = table.TableStatus;
                    if (tableStatus == TableStatus.ACTIVE)
                        return;
                }
                catch (AmazonServiceException ase)
                {
                    if (!ase.ErrorCode.Equals("ResourceNotFoundException", StringComparison.InvariantCultureIgnoreCase))
                        throw ase;
                }
            }

            throw new Exception($"Failed to wait for table '{tableName}' to become available. After 10 minutes it finished on '{tableStatus}' status.");
        }
    }
}
