using Amazon.Lambda.Core;
using Common.ClassesDB;
using Common.ClassesLambda;
using Common.Handlers;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaURLChecker;

public class Function
{
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public string FunctionHandler(LambdaRequestBody input, ILambdaContext context)
    {
        var response = new LambdaResponseBody();

        var extension = ExtensionDownloadhandler.GetExtension(input.ExtensionPageUrl,Common.Enums.ExtDownloadType.OnlyCrxFile);

        UrlCheckHandler.CheckURLs(extension);
        extension.Version = input.ExtensionVersion;

        var checkedUrls = new ExtensionURLsResult(extension);

        DynamoDBHandler.UpdateEntry(Common.Res.DBTables.URLs, checkedUrls);
        response.SetSuccess(true, "URLs checked!");
        return JsonSerializer.Serialize(response);
    }
}
