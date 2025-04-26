using Common.ClassesLambda;
using Common.Handlers;
using System.IO.Compression;
using System.Text;

namespace Common.Classes
{
    public class JSFile
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string FullName { get; set; }
        public string Content { get; set; }
        public long Lenght { get; set; }
        public uint Crc32 { get; set; }
        public List<NpmRegistry> NPMRegistries { get; set; }
        public NpmRegistry? BestRegistry { get; set; }
        public int TotalFilesChecked { get; set; }
        public Dictionary<ulong, int> Profile { get; set; }
        public double Norm { get; set; }
        public JSFile(ZipArchiveEntry entry) 
        {
            Name = entry.Name.Replace(".js",string.Empty).Trim();
            Path = entry.FullName.Replace(entry.Name, string.Empty).Trim();
            FullName = entry.FullName;

            NPMRegistries = [];
            Crc32 = entry.Crc32;
            using (var reader = new StreamReader(entry.Open()))
            {
                Content = reader.ReadToEnd().Trim();
            }
            Lenght = Content.Length;
            BestRegistry = null;
            TotalFilesChecked = 0;
            Profile = new((int)Lenght / SimilarityHandler.DEFAULT_K);
            SimilarityHandler.SetCosineProfile(this);
        }

        public JSFile(JSFileLambdaPayload payload)
        {
            Name = payload.Name;
            Path = payload.Directory;
            FullName = string.Empty;
            Content = payload.Content;
            Lenght = Content.Length;
            Crc32 = 0;
            NPMRegistries = payload.NPMRegistries;
            TotalFilesChecked = 0;
            Profile = [];
            Norm = 0;
        }

        public void DisposeRegistriesList()
        {
            NPMRegistries?.Clear();
        }
        public void DisposeProfile()
        {
            Profile?.Clear();
        }

        public override string ToString()
        {
            if (BestRegistry is not null)
            {
                if(BestRegistry.BestPackage is not null)
                {
                    var package = BestRegistry.BestPackage;
                    var sBuilder = new StringBuilder();

                    sBuilder.Append(string.Format("{0} | ", FullName));

                    var similarity = package.BestSimilarity;
                    sBuilder.Append(string.Format("({0:0.00}%) similar com ", similarity * 100));
                    sBuilder.Append(string.Format("\"{0}\" na versão {1}",package.Name, package.Version));
                    sBuilder.Append(string.Format(" - Última versão: {0}", BestRegistry.LatestVersionStable));
                    sBuilder.Append(string.Format(" - {0} arquivos analisados", TotalFilesChecked));
                    return sBuilder.ToString();
                }
            }
            return string.Format("{0} | Sem correspondências - {1} arquivos analisados", FullName,TotalFilesChecked);
        }
    }
}
