using Common.Enums;
using System.IO.Compression;
using System.Security;
using System.Text;
using System.Linq;
using Common.Services;
using static System.Net.Mime.MediaTypeNames;
using Common.WebClasses.VirusTotal;

namespace Common.Classes
{
    public class BrowserExtension
    {
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
        public long CrxArchiveLenght { get; set; }
        public string CrxB64 { get; set; }
        public string VirusTotalAnalysisUrl { get; set; }
        internal readonly MemoryStream CrxStream = new();
        public BrowserExtension(string pageUrl, string downloadUrl, string name, string id) 
        { 
            PageUrl = pageUrl;
            DownloadUrl = downloadUrl;
            SimpleName = name;
            ID = id;
            Name = string.Empty;
            Version = string.Empty;
            Provider = string.Empty;
            CrxB64 = string.Empty;
            VirusTotalAnalysisUrl = string.Empty;
            CrxArchiveLenght = 0;

            LastUpdated = DateTime.MinValue;
            Permissions = [];
            ContainedURLs = [];
            ContainedJSFiles = [];
        }

        public void SetCrxArchive(Stream stream)
        {
            stream.CopyTo(CrxStream);
            CrxStream.Seek(0, SeekOrigin.Begin);

            var len = (int)CrxStream.Length;
            CrxArchiveLenght += len;
            byte[] data = new byte[len];
            CrxStream.Read(data, 0, len);
            CrxB64 = Convert.ToBase64String(data);

            CrxStream.Seek(0, SeekOrigin.Begin);
            CrxArchive = new ZipArchive(CrxStream);
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
        public void PrintBasicInfo()
        {
            Console.WriteLine("Informações básicas da extensão:");
            Console.WriteLine(" *Nome: " + this.SimpleName);
            Console.WriteLine(" *ID: " + this.ID);

            var crxSizeMB = ((float)CrxArchiveLenght) / (1024 * 1024);

            Console.WriteLine(string.Format(" *Tamanho comprimido (.crx): {0:0.00}MB", crxSizeMB));
            long size = 0;
            var numEntries = 0;
            foreach (var entry in CrxArchive.Entries.Where(e => !e.Name.Equals(string.Empty))) 
            {
                numEntries++;
                size += entry.Length;
            }
            var sizeMB = ((float)size) / (1024*1024);

            Console.WriteLine(string.Format(" *Tamanho: total de {0:0.00}MB em {1} arquivos", sizeMB, numEntries));
        }
        public void PrintScrapedInfo()
        {
            Console.WriteLine("Informações da extensão:");
            Console.WriteLine(" *Nome: " + this.Name);
            Console.WriteLine(string.Format(" *Nota: {0:0.0}/5.0",this.Rating));
            Console.WriteLine(" *Qtd. Avaliaçãoes: " + this.NumReviews);
            Console.WriteLine(" *Qtd. Downloads: " + this.NumDownloads);
            Console.WriteLine(" *Versão: " + this.Version);
            Console.WriteLine(" *Última Atualização: " + this.LastUpdated.ToShortDateString());
            Console.WriteLine(" *Fornecedor: " + this.Provider);
        }
        public void PrintPermssions()
        {
            Console.WriteLine("\tPermissões obrigatórias:");
            foreach (var permission in Permissions.Where(p => p.Type.Equals(PermissionType.Permission)))
            {
                Console.WriteLine("\t\t" + permission.Name);
            }

            Console.WriteLine("\tPermissões opcionais:");
            foreach (var permission in Permissions.Where(p => p.Type.Equals(PermissionType.OptionalPermission)))
            {
                Console.WriteLine("\t\t" + permission.Name);
            }

            Console.WriteLine("\thosts obrigatórios:");
            foreach (var permission in Permissions.Where(p => p.Type.Equals(PermissionType.Host)))
            {
                Console.WriteLine("\t\t" + permission.Name);
            }

            Console.WriteLine("\thosts opcionais:");
            foreach (var permission in Permissions.Where(p => p.Type.Equals(PermissionType.OptionalHost)))
            {
                Console.WriteLine("\t\t" + permission.Name);
            }
        }
        public void PrintURLs()
        {
            Console.WriteLine("Endereços Web encontrados na extensão:");
            foreach(var url in ContainedURLs)
            {
                Console.WriteLine(string.Format("{0}\r\t\t\t\t\t\t\t\t|{1}",url.Path, url.ThreatType.ToString()));
            }
        }
        public void PrintVTResult()
        {
            Console.WriteLine("Resultados do scan do VirusTotal:");
            Console.WriteLine(string.Format("\tMotores de anti-malware consultados:\r\t\t\t\t\t\t{0:00}", VirusTotalResult.ScanResults.Count));
            Console.WriteLine(string.Format("\t*Sem detecção:\r\t\t\t\t\t\t{0:00}", VirusTotalResult.Statistics.NumUndetectd));
            Console.WriteLine(string.Format("\t*Inofensivo:\r\t\t\t\t\t\t{0:00}", VirusTotalResult.Statistics.NumHarmless));
            Console.WriteLine(string.Format("\t*Suspeito:\r\t\t\t\t\t\t{0:00}", VirusTotalResult.Statistics.NumSuspicious));
            Console.WriteLine(string.Format("\t*Malicioso:\r\t\t\t\t\t\t{0:00}", VirusTotalResult.Statistics.NumMalicious));
            Console.WriteLine(string.Format("\t*Falha no scan:\r\t\t\t\t\t\t{0:00}", VirusTotalResult.Statistics.NumFailure));
            Console.WriteLine(string.Format("\t*Timeout no scan:\r\t\t\t\t\t\t{0:00}", VirusTotalResult.Statistics.NumTimeout));
            Console.WriteLine(string.Format("\t*Sem suporte ao arquivo enviado:\r\t\t\t\t\t\t{0:00}", VirusTotalResult.Statistics.NumUnsupported));
        }

    }
}


