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
        public int TotalFilesChecked { get; set; }
        public ExtensionJSFile()
        {
            Name = string.Empty;
            MatchedLibrary = string.Empty;
            MatchedVersion = string.Empty;
            LatestVersionStable = string.Empty;
            LatestVersionDevelopment = string.Empty;

            HasMatch = false;
            Similarity = 0;
            TotalFilesChecked = 0;

            MatchedVersionDate = DateTime.MinValue;
            LatestUpdateStable = DateTime.MinValue;
            LatestUpdateDevelopment = DateTime.MinValue;
        }

        public ExtensionJSFile(JSFile jsFile) 
        {
            Name = jsFile.Name;
            TotalFilesChecked = jsFile.TotalFilesChecked;
            MatchedLibrary = string.Empty;
            MatchedVersion = string.Empty;
            LatestVersionStable = string.Empty;
            LatestVersionDevelopment = string.Empty;
            MatchedVersionDate = DateTime.MinValue;
            LatestUpdateStable = DateTime.MinValue;
            LatestUpdateDevelopment = DateTime.MinValue;
            Similarity = -1;
            HasMatch = false;

            if(jsFile.BestRegistry is not null)
            {
                if(jsFile.BestRegistry.BestPackage is not null)
                {
                    HasMatch = true;

                    var package = jsFile.BestRegistry.BestPackage;
                    MatchedLibrary = package.Name;
                    MatchedVersion = package.Version;
                    MatchedVersionDate = package.ReleaseDate;
                    LatestVersionStable = jsFile.BestRegistry.LatestVersionStable;
                    LatestVersionDevelopment = jsFile.BestRegistry.LatestVersionDevelopment;
                    LatestUpdateStable = jsFile.BestRegistry.LatestUpdateStable;
                    LatestUpdateDevelopment = jsFile.BestRegistry.LatestUpdateDevelopment;
                    Similarity = package.BestSimilarity;
                }
            }
        }
    }
}
