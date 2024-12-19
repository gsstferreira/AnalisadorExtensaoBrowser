using Common.Constantes;
using Common.Enums;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using System.Security;
using System.Text;
using System.Linq;

namespace Common.Classes
{
    public class BrowserExtension
    {
        public string PageUrl { get; set; }
        public string DownloadUrl { get; set; }
        public ZipArchive CrxArchive { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Provider { get; set; }
        public float Rating { get; set; }
        public int NumReviews { get; set; }
        public long NumDownloads { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<Permission> Permissions { get; set; }

        private BrowserExtension(string pageUrl, string downloadUrl) 
        { 
            PageUrl = pageUrl;
            DownloadUrl = downloadUrl;
            Name = string.Empty;
            Version = string.Empty;
            Provider = string.Empty;

            LastUpdated = DateTime.MinValue;
            Permissions = [];
        }

        public static BrowserExtension GetExtension(string extensionUrl)
        {
            var urlTrim = extensionUrl.Replace(ChromeWebStore.ChromeExtensionViewUrl, string.Empty).Trim();
            var extensionId = urlTrim.Split(['/', '?'])[1];

            var downloadUrl = ChromeWebStore.ChromeExtensionDownloadUrl.
                Replace("[PRODVER]", ChromeWebStore.ChromeProdVersion).
                Replace("[FORMAT]", ChromeWebStore.AcceptedFormat).
                Replace("[ID]", extensionId);

            var extension = new BrowserExtension(extensionUrl, downloadUrl);

            Console.WriteLine("Obtendo info de extensão - {0}",DateTime.Now);
            extension.DownloadCrx();
            extension.ScrapeExtensionInfo();
            extension.ParsePermissions();

            Console.WriteLine("Infos obtidas - {0}", DateTime.Now);

            return extension;
        }

        private void DownloadCrx()
        {
            // Obter arquivo .crx do repositório Google
            using (var httpClient = new HttpClient()) 
            {
                var response = httpClient.GetStreamAsync(this.DownloadUrl);
                response.Wait();
                var stream = response.Result;

                // Remover cabeçalho adicional para converter .crx em .zip
                byte[] chromeNumber = new byte[4];  // "Magic Number", string de valor "Cr24"
                byte[] crxVersion = new byte[4];    // versão do .crx
                byte[] headerLenght = new byte[4];  // tamanho do restante do cabeçalho adicional

                stream.Read(chromeNumber, 0, 4);    // extração do "Magic Number"
                stream.Read(crxVersion, 0, 4);      // extração da versão do .crx
                stream.Read(headerLenght, 0, 4);    // extração do tamanho do cabeçalho

                string magicNumber = Encoding.UTF8.GetString(chromeNumber);
                int version = checked((int)BitConverter.ToUInt32(crxVersion, 0));       // byte[] -> uint -> int numericamente equivalente
                int headerLen = checked((int)BitConverter.ToUInt32(headerLenght, 0));   // byte[] -> uint -> int numericamente equivalente

                stream.Read(new byte[headerLen], 0, headerLen);     // Remoção do cabeçalho, deixando apenas o .zip no stream
                this.CrxArchive = new ZipArchive(stream);
            }
        }

        private void ScrapeExtensionInfo()
        {
            // Obter página HTML da extensão para scrapping
            var scrapper = new HtmlAgilityPack.HtmlWeb();
            var page = scrapper.Load(this.PageUrl).DocumentNode;

            this.Name = page.SelectSingleNode(HTMLXpaths.xPath_Name).InnerText;
            this.Provider = page.SelectSingleNode(HTMLXpaths.xPath_Provider).InnerText;
            this.Rating = float.Parse(page.SelectSingleNode(HTMLXpaths.xPath_Rating).InnerText);

            var divAvalicaoes = page.SelectSingleNode(HTMLXpaths.xPath_NumReviews).InnerText.Split();

            var numAvaliacoes = float.Parse(divAvalicaoes[0]);

            if(divAvalicaoes.Contains("mil"))
            {
                numAvaliacoes *= 1000;
            }
            this.NumReviews = (int)numAvaliacoes;
            this.Version = page.SelectSingleNode(HTMLXpaths.xPath_Version).InnerText;
            
            var divAtt = page.SelectSingleNode(HTMLXpaths.xPath_LastUpdated).ChildNodes.ElementAt(1).InnerText;
            this.LastUpdated = DateTime.Parse(divAtt);

            var divNumDl = page.SelectSingleNode(HTMLXpaths.xPath_NumDownloads).ChildNodes.ElementAt(2).InnerText;
            this.NumDownloads = int.Parse(divNumDl.Split()[0].Replace(".",string.Empty));
        }

        private void ParsePermissions()
        {
            var manifest = this.CrxArchive.GetEntry("manifest.json");
            if (manifest != null)
            {
                JObject json;

                using(var reader = new StreamReader(manifest.Open())) 
                {
                    json = JObject.Parse(reader.ReadToEnd());
                }
                
                if(json.TryGetValue("permissions", out var permissions))
                {
                    this.Permissions.AddRange(from string value in permissions.Values<string>()
                                              where !string.IsNullOrEmpty(value)
                                              select new Permission(value.Trim(), PermissionType.Permission));
                }

                if (json.TryGetValue("optional_permissions", out permissions))
                {
                    this.Permissions.AddRange(from string value in permissions.Values<string>()
                                              where !string.IsNullOrEmpty(value)
                                              select new Permission(value.Trim(), PermissionType.OptionalPermission));
                }
                if (json.TryGetValue("host_permissions", out permissions))
                {
                    this.Permissions.AddRange(from string value in permissions.Values<string>()
                                              where !string.IsNullOrEmpty(value)
                                              select new Permission(value.Trim(), PermissionType.Host));
                }
                if (json.TryGetValue("optional_host_permissions", out permissions))
                {
                    this.Permissions.AddRange(from string value in permissions.Values<string>()
                                              where !string.IsNullOrEmpty(value)
                                              select new Permission(value.Trim(), PermissionType.OptionalHost));
                }
            }
        }
        public void ExtrairArquivos(string path)
        {
            try
            {
                this.CrxArchive.ExtractToDirectory(path);
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

        public void ListarConteudo()
        {
            foreach (var item in CrxArchive.Entries)
            {
                Console.WriteLine(item.FullName);
            }
        }
    }
}


