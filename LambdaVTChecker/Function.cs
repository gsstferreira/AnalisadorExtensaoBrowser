using Amazon.Lambda.Core;
using Common.ClassesDB;
using Common.ClassesLambda;
using Common.Handlers;
using Res;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaVTChecker;

public class Function
{
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="payload">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public string FunctionHandler(LambdaAnalysisPayload payload, ILambdaContext context)
    {
        var extension = ExtensionDownloadhandler.GetExtension(payload.ExtensionPageUrl, Common.Enums.DownloadType.OnlyCrxFile);

        VirusTotalHandler.UploadFileToVT(extension);

        var vtCheckResult = new ExtensionVTResult(extension, payload.AnalysisId);

        DynamoDBHandler.PutEntry(DBTables.VirusTotal, vtCheckResult);
        return string.Empty;
    }
}
