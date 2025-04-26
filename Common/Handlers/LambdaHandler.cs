using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Runtime.CredentialManagement;
using Res;

namespace Common.Handlers
{
    public class LambdaHandler
    {
        private static readonly AmazonLambdaClient LambdaClient;
        static LambdaHandler()
        {
            if (new SharedCredentialsFile().TryGetProfile(Keys.AWSProfile, out _))
            {
                LambdaClient = new AmazonLambdaClient(new AmazonLambdaConfig
                {
                    RegionEndpoint = RegionEndpoint.SAEast1,
                    Profile = new Profile(Keys.AWSProfile)
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
