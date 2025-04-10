﻿using Common.ClassesLambda;
using Common.Services;
using System.Diagnostics;
using System.Text.Json;

internal class Program
{
    private static void Main(string[] args)
    {
        string UrlTest = "https://chromewebstore.google.com/detail/adobe-acrobat-ferramentas/efaidnbmnnnibpcajpcglclefindmkaj?hl=pt-br";

        Console.WriteLine("URL: " + UrlTest);
        var ext = ExtensionDownloadService.GetExtension(UrlTest, Common.Enums.ExtDownloadType.Full);

        var requestbody = new LambdaRequestBody(ext);
        var json = JsonSerializer.Serialize(requestbody);

        //var respInfo = LambdaService.CallFunction("ExtensionAnalysis_ExtensionInfo", json);
        //Console.WriteLine("Web scrapping queued!");
        //var respPermission = LambdaService.CallFunction("ExtensionAnalysis_Permissions", json);
        //Console.WriteLine("Permissions parsing queued!");
        var respURL = LambdaService.CallFunction("ExtensionAnalysis_URL", json);
        Console.WriteLine("URLs checking queued!");
        //var respVirusTotal = LambdaService.CallFunction("ExtensionAnalysis_VirusTotal", json);
        //Console.WriteLine("Virus Total analysis queued!");
        //var respJS = LambdaService.CallFunction("ExtensionAnalysis_JSFiles", json);
        //Console.WriteLine(".js files checking queued!");

    }
}