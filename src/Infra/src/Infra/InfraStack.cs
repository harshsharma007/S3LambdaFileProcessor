using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.S3.Notifications;
using Constructs;
using System.Collections.Generic;

namespace Infra;

public class InfraStack : Stack
{
    public InfraStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        // S3 Bucket
        var bucket = new Bucket(this, "FileUploadBucket", new BucketProps
        {
            BucketName = $"file-processor-{this.Account}-{this.Region}-{this.Node.Id.ToLower()}",
            RemovalPolicy = RemovalPolicy.DESTROY,
            AutoDeleteObjects = true,
            Versioned = true,
            BlockPublicAccess = BlockPublicAccess.BLOCK_ALL
        });

        // Lambda Function
        var lambda = new Function(this, "FileProcessorLambda", new FunctionProps
        {
            Runtime = Runtime.DOTNET_8,
            // This line means Run: Function::FunctionHandler from S3LambdaFileProcessor.dll
            Handler = "S3LambdaFileProcessor::Function::FunctionHandler",
            Code = Code.FromAsset("../../publish"),
            MemorySize = 256,
            Timeout = Duration.Seconds(30),
            Environment = new Dictionary<string, string>
            {
                { "BUCKET_NAME", bucket.BucketName }
            },
            LogRetention = RetentionDays.ONE_WEEK
        });

        // Permissions
        bucket.GrantReadWrite(lambda);

        // Trigger Lambda on upload
        bucket.AddEventNotification(EventType.OBJECT_CREATED, new LambdaDestination(lambda), new NotificationKeyFilter { Prefix = "raw/" });
    }
}
