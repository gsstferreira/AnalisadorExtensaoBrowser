using Common.Handlers;
using System.Diagnostics;

internal class Program
{
    private static void Main()
    {
        //Console.WriteLine(Environment.Version.ToString());
        //string UrlTest = "https://chromewebstore.google.com/detail/ultimate-car-driving-game/aomkpefnllinimbhddlfhelelngakbbn";
        //Console.WriteLine("URL: " + UrlTest);
        //var ext = ExtensionDownloadhandler.GetExtension(UrlTest, Common.Enums.DownloadType.Full);

        //ext.WriteZipToPath("C:/XExtensionDL");

        ////var fileStream = File.OpenRead("C:/XExtensionDL/sidebar-chatgpt-favoritos.zip");

        ////var memStream = new MemoryStream();

        ////fileStream.CopyTo(memStream);

        ////ext.SetCrxFile(new MemoryStream(), memStream);

        //VirusTotalHandler.UploadFileToVTRestSharp(ext);

        //var response = VirusTotalHandler.CheckVTAnalysis("https://www.virustotal.com/api/v3/analyses/MjZiM2I0OWI3Nzk4MGExNjdiNTkyY2EwNTcyMzA3ZWU6MTc0NTE5OTUyNw==");
        //Console.WriteLine(response);
        //var list = ext.ContainedJSFiles.OrderByDescending(f => f.Lenght).ToList();

        //var file = list[0];

        //int iter = 50000;
        //var dictionary = new Dictionary<int, int>();
        //var watch = new Stopwatch();
        //watch.Start();

        //for(int i = 0; i < iter; i++)
        //{
        //    dictionary.Clear();
        //    TestSimilarityHandler.GetProfileManualHash(file.Content, dictionary);
        //}
        //watch.Stop();

        //Console.WriteLine("Manual Hash: {0:0.00}s", watch.ElapsedMilliseconds/1000.0);
        //dictionary = [];
        //watch.Restart();
        //for (int i = 0; i < iter; i++)
        //{
        //    dictionary.Clear();
        //    TestSimilarityHandler.GetProfileHashCode(file.Content, dictionary);
        //}
        //watch.Stop();
        //Console.WriteLine("Standard Hash: {0:0.00}s", watch.ElapsedMilliseconds / 1000.0);

        //var dictionary2 = new Dictionary<ulong, int>();
        //watch.Restart();
        //for (int i = 0; i < iter; i++)
        //{
        //    dictionary2.Clear();
        //    TestSimilarityHandler.GetProfileKnut(file.Content, dictionary2);
        //}
        //watch.Stop();
        //Console.WriteLine("Knut: {0:0.00}s", watch.ElapsedMilliseconds / 1000.0);

        //var requestBody = LambdaHandler.CallFunction(Lambda.ScrappingInfo, JsonSerializer.Serialize(UrlTest2), false).Result;

        //using (var reader = new StreamReader(requestBody.Payload))
        //{
        //    var body = reader.ReadToEnd();
        //    var query = LambdaHandler.CallFunction(Lambda.JS_Query, body, true);
        //}
    }
}