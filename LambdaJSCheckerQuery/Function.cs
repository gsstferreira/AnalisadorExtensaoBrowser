using Amazon.DynamoDBv2.Model;
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
    private const int _files_before_new_call = 50;

    public static string FunctionHandler(JsQueryLambdaPayload input)
    {
        var extension = ExtensionDownloadhandler.GetExtension(input.ExtensionPageUrl, ExtDownloadType.OnlyCrxFile);

        var jsFiles = JavaScriptCheckHandlerNew.GetJSFiles(extension);

        if (input.StartAt == 0)
        {
            var initialCount = new ExtensionJSResult(jsFiles, input.AnalysisId);

            DynamoDBHandler.PutEntry(DBTables.JSFiles, initialCount);
        }
        else
        {
            jsFiles.RemoveRange(0, input.StartAt);
        }

        if (jsFiles.Count > _files_before_new_call)
        {
            var jsFilesSection = jsFiles.Take(_files_before_new_call).ToList();
            var newStartIndex = input.StartAt + _files_before_new_call;

            var newPayload = new JsQueryLambdaPayload(input.ExtensionPageUrl, input.AnalysisId, newStartIndex);
            var json = JsonSerializer.Serialize(newPayload, JsQueryLambdaSG.Default.JsQueryLambdaPayload);
            foreach (var jsFile in jsFilesSection)
            {
                CheckPackages(jsFile, input.AnalysisId);
            }
            Console.WriteLine("{0} files remaining, invoking function again to avoid timeout", jsFiles.Count - _files_before_new_call);
            var newLambda = LambdaHandler.CallFunction(Lambda.JS_Query, json, true);
            newLambda.Wait();
        }
        else
        {
            foreach (var jsFile in jsFiles)
            {
                CheckPackages(jsFile, input.AnalysisId);
            }
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
