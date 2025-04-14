using Amazon.Lambda.Core;
using Common.ClassesDB;
using Common.ClassesLambda;
using Common.Handlers;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaExtensionPermissions;

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
        {
            var response = new LambdaResponseBody();

            var extension = ExtensionDownloadhandler.GetExtension(input.ExtensionPageUrl, Common.Enums.ExtDownloadType.OnlyCrxFile);

            PermissionCheckHandler.ParsePermissions(extension);
            extension.Version = input.ExtensionVersion;

            var permissionResult = new ExtensionPermissionsResult(extension);

            DynamoDBHandler.UpdateEntry(Common.Res.DBTables.Permissions, permissionResult);
            response.SetSuccess(true, "extension permissions parsed.");
            return JsonSerializer.Serialize(response);
        }
    }
}
