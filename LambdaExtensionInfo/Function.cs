using Amazon.Lambda.Core;
using Common.ClassesDB;
using Common.ClassesLambda;
using Common.Services;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaExtensionInfo;

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

        var extension = ExtensionDownloadService.GetExtension(input.ExtensionPageUrl, Common.Enums.ExtDownloadType.OnlyScrape);

        var extensionInfoResult = new ExtensionInfoResult(extension);

        DynamoDBService.SaveItemToDB(Common.Res.DBTables.ExtensionInfo, extensionInfoResult);
        response.SetSuccess(true, "extension info scrapped.");
        return JsonSerializer.Serialize(response);
    }
}
