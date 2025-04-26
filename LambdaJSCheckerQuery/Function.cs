using Amazon.Lambda.Core;
using Common.Classes;
using Common.ClassesDB;
using Common.ClassesLambda;
using Common.Enums;
using Common.Handlers;
using Common.JsonSourceGenerators;
using Res;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaJSCheckerQuery;

public class Function
{
    private const int _sleep_npm_queries = 1000;
    private const int _files_before_new_call = 100;

    public static string FunctionHandler(JsQueryLambdaPayload input)
    {
        var extension = ExtensionDownloadhandler.GetExtension(input.ExtensionPageUrl, DownloadType.OnlyCrxFile);

        var jsFiles = JavaScriptCheckHandlerNew.GetJSFiles(extension);
        var count = jsFiles.Count;
        Console.WriteLine("{0} arquivos JavaScript encontrados na extensão.", count);
        if (input.StartAt == 0)
        {
            DynamoDBHandler.PutEntry(DBTables.JSFiles, new ExtJSResult(jsFiles, input.AnalysisId));
        }
        else
        {
            jsFiles = [.. jsFiles.Skip(input.StartAt)];
        }
        int analyzed = 0;

        if (jsFiles.Count > _files_before_new_call)
        {
            var jsFilesSection = jsFiles.Take(_files_before_new_call).ToList();
            var newStartIndex = input.StartAt + _files_before_new_call;

            var newPayload = new JsQueryLambdaPayload(input.ExtensionPageUrl, input.AnalysisId, newStartIndex);
            var json = JsonSerializer.Serialize(newPayload, JsQueryLambdaSG.Default.JsQueryLambdaPayload);
            Console.WriteLine("Analisando arquivos [{0}] a [{1}] de [{2}]", input.StartAt, newStartIndex, count);
            foreach (var jsFile in jsFilesSection)
            {
                CheckPackages(jsFile, input.AnalysisId);
                analyzed++;
            }
            Console.WriteLine("{0} arquivos enviados para análise", analyzed);
            Console.WriteLine("{0} arquivos restantes, invocando nova execução.", jsFiles.Count - _files_before_new_call);
            var newLambda = LambdaHandler.CallFunction(Lambda.JS_Query, json, true).Result;
            Console.WriteLine("Resultado da invocação: {0} {1}",newLambda.StatusCode, newLambda.HttpStatusCode);
        }
        else
        {
            Console.WriteLine("Analisando arquivos [{0}] a [{1}] de [{2}]", input.StartAt, count - input.StartAt, count);
            foreach (var jsFile in jsFiles)
            {
                CheckPackages(jsFile, input.AnalysisId);
                analyzed++;
            }
            Console.WriteLine("{0} arquivos enviados para análise", analyzed);
        }
        return string.Empty;
    }

    private static void CheckPackages(JSFile file, string analysisId)
    {
        var elapsed = JavaScriptCheckHandlerNew.GetPotentialNPMPackages(file);
        var lambdaFile = new JSFileLambdaPayload(file, analysisId);
        var jsPayload = JsonSerializer.Serialize(lambdaFile, JsFileLambdaSG.Default.JSFileLambdaPayload);
        LambdaHandler.CallFunction(Lambda.JS_Package, jsPayload, false);

        var timeToWait = _sleep_npm_queries - (int)elapsed;

        if (timeToWait > 0)
        {
            Thread.Sleep(timeToWait);
        }
    }
}
