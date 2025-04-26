using Amazon.Lambda.Core;
using Common.ClassesDB;
using Common.ClassesLambda;
using Common.Handlers;
using Res;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaURLChecker;

public class Function
{
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="payload">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public static string FunctionHandler(LambdaAnalysisPayload payload)
    {
        var extension = ExtensionDownloadhandler.GetExtension(payload.ExtensionPageUrl,Common.Enums.DownloadType.OnlyCrxFile);

        UrlCheckHandler.CheckURLs(extension);

        var checkedUrls = new ExtURLsResult(extension, payload.AnalysisId);

        DynamoDBHandler.PutEntry(DBTables.URLs, checkedUrls);
        return string.Empty;
    }
}
