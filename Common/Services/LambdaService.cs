using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Runtime;

namespace Common.Services
{
    public class LambdaService
    {
        private static readonly AmazonLambdaClient lambdaClient = new AmazonLambdaClient(new AmazonLambdaConfig 
        {
            RegionEndpoint = RegionEndpoint.SAEast1,
            Profile = new Profile("BrowserExtensionAnalysis")
        });
        public static InvokeResponse CallFunction(string functionName, string payload)
        {
            var call = lambdaClient.InvokeAsync(new InvokeRequest
            {
                FunctionName = functionName,
                Payload = payload,
                InvocationType = "Event",
            });
            call.Wait();

            return call.Result;
        }
    }
}
