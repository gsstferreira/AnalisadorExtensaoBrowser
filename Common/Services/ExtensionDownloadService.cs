using Common.Classes;
using Common.Enums;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Common.Services
{
    public class ExtensionDownloadService
    {
        private static readonly HttpClient _httpClient = new();
        public static BrowserExtension GetExtension(string url, ExtDownloadType downloadType)
        {
            var urlParts = url.Replace(Res.Params.ViewURL, string.Empty).Trim().Split(['/', '?']);
            var simpleName = urlParts[0];
            var extensionId = urlParts[1];

            var downloadUrl = Res.Params.DownloadURL.
                Replace("[PRODVER]", Res.Params.ChromeProdVersion).
                Replace("[FORMAT]", Res.Params.AcceptedFormat).
                Replace("[ID]", extensionId);

            var extension = new BrowserExtension(url, downloadUrl, simpleName, extensionId);

            switch (downloadType)
            {
                case ExtDownloadType.OnlyCrxFile:
                    DownloadCrx(extension);
                    break;
                case ExtDownloadType.OnlyScrape:
                    ScrapeExtensionInfo(extension);
                    break;
                default:
                    DownloadCrx(extension);
                    ScrapeExtensionInfo(extension);
                    break;
            }

            return extension;
        }

        internal static void DownloadCrx(BrowserExtension extension)
        {
            // Obter arquivo .crx do repositório Google
            using (var response = _httpClient.GetStreamAsync(extension.DownloadUrl).Result)
            {
                int crxSize = 0;

                // Remover cabeçalho adicional para converter .crx em .zip
                byte[] chromeNumber = new byte[4];  // "Magic Number", string de valor "Cr24"
                byte[] crxVersion = new byte[4];    // versão do .crx
                byte[] headerLenght = new byte[4];  // tamanho do restante do cabeçalho adicional

                crxSize += response.Read(chromeNumber, 0, 4);    // extração do "Magic Number"
                crxSize += response.Read(crxVersion, 0, 4);      // extração da versão do .crx
                crxSize += response.Read(headerLenght, 0, 4);    // extração do tamanho do cabeçalho

                string magicNumber = Encoding.UTF8.GetString(chromeNumber);
                int version = checked((int)BitConverter.ToUInt32(crxVersion, 0));       // byte[] -> uint -> int numericamente equivalente
                int headerLen = checked((int)BitConverter.ToUInt32(headerLenght, 0));   // byte[] -> uint -> int numericamente equivalente

                crxSize += response.Read(new byte[headerLen], 0, headerLen);     // Remoção do cabeçalho, deixando apenas o .zip no stream
                extension.SetCrxArchive(response);
            }
        }
        internal static void ScrapeExtensionInfo(BrowserExtension extension)
        {
            // Obter página HTML da extensão para scrapping
            var scrapper = new HtmlAgilityPack.HtmlWeb();
            var page = scrapper.Load(extension.PageUrl).DocumentNode;

            extension.Name = TrySelectSingleNode(page, Res.SearchStrs.xPath_Name);
            extension.Provider = TrySelectSingleNode(page, Res.SearchStrs.xPath_Provider);

            if (float.TryParse(TrySelectSingleNode(page, Res.SearchStrs.xPath_Rating), out var rating))
            {
                extension.Rating = rating;
            }
            else
            {
                extension.Rating = 0;
            }

            var divAvalicaoes = TrySelectSingleNode(page, Res.SearchStrs.xPath_Reviews).Split();

            if (!float.TryParse(divAvalicaoes[0], out var score))
            {
                score = -1;
            }
            else
            {
                if (divAvalicaoes.Contains("mil"))
                {
                    score *= 1000;
                }
            }
            extension.NumReviews = (int)score;
            extension.Version = TrySelectSingleNode(page, Res.SearchStrs.xPath_Version);

            try
            {
                var divAtt = page.SelectSingleNode(Res.SearchStrs.xPath_LastUpdate).ChildNodes.ElementAt(1).InnerText;
                extension.LastUpdated = DateTime.Parse(divAtt);
            }
            catch
            {
                extension.LastUpdated = DateTime.MinValue;
            }
            try
            {
                var divNumDl = page.SelectSingleNode(Res.SearchStrs.xPath_Downloads).ChildNodes.ElementAt(2).InnerText;
                extension.NumDownloads = int.Parse(divNumDl.Split()[0].Replace(".", string.Empty));
            }
            catch
            {
                extension.NumDownloads = -1;
            }


        }

        internal static string TrySelectSingleNode(HtmlNode node, string xPath)
        {
            try
            {
                var element = node.SelectSingleNode(xPath);

                return element.InnerText;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
