using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.S3.Notifications;
using Constructs;

namespace Infra;

public class InfraStack : Stack
{
    public InfraStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        // S3 Bucket
        var bucket = new Bucket(this, "FileUploadBucket", new BucketProps
        {
            Versioned = true
        });

        // Lambda Function
        var lambda = new Function(this, "FileProcessorLambda", new FunctionProps
        {
            Runtime = Runtime.DOTNET_8,
            Handler = "S3LambdaFileProcessor::Function::FunctionHandler",
            Code = Code.FromAsset("../../../S3LambdaFileProcessor/bin/Release/net8.0"),
            MemorySize = 256,
            Timeout = Duration.Seconds(30)
        });

        // Permissions
        bucket.GrantReadWrite(lambda);

        // Trigger Lambda on upload
        bucket.AddEventNotification(EventType.OBJECT_CREATED, new LambdaDestination(lambda));
    }
}
