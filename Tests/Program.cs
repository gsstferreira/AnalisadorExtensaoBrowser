// Obter extensão

using Common.ClassesDB;
using Common.Services;

internal class Program
{
    private static void Main(string[] args)
    {
        string UrlTest = "https://chromewebstore.google.com/detail/adobe-acrobat-ferramentas/efaidnbmnnnibpcajpcglclefindmkaj?hl=pt-br";

        Console.WriteLine("URL: " + UrlTest);
        var ext = ExtensionDownloadService.GetExtension(UrlTest, Common.Enums.ExtDownloadType.Full);

        VirusTotalService.UploadFileToVT(ext);

        var vtDB = new ExtensionVTResult(ext.VirusTotalAnalysisUrl, ext.ID, ext.Version);

        DynamoDBService.SaveItemToDB(Common.Res.DBTables.VirusTotal, vtDB);
    }
}