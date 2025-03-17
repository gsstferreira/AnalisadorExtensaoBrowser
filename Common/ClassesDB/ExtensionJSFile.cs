using Amazon.Runtime;
using Common.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.ClassesDB
{
    public  class ExtensionJSFile
    {
        public string Name { get; set; }
        public string MatchedLibrary { get; set; }
        public string MatchedVersion { get; set; }
        public string LastVersion { get; set; }
        public DateTime MatchedVersionDate { get; set; }
        public DateTime LastVersionDate { get; set; }
        public double MatchConfidence { get; set; }
        public bool HasMatch { get; set; }
        public ExtensionJSFile()
        {
            Name = string.Empty;
            MatchedLibrary = string.Empty;
            MatchedVersion = string.Empty;
            LastVersion = string.Empty;
        }

        public ExtensionJSFile(JSFile jsFile) 
        {
            Name = jsFile.Name;
            MatchedLibrary = string.Empty;
            MatchedVersion = string.Empty;
            LastVersion = string.Empty;
            MatchedVersionDate = DateTime.MinValue;
            LastVersionDate = DateTime.MinValue;
            MatchConfidence = -1;
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
                    LastVersion = jsFile.BestMatchedRegistry.LatestVersion;
                    LastVersionDate = jsFile.BestMatchedRegistry.LastUpdated;
                    MatchConfidence = package.BestSimilarity;
                }
            }
        }
    }
}
