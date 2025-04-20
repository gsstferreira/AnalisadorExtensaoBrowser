using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Common.Classes;
using Common.ClassesDB;
using Common.ClassesLambda;
using Common.Handlers;
using Res;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaJsCheckerPackage;

public class Function
{
    public static string FunctionHandler(JSFileLambdaPayload payload)
    {
        var analysisId = payload.AnalysisId;

        var jsFile = new JSFile(payload);
        Console.WriteLine("[{0}] - {1} registros para avaliar.", jsFile.Name, jsFile.NPMRegistries.Count);
        JavaScriptCheckHandlerNew.ValidateNPMPackages(jsFile);

        try
        {
            var result = new ExtensionJSFile(jsFile);

            var item = new AttributeValue
            {
                L =
                [
                    new() {
                    M = DynamoDBHandler.MapAsAttributes(result),
                    IsMSet = true,
                }
                ],
                IsLSet = true
            };
            Console.WriteLine("[{0}] - Mapa de atributos para DynamoDB criado.", jsFile.Name);
            var updateRequest = new UpdateItemRequest
            {
                TableName = DBTables.JSFiles,
                Key = new Dictionary<string, AttributeValue> { { "AnalysisID", new AttributeValue(analysisId) } },
                ExpressionAttributeNames = new Dictionary<string, string> { { "#A", "ExtensionJSFiles" } },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                {":jsFile", item }
            },
                UpdateExpression = "SET #A = list_append(#A, :jsFile)"
            };

            DynamoDBHandler.UpdateEntry(updateRequest);
            Console.WriteLine("[{0}] - Item salvo no DynamoDB.", jsFile.Name);
        }
        catch (Exception ex) 
        {
            Console.WriteLine("[{0}] - {1}", jsFile.Name, ex.Message);
        }

        return string.Empty;
    }
}
