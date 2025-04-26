using Common.Classes;
using Common.Handlers;
using Res;
using System.Text.RegularExpressions;
using Testing;

internal partial class Program
{
    private static void Main()
    {
        //VirusTotalTest.EicarTest();

        var stringResult = "https://www.virustotal.com/api/v3/analyses/MjZiM2I0OWI3Nzk4MGExNjdiNTkyY2EwNTcyMzA3ZWU6MTc0NTYzNDYyMg==";

        var result = VirusTotalHandler.CheckVTAnalysis(stringResult);

        Console.WriteLine("Url da análise: {0}", stringResult);
        Console.WriteLine("Resultados:");

        Console.WriteLine("Detecções: {0}/{1}", result.Statistics.NumMalicious, result.ScanResults.Count);

        for (int i = 0; i < result.ScanResults.Count; i += 2) 
        {
            var result1 = result.ScanResults[i];
            Console.Write("[{0}]\r\t\t\t{1}", result1.EngineName, result1.Category);

            if (i + 1 < result.ScanResults.Count) 
            {
                var result2 = result.ScanResults[i + 1];
                Console.WriteLine("\r\t\t\t\t\t\t[{0}]\r\t\t\t\t\t\t\t\t\t{1}", result1.EngineName, result1.Category);
            }
        }
        Console.WriteLine();
    }
}