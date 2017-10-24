using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerless
{
    public interface IDynamoDBContextWithBatch : IDynamoDBContext
    {

        //
        // Summary:
        //     Creates a strongly-typed BatchGet object, allowing a batch-get operation against
        //     DynamoDB.
        //
        // Type parameters:
        //   T:
        //     Type of objects to get
        //
        // Returns:
        //     Empty strongly-typed BatchGet object
        BatchGet<T> CreateBatchGet<T>();

        //
        // Summary:
        //     Creates a strongly-typed BatchGet object, allowing a batch-get operation against
        //     DynamoDB.
        //
        // Parameters:
        //   operationConfig:
        //     Config object which can be used to override that table used.
        //
        // Type parameters:
        //   T:
        //     Type of objects to get
        //
        // Returns:
        //     Empty strongly-typed BatchGet object
        BatchGet<T> CreateBatchGet<T>(DynamoDBOperationConfig operationConfig);

        //
        // Summary:
        //     Creates a strongly-typed BatchWrite object, allowing a batch-write operation
        //     against DynamoDB.
        //
        // Type parameters:
        //   T:
        //     Type of objects to write
        //
        // Returns:
        //     Empty strongly-typed BatchWrite object
        BatchWrite<T> CreateBatchWrite<T>();

        //
        // Summary:
        //     Creates a strongly-typed BatchWrite object, allowing a batch-write operation
        //     against DynamoDB.
        //
        // Parameters:
        //   operationConfig:
        //     Config object which can be used to override that table used.
        //
        // Type parameters:
        //   T:
        //     Type of objects to write
        //
        // Returns:
        //     Empty strongly-typed BatchWrite object
        BatchWrite<T> CreateBatchWrite<T>(DynamoDBOperationConfig operationConfig);
    }
}
