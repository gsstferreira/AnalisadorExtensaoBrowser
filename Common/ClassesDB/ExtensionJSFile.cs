using Common.Classes;

namespace Common.ClassesDB
{
    public  class ExtensionJSFile
    {
        public string Name { get; set; }
        public string MatchedLibrary { get; set; }
        public string MatchedVersion { get; set; }
        public string LatestVersionStable { get; set; }
        public string LatestVersionDevelopment { get; set; }
        public DateTime MatchedVersionDate { get; set; }
        public DateTime LatestUpdateStable { get; set; }
        public DateTime LatestUpdateDevelopment { get; set; }
        public double Similarity { get; set; }
        public bool HasMatch { get; set; }
        public ExtensionJSFile()
        {
            Name = string.Empty;
            MatchedLibrary = string.Empty;
            MatchedVersion = string.Empty;
            LatestVersionStable = string.Empty;
            LatestVersionDevelopment = string.Empty;

            HasMatch = false;
            Similarity = 0;

            MatchedVersionDate = DateTime.MinValue;
            LatestUpdateStable = DateTime.MinValue;
            LatestUpdateDevelopment = DateTime.MinValue;
        }

        public ExtensionJSFile(JSFile jsFile) 
        {
            Name = jsFile.Name;
            MatchedLibrary = string.Empty;
            MatchedVersion = string.Empty;
            LatestVersionStable = string.Empty;
            LatestVersionDevelopment = string.Empty;
            MatchedVersionDate = DateTime.MinValue;
            LatestUpdateStable = DateTime.MinValue;
            LatestUpdateDevelopment = DateTime.MinValue;
            Similarity = -1;
            HasMatch = false;

            if(jsFile.BestMatchedRegistry != null)
            {
                if(jsFile.BestMatchedRegistry.MostSimilarPackage != null)
                {
                    HasMatch = true;

                    var package = jsFile.BestMatchedRegistry.MostSimilarPackage;
                    MatchedLibrary = package.Name;
                    MatchedVersion = package.Version;
                    MatchedVersionDate = package.ReleaseDate;
                    LatestVersionStable = jsFile.BestMatchedRegistry.LatestVersionStable;
                    LatestVersionDevelopment = jsFile.BestMatchedRegistry.LatestVersionDevelopment;
                    LatestUpdateStable = jsFile.BestMatchedRegistry.LatestUpdateStable;
                    LatestUpdateDevelopment = jsFile.BestMatchedRegistry.LatestUpdateDevelopment;
                    Similarity = package.BestSimilarity;
                }
            }
        }
    }
}
