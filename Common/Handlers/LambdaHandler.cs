using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Runtime;

namespace Common.Handlers
{
    public class LambdaHandler
    {
        private static readonly string isLambda;
        private static readonly AmazonLambdaClient LambdaClient;
        static LambdaHandler()
        {
            isLambda = System.Environment.GetEnvironmentVariable("LAMBDA_TASK_ROOT") ?? string.Empty;

            if (string.IsNullOrEmpty(isLambda))
            {
                LambdaClient = new AmazonLambdaClient(new AmazonLambdaConfig
                {
                    RegionEndpoint = RegionEndpoint.SAEast1,
                    Profile = new Profile("BrowserExtensionAnalysis")
                });
            }
            else
            {
                LambdaClient = new AmazonLambdaClient();
            }
        }

        public static Task<InvokeResponse> CallFunction(string functionName, string payload, bool isEvent)
        {

            var call = LambdaClient.InvokeAsync(new InvokeRequest
            {
                FunctionName = functionName,
                Payload = payload,
                InvocationType = isEvent ? "Event" : "RequestResponse"
            });

            return call;
        }
    }
}
