using Common.Enums;
using System.IO.Compression;
using Common.ClassesWeb.VirusTotal;

namespace Common.Classes
{
    public class BrowserExtension
    {
        private readonly MemoryStream CrxStream = new();

        public string PageUrl { get; set; }
        public string DownloadUrl { get; set; }
        public string SimpleName { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Provider { get; set; }
        public float Rating { get; set; }
        public int NumReviews { get; set; }
        public long NumDownloads { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<Permission> Permissions { get; set; }
        public List<Url> ContainedURLs { get; set; }
        public List<JSFile> ContainedJSFiles { get; set; }
        public VTResponse VirusTotalResult { get; set; }
        public ZipArchive CrxArchive { get; set; }
        public string VirusTotalAnalysisUrl { get; set; }

        public BrowserExtension(string pageUrl, string downloadUrl, string name, string id) 
        { 
            PageUrl = pageUrl;
            DownloadUrl = downloadUrl;
            SimpleName = name;
            ID = id;
            Name = string.Empty;
            Version = string.Empty;
            Provider = string.Empty;
            VirusTotalAnalysisUrl = string.Empty;

            LastUpdated = DateTime.MinValue;
            Permissions = [];
            ContainedURLs = [];
            ContainedJSFiles = [];

            VirusTotalResult = new VTResponse();
            CrxArchive = new ZipArchive(new MemoryStream(0), ZipArchiveMode.Create);
        }

        public void SetCrxArchive(Stream stream)
        {
            stream.CopyTo(CrxStream);
            CrxStream.Seek(0, SeekOrigin.Begin);
            CrxArchive = new ZipArchive(CrxStream);
        }

        public string GetCrxAsB64()
        {
            CrxStream.Seek(0, SeekOrigin.Begin);
            var len = (int) CrxStream.Length;
            byte[] data = new byte[len];
            CrxStream.Read(data, 0, len);
            return Convert.ToBase64String(data);
        }

        public long GetCrxSize()
        {
            return CrxStream.Length;
        }

        public void ExtractToPath(string path)
        {
            string simpleName = this.PageUrl.Split('/')[4];
            try
            {
                if (path.EndsWith("/"))
                {
                    this.CrxArchive.ExtractToDirectory(path + simpleName);
                }
                else
                {
                    this.CrxArchive.ExtractToDirectory(path + "/" + simpleName);
                }
                
            }
            catch (IOException)
            {
                Console.WriteLine("Extensão já foi extraída no local!");
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }   
        }
    }
}


