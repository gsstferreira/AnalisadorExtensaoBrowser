namespace Common.Classes
{
    public class NpmRegistry
    {
        public string Name { get; set; }
        public string LatestVersionStable { get; set; }
        public string LatestVersionDevelopment { get; set; }
        public DateTime LatestUpdateStable { get; set; }
        public DateTime LatestUpdateDevelopment { get; set; }
        public string HomepageUrl { get; set; }
        public string RepositoryUrl { get; set; }
        public string BugTrackerUrl { get; set; }
        public string NPMPageUrl { get; set; }
        public List<NPMPackage> Packages { get; set; }
        public NPMPackage? BestPackage { get; set; }
        public int TotalNumFilesChecked { get; set; }
        public int TotalNumFilesOpened { get; set; }
        public NpmRegistry() 
        { 
            Name = string.Empty;
            LatestVersionStable = string.Empty;
            LatestVersionDevelopment = string.Empty;
            LatestUpdateStable = DateTime.MinValue;
            LatestUpdateDevelopment = DateTime.MinValue;
            HomepageUrl = string.Empty;
            RepositoryUrl = string.Empty;
            BugTrackerUrl = string.Empty;
            NPMPageUrl = string.Empty;
            Packages = [];
            BestPackage = null;
        }
        public bool HasAnyMatchedPackages()
        {
            foreach (var package in Packages) 
            {
                if (package.BestSimilarity > 0) return true;
            }
            return false;
        }

        public void CountNumFilesChecked()
        {
            TotalNumFilesChecked = Packages.Sum(x => x.FilesChecked);
            TotalNumFilesOpened = Packages.Sum(x => x.FilesOpened);
        }

        public void DisposePackagesList()
        {
            Packages?.Clear();
        }
    }
}
