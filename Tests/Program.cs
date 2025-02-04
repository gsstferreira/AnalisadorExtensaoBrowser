// Obter extensão

using Common.Enums;
using Common.Services;
using F23.StringSimilarity;
using Microsoft.Win32;
using System.Net.Http;
using System.Runtime.Versioning;

internal class Program
{
    private static void Main(string[] args)
    {
        string url_test = "https://chromewebstore.google.com/detail/adobe-acrobat-ferramentas/efaidnbmnnnibpcajpcglclefindmkaj?hl=pt-br";

        Console.WriteLine("URL: " + url_test);
        var ext = ExtensionDownloadService.GetExtension(url_test, Common.Enums.ExtDownloadType.Full);
        //PermissionCheckService.ParsePermissions(ext);
        //ext.PrintBasicInfo();
        //ext.ExtractToPath("C:/XExtensionDL");
        //ext.PrintScrapedInfo();

        Console.WriteLine("Enviando .zip da extensão à análise do VirusTotal...");
        VirusTotalService.UploadFileToVT(ext);
        Console.WriteLine("Esperando 30 segundos antes de acessar os resultados...");
        Thread.Sleep(30000);
        Console.WriteLine("Acessando a URL de resultados...");
        Console.WriteLine("URl do resultado: " + ext.VirusTotalAnalysisUrl);
        VirusTotalService.CheckVTAnalysis(ext);
        ext.PrintVTResult();
        //UrlCheckService.CheckURLs(ext);
        //ext.PrintURLs();
        //JavaScriptContentCheckService.CheckJSFiles(ext);
    }
}