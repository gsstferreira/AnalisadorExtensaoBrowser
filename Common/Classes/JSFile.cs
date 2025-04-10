using Common.Services;
using System.Collections;
using System.IO.Compression;
using System.Text;

namespace Common.Classes
{
    public class JSFile
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Content { get; set; }
        public long Lenght { get; set; }
        public long LenChecksum { get; set; }
        public List<NPMRegistry> NPMRegistries { get; set; }
        public NPMRegistry? BestMatchedRegistry { get; set; }
        public int TotalFilesChecked { get; set; }
        public IDictionary<string, short> Profile { get; set; }
        public double Norm { get; set; }
        public JSFile(ZipArchiveEntry entry) 
        {
            Name = entry.Name.Replace(".js",string.Empty).Trim();
            Path = entry.FullName.Replace(entry.Name, string.Empty).Trim();

            NPMRegistries = [];
            Profile = new Dictionary<string,short>();
            LenChecksum = entry.Crc32 + Lenght;
            using (var reader = new StreamReader(entry.Open()))
            {
                Content = reader.ReadToEnd().Trim();
            }
            Lenght = Content.Length;
            CosineSimilarityService.SetCosineProfile(this);
            BestMatchedRegistry = null;
            TotalFilesChecked = 0;
        }

        public void DisposeRegistriesList()
        {
            NPMRegistries?.Clear();
        }
        public void DisposeProfile()
        {
            Profile?.Clear();
        }

        public string GetFullName()
        {
            return Path + Name;
        }

        public override string ToString()
        {
            if (BestMatchedRegistry != null)
            {
                if(BestMatchedRegistry.MostSimilarPackage != null)
                {
                    var package = BestMatchedRegistry.MostSimilarPackage;
                    var sBuilder = new StringBuilder();

                    sBuilder.Append(string.Format("{0} | ", GetFullName()));

                    var similarity = package.BestSimilarity;
                    sBuilder.Append(string.Format("({0:0.00}%) similar com ", similarity * 100));
                    sBuilder.Append(string.Format("\"{0}\" na versão {1}",package.Name, package.Version));
                    sBuilder.Append(string.Format(" - Última versão: {0}", BestMatchedRegistry.LatestVersion));
                    sBuilder.Append(string.Format(" - {0} arquivos analisados", TotalFilesChecked));
                    return sBuilder.ToString();
                }
            }
            return string.Format("{0} | Sem correspondências - {1} arquivos analisados", GetFullName(),TotalFilesChecked);
        }
    }
}
