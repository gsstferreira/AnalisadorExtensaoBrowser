using Common.Enums;
using System.IO.Compression;
using System.Text;

namespace Common.Classes
{
    public class JSFile
    {
        public static readonly double PerfectMatchThreshold = 0.9999999;
        public string Name { get; set; }
        public string Path { get; set; }
        public UpdateLevel UpdateLevel { get; set; }
        public string Content { get; set; }
        public long Size { get; set; }
        public long SizeChecksum { get; set; }
        public List<NPMRegistry> NPMRegistries { get; set; }
        public NPMRegistry? BestMatchedRegistry { get; set; }
        public int TotalFilesChecked { get; set; }
        public JSFile(ZipArchiveEntry entry) 
        {
            Name = entry.Name.Replace(".js",string.Empty).Trim();
            Path = entry.FullName.Replace(entry.Name, string.Empty).Trim();

            UpdateLevel = UpdateLevel.UNCHECKED;
            NPMRegistries = [];
            SizeChecksum = entry.Crc32 + Size;
            using (var reader = new StreamReader(entry.Open()))
            {
                Content = reader.ReadToEnd().Trim();
            }
            Size = Content.Length;
            BestMatchedRegistry = null;
            TotalFilesChecked = 0;
        }

        public void DisposeRegistriesList()
        {
            NPMRegistries.Clear();
            NPMRegistries = null;
        }

        public string GetFullName()
        {
            return Path + Name;
        }

        public override string ToString()
        {
            string spacing = "\r\t\t\t\t\t\t\t\t";

            if (BestMatchedRegistry != null)
            {
                if(BestMatchedRegistry.MostSimilarPackage != null)
                {
                    var package = BestMatchedRegistry.MostSimilarPackage;
                    var sBuilder = new StringBuilder();

                    sBuilder.Append(string.Format("{0}{1}| ", GetFullName(),spacing));

                    var similarity = package.BestSimilarity;
                    if(similarity >= 0.99)
                    {
                        sBuilder.Append("Corr. altíssima ");
                    }
                    else  if(similarity > 0.95)
                    {
                        sBuilder.Append("Corr. alta ");
                    }
                    else
                    {
                        sBuilder.Append("Corr. média ");
                    }
                    sBuilder.Append(string.Format("({0:0.00}%) ", similarity * 100));
                    sBuilder.Append(string.Format("com \"{0}\" na versão {1}",package.Name, package.Version));
                    sBuilder.Append(string.Format(" - Última versão: {0}", BestMatchedRegistry.LatestVersion));
                    return sBuilder.ToString();
                }
            }
            return string.Format("{0}{1}| Sem correspondências", GetFullName(),spacing);
        }
    }
}
