using Amazon.Lambda.Core;
using Common.ClassesDB;
using Common.ClassesLambda;
using Common.Handlers;
using Res;
using System.Text;
using System.Web;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaExtensionInfo;

public class Function
{
    public static LambdaAnalysisPayload FunctionHandler(string extensionUrl)
    {
        var extension = ExtensionDownloadhandler.GetExtension(extensionUrl, Common.Enums.DownloadType.OnlyScrape);

        //extension: entidade representando a extensão de navegador
        string concat = extension.Id + extension.Version;
        string analysisId = Convert.ToBase64String(Encoding.Default.GetBytes(concat));
        //analysisId: string resultante da codificação em base64

        analysisId = HttpUtility.HtmlEncode(analysisId);

        var extensionInfoResult = new ExtInfoResult(extension, analysisId);

        Console.WriteLine(extension.Id + "|" + extension.Version);

        var listTask = new List<Task>
        {
            Task.Factory.StartNew(() => DynamoDBHandler.PutEntry(DBTables.ExtensionInfo, extensionInfoResult)),
            Task.Factory.StartNew(() => DynamoDBHandler.DeleteEntry(DBTables.Permissions, analysisId)),
            Task.Factory.StartNew(() => DynamoDBHandler.DeleteEntry(DBTables.URLs, analysisId)),
            Task.Factory.StartNew(() => DynamoDBHandler.DeleteEntry(DBTables.JSFiles, analysisId)),
            Task.Factory.StartNew(() => DynamoDBHandler.DeleteEntry(DBTables.VirusTotal, analysisId))
        };

        Task.WaitAll([.. listTask]);

        var response = new LambdaAnalysisPayload
        {
            AnalysisId = analysisId,
            ExtensionPageUrl = extensionUrl
        };

        return response;
    }
}
