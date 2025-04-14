using Common.ClassesLambda;
using Common.Handlers;
using System.Text.Json;

internal class Program
{
    private static void Main(string[] args)
    {
        string UrlTest = "https://chromewebstore.google.com/detail/adobe-acrobat-ferramentas/efaidnbmnnnibpcajpcglclefindmkaj?hl=pt-br";

        Console.WriteLine("URL: " + UrlTest);
        var ext = ExtensionDownloadhandler.GetExtension(UrlTest, Common.Enums.ExtDownloadType.Full);

        var requestbody = new LambdaRequestBody(ext);
        var json = JsonSerializer.Serialize(requestbody);

        //JavaScriptCheckHandler.CheckJSFiles(ext);

        //var listJs = new List<ExtensionJSFile>();

        //foreach (var file in ext.ContainedJSFiles)
        //{
        //    listJs.Add(new ExtensionJSFile(file));
        //}
        var respInfo = LambdaHandler.CallFunction("ExtensionAnalysis_ExtensionInfo", json);
        Console.WriteLine("Web scrapping queued!");
        var respPermission = LambdaHandler.CallFunction("ExtensionAnalysis_Permissions", json);
        Console.WriteLine("Permissions parsing queued!");
        var respURL = LambdaHandler.CallFunction("ExtensionAnalysis_URL", json);
        Console.WriteLine("URLs checking queued!");
        var respVirusTotal = LambdaHandler.CallFunction("ExtensionAnalysis_VirusTotal", json);
        Console.WriteLine("Virus Total analysis queued!");
        var respJS = LambdaHandler.CallFunction("ExtensionAnalysis_JSFiles", json);
        Console.WriteLine(".js files checking queued!");

    }
}