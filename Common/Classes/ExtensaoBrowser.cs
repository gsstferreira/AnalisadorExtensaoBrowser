using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Common.Classes
{
    public class ExtensaoBrowser
    {
        public required string Url { get; set; }
        public required string UrlDownload { get; set; }
        public required Stream ArquivoCrx { get; set; }

        private ExtensaoBrowser() { }

        private static Stream BaixarExtensao(string urlDownload)
        {
            using (var client = new HttpClient())
            {
                // Obter arquivo .crx do repositório Google
                var response = client.GetStreamAsync(urlDownload);
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

                return stream;
            }
        }

        public static ExtensaoBrowser ObterExtensao(string url)
        {
            Console.WriteLine("Obtendo Extensão...");

            var urlTrim = url.Replace(Constantes.ChromeExtensionViewUrl, string.Empty).Trim();
            var urlSplit = urlTrim.Split(['/', '?']);
            var nomeExtensao = urlSplit[0];
            var IdExtensao = urlSplit[1];

            var urlDownload = Constantes.ChromeExtensionDownloadUrl.
                Replace("[PRODVER]", Constantes.ChromeProdVersion).
                Replace("[FORMAT]", Constantes.AcceptedFormat).
                Replace("[ID]", IdExtensao);

            var arquivoCrx = BaixarExtensao(urlDownload);

            var extensao = new ExtensaoBrowser()
            {
                Url = url,
                UrlDownload = urlDownload,
                ArquivoCrx = arquivoCrx,
            };


            Console.WriteLine("Extensão obtida!");
            extensao.SalvarArquivo("C:\\XExtensionDL\\" + nomeExtensao + ".zip");
            extensao.ListarConteudo("C:\\XExtensionDL\\" + nomeExtensao + ".zip");


            return extensao;
        }

        public void SalvarArquivo(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            {
                ArquivoCrx.CopyTo(fs);
            }
        }

        public void ListarConteudo(string path)
        {
            var zip = ZipFile.OpenRead(path);

            foreach (var item in zip.Entries)
            {
                Console.WriteLine(item.Name + " - " + item.Length);
            }
        }
    }
}


