using System.IO.Compression;
using Common.ClassesWeb.VirusTotal;
using System.Security.Cryptography;

namespace Common.Classes
{
    public class BrowserExtension
    {
        private MemoryStream? CrxFileStream;
        private MemoryStream? ZipFileStream;
        public string PageUrl { get; set; }
        public string IconUrl { get; set; }
        public string DownloadUrl { get; set; }
        public string SimpleName { get; set; }
        public string Id { get; set; }
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
        public ZipArchive ExtensionContent { get; set; }
        public string VirusTotalAnalysisUrl { get; set; }

        public BrowserExtension()
        {
            PageUrl = string.Empty;
            IconUrl = string.Empty;
            DownloadUrl = string.Empty;
            SimpleName = string.Empty;
            Id = string.Empty;
            Name = string.Empty;
            Version = string.Empty;
            Provider = string.Empty;
            VirusTotalAnalysisUrl = string.Empty;
            LastUpdated = DateTime.MinValue;
            ContainedJSFiles = [];
            ContainedURLs = [];
            Permissions = [];
            VirusTotalResult = new VTResponse();

            ExtensionContent = new ZipArchive(new MemoryStream(0), ZipArchiveMode.Create);
            Rating = -1;
            NumReviews = -1;
            NumDownloads = -1;
        }
        public BrowserExtension(string pageUrl, string downloadUrl, string name, string id)
        {
            PageUrl = pageUrl;
            IconUrl = string.Empty;
            DownloadUrl = downloadUrl;
            SimpleName = name;
            Id = id;
            Name = string.Empty;
            Version = string.Empty;
            Provider = string.Empty;
            VirusTotalAnalysisUrl = string.Empty;

            LastUpdated = DateTime.MinValue;
            Permissions = [];
            ContainedURLs = [];
            ContainedJSFiles = [];

            VirusTotalResult = new VTResponse();
            ExtensionContent = new ZipArchive(new MemoryStream(0), ZipArchiveMode.Create);
        }

        public void SetCrxFile(MemoryStream streamCrx, MemoryStream streamZip)
        {
            CrxFileStream = streamCrx;
            ZipFileStream = streamZip;
            streamZip.Seek(0, SeekOrigin.Begin);
            ExtensionContent = new ZipArchive(streamZip, ZipArchiveMode.Read);
        }

        public string GetCrxAsB64()
        {
            if (CrxFileStream is null) return string.Empty;
            else
            {
                CrxFileStream.Seek(0, SeekOrigin.Begin);

                var cryptoStream = new CryptoStream(CrxFileStream, new ToBase64Transform(), CryptoStreamMode.Read);
                var reader = new StreamReader(cryptoStream);

                string content = reader.ReadToEnd();
                CrxFileStream.Seek(0, SeekOrigin.Begin);

                return content;
            }
        }

        public byte[] GetZipAsBuffer()
        {
            if (ZipFileStream is null) return [];
            else
            {
                var result = ZipFileStream.ToArray();
                ZipFileStream.Seek(0, SeekOrigin.Begin);
                return result;
            }
        }

        public long GetCrxSize()
        {
            return CrxFileStream is not null ? CrxFileStream.Length : 0;
        }

        public long GetZipSize()
        {
            return ZipFileStream is not null ? ZipFileStream.Length : 0;
        }

        public void WriteZipToPath(string path)
        {
            if (ZipFileStream is null) Console.WriteLine("No file to write!");
            else
            {
                string simpleName = PageUrl.Split('/')[4];
                ZipFileStream.Seek(0, SeekOrigin.Begin);
                try
                {
                    if (path.EndsWith('/'))
                    {
                        using var writer = File.Create(path + simpleName + ".zip");
                        var buffer = ZipFileStream.ToArray();
                        writer.Write(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        using var writer = File.Create(path + "/" + simpleName + ".zip");
                        var buffer = ZipFileStream.ToArray();
                        writer.Write(buffer, 0, buffer.Length);
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
                finally
                {
                    ZipFileStream.Seek(0, SeekOrigin.Begin);
                }
            }
        }

        public void DisposeArchives()
        {
            ZipFileStream?.Dispose();
            CrxFileStream?.Dispose();
        }
    }
}


