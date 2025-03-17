using Common.ClassesWeb.NPMRegistry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Classes
{
    public class NPMRegistry
    {
        public string Name { get; set; }
        public string LatestVersion { get; set; }
        public DateTime LastUpdated { get; set; }
        public string HomepageUrl { get; set; }
        public string RepositoryUrl { get; set; }
        public string BugTrackerUrl { get; set; }
        public string NPMPageUrl { get; set; }
        public List<NPMPackage> Packages { get; set; }
        public NPMPackage? MostSimilarPackage { get; set; }
        public int TotalJsFiles { get; set; }
        public NPMRegistry() 
        { 
            Name = string.Empty;
            LatestVersion = string.Empty;
            LastUpdated = DateTime.MinValue;
            HomepageUrl = string.Empty;
            RepositoryUrl = string.Empty;
            BugTrackerUrl = string.Empty;
            NPMPageUrl = string.Empty;
            Packages = [];
            MostSimilarPackage = null;
            TotalJsFiles = 0;
        }
        public bool HasAnyMatchedPackages()
        {
            foreach (var package in Packages) 
            {
                if (package.BestSimilarity > 0) return true;
            }
            return false;
        }
    }
}
