using Amazon.Lambda.Core;
using Common.ClassesDB;
using Common.ClassesLambda;
using Common.Handlers;
using Res;
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
    public static string FunctionHandler(LambdaAnalysisPayload payload)
    {
        {
            var extension = ExtensionDownloadhandler.GetExtension(payload.ExtensionPageUrl, Common.Enums.DownloadType.OnlyCrxFile);

            PermissionCheckHandler.ParsePermissions(extension);

            var permissionResult = new ExtensionPermissionsResult(extension, payload.AnalysisId);

            DynamoDBHandler.PutEntry(DBTables.Permissions, permissionResult);
            return string.Empty;
        }
    }
}
