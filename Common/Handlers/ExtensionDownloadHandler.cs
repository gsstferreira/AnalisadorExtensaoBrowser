using Common.Classes;
using Common.Enums;
using HtmlAgilityPack;
using Res;
using System.Globalization;
using System.Text;

namespace Common.Handlers
{
    public class ExtensionDownloadhandler
    {
        private static readonly HttpClient _httpClient = new();
        private static readonly CultureInfo _culture = new("pt-BR");

        public static bool IsThisUrlExtension(string url)
        {
            if (string.IsNullOrEmpty(url)) return false;
            else
            {
                var urlParts = url.Replace(Params.ViewURL, string.Empty).Trim().Split(['/', '?']);
                var extensionId = urlParts[1];

                var downloadUrl = Params.DownloadURL.
                    Replace("[PRODVER]", Params.ChromeProdVersion).
                    Replace("[FORMAT]", Params.AcceptedFormat).
                    Replace("[ID]", extensionId);

                try
                {
                    var response = _httpClient.Send(new HttpRequestMessage(HttpMethod.Head, downloadUrl));

                    if (response is not null)
                    {
                        Console.WriteLine(response.StatusCode);
                        return response.IsSuccessStatusCode;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return false;
        }
        public static BrowserExtension GetExtension(string url, DownloadType downloadType)
        {
            var urlParts = url.Replace(Params.ViewURL, string.Empty).Trim().Split(['/', '?']);
            var simpleName = urlParts[0];
            var extensionId = urlParts[1];

            var downloadUrl = Params.DownloadURL.
                Replace("[PRODVER]", Params.ChromeProdVersion).
                Replace("[FORMAT]", Params.AcceptedFormat).
                Replace("[ID]", extensionId);

            var extension = new BrowserExtension(url, downloadUrl, simpleName, extensionId);

            switch (downloadType)
            {
                case DownloadType.OnlyCrxFile:
                    DownloadCrx(extension);
                    break;
                case DownloadType.OnlyScrape:
                    ScrapeExtensionInfo(extension);
                    break;
                default:
                    DownloadCrx(extension);
                    ScrapeExtensionInfo(extension);
                    break;
            }

            return extension;
        }
        private static void DownloadCrx(BrowserExtension extension)
        {
            MemoryStream crxStream = new();
            MemoryStream zipStream = new();
            // Obter arquivo .crx do repositório Google
            using (var response = _httpClient.GetStreamAsync(extension.DownloadUrl).Result)
            {
                response.CopyTo(crxStream);
                crxStream.Seek(0, SeekOrigin.Begin);
            }

            // Remover cabeçalho adicional para converter .crx em .zip
            byte[] crNum = new byte[4];     // "Magic Number", string de valor "Cr24"
            byte[] crxVer = new byte[4];    // versão do .crx
            byte[] hLen = new byte[4];      // tamanho do restante do cabeçalho adicional

            crxStream.Read(crNum, 0, crNum.Length);     // extração do "Magic Number"
            crxStream.Read(crxVer, 0, crxVer.Length);   // extração da versão do .crx
            crxStream.Read(hLen, 0, hLen.Length);       // extração do tamanho do cabeçalho

            //string magicNumber = Encoding.UTF8.GetString(crNum);
            //int version = (int)BitConverter.ToUInt32(crxVer, 0);        // byte[] -> uint -> int numericamente equivalente
            int headerLength = (int)BitConverter.ToUInt32(hLen, 0);     // byte[] -> uint -> int numericamente equivalente

            crxStream.Read(new byte[headerLength], 0, headerLength);    // Remoção do cabeçalho, deixando apenas o .zip no stream

            crxStream.CopyTo(zipStream);
               
            extension.SetCrxFile(crxStream, zipStream);
        }
        private static void ScrapeExtensionInfo(BrowserExtension extension)
        {
            // Obter página HTML da extensão para scrapping
            var scrapper = new HtmlWeb();

            var culturePage = extension.PageUrl.Split("?")[0] + "?hl=pt-br";

            var page = scrapper.Load(culturePage).DocumentNode;

            extension.Name = TrySelectSingleNode(page, SearchStrs.xPath_Name);
            extension.Provider = TrySelectSingleNode(page, SearchStrs.xPath_Provider);

            if (float.TryParse(TrySelectSingleNode(page, SearchStrs.xPath_Rating), out var rating)) extension.Rating = rating;
            else extension.Rating = 0;

            var divAvalicaoes = TrySelectSingleNode(page, SearchStrs.xPath_Reviews).Split();

            if (!float.TryParse(divAvalicaoes[0], out var score)) score = -1;
            else
            {
                if (divAvalicaoes.Contains("mil")) score *= 1000;
            }
            extension.NumReviews = (int)score;
            extension.Version = TrySelectSingleNode(page, SearchStrs.xPath_Version);

            try
            {
                var divInfoUpdate = page.SelectSingleNode(SearchStrs.xPath_LastUpdate);

                if (divInfoUpdate is null) extension.LastUpdated = DateTime.MinValue;
                else
                {
                    var node = divInfoUpdate.ChildNodes[1].InnerText;
                    extension.LastUpdated = DateTime.Parse(node, _culture);
                }
            }
            catch { extension.LastUpdated = DateTime.MinValue; }

            try
            {
                var divInfoDl = page.SelectSingleNode(SearchStrs.xPath_Downloads);

                if (divInfoDl is null) extension.NumDownloads = -1;
                else
                {
                    var node = divInfoDl.ChildNodes[2].InnerText;
                    extension.NumDownloads = int.Parse(node.Split()[0].Replace(".", string.Empty));
                }
            }
            catch { extension.NumDownloads = -1; }

            try
            {
                var divImgIcon = page.SelectSingleNode(SearchStrs.xPath_icon);

                if (divImgIcon is null) extension.IconUrl = string.Empty;
                else
                {
                    var node = divImgIcon.GetAttributeValue("src", string.Empty);
                    extension.IconUrl = node.Split('=')[0] + "=s500";
                }
            }
            catch { extension.IconUrl = string.Empty; }
        }
        private static string TrySelectSingleNode(HtmlNode node, string xPath)
        {
            try
            {
                var element = node.SelectSingleNode(xPath);

                if (element is null) return string.Empty;
                else return element.InnerText;
            }
            catch { return string.Empty; }
        }
    }
}
