using Common.ClassesDB;
using System.Web;

namespace AnalysisWebApp.Models
{
    public class JsFileViewModel
    {
        public string Name { get; set; }
        public bool HasMatch { get; set; }
        public string MatchedLibrary { get; set; }
        public string MatchedVersion { get; set; }
        public string MatchedVersionDate { get; set; }
        public string LatestVersionStable { get; set; }
        public string LatestVersionDevelopment { get; set; }
        public string LatestUpdateStable { get; set; }
        public string LatestUpdateDevelopment { get; set; }
        public string Similarity { get; set; }
        public string NpmPackageLink { get; set; }
        public string SnykVulnerabilityLink { get; set; }

        public JsFileViewModel()
        {
            Name = string.Empty;
            HasMatch = false;
            MatchedLibrary = string.Empty;
            MatchedVersion = string.Empty;
            MatchedVersionDate = string.Empty;
            LatestVersionStable = string.Empty;
            LatestVersionDevelopment = string.Empty;
            LatestUpdateStable = string.Empty;
            LatestUpdateDevelopment = string.Empty;
            Similarity = string.Empty;
            SnykVulnerabilityLink = string.Empty;
            NpmPackageLink = string.Empty;
        }
        public JsFileViewModel(ExtensionJSFile file)
        {
            Name = file.Name;
            HasMatch = file.HasMatch;
            MatchedLibrary = file.MatchedLibrary;
            MatchedVersion = file.MatchedVersion;
            LatestVersionStable = file.LatestVersionStable;
            LatestVersionDevelopment = file.LatestVersionDevelopment;

            MatchedVersionDate = file.MatchedVersionDate.ToString(Constants.DateStringFormat);
            LatestUpdateStable = file.LatestUpdateStable.ToString(Constants.DateStringFormat);
            LatestUpdateDevelopment = file.LatestUpdateDevelopment.ToString(Constants.DateStringFormat);

            Similarity = string.Format("{0:0.00}%", file.Similarity*100);

            var nameEncoded = HttpUtility.UrlEncode(MatchedLibrary);
            var versionEncoded = HttpUtility.UrlEncode(MatchedVersion);

            SnykVulnerabilityLink = Common.Res.Params.SnykDbUrl.Replace("[pack]", nameEncoded).Replace("[version]", versionEncoded);
            NpmPackageLink = Common.Res.Params.NpmPackageUrl + MatchedLibrary;
        }

        public string GetMatchedVersionInfo()
        {
            return string.Format("{0} ({1})", MatchedVersion, MatchedVersionDate);
        }
        public string GetLatestVersionStableInfo()
        {
            if (!string.IsNullOrEmpty(LatestVersionStable))
            {
                return string.Format("{0} ({1})", LatestVersionStable, LatestUpdateStable);
            }
            else
            {
                return "N/A";
            }
        }
        public string GetLatestVersionDevInfo()
        {
            if(!string.IsNullOrEmpty(LatestVersionDevelopment))
            {
                return string.Format("{0} ({1})", LatestVersionDevelopment, LatestUpdateDevelopment);
            }
            else
            {
                return "N/A";
            }
        }
    }
}
