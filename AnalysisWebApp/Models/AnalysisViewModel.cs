using Common.ClassesDB;
using Common.ClassesWeb.GoogleSafeBrowsing;
using Common.ClassesWeb.VirusTotal;
using Common.Enums;
using Common.Handlers;

namespace AnalysisWebApp.Models
{
    public class AnalysisViewModel
    {
        public string AnalysisId { get; set; }
        public string AnalysisDateTime { get; set; }
        public string ExtensionName { get; set; }
        public string ExtensionVersion { get; set; }
        public string ExtensionId { get; set; }
        public string ExtensionRating { get; set; }
        public string ExtensionReviews { get; set; }
        public string ExtensionDownloads { get; set; }
        public string ExtensionProvider { get; set; }
        public string ExtensionPageUrl { get; set; }
        public string ExtensionsIconUrl { get; set; }
        public string ExtensionLastUpdate { get; set; }
        public ExtensionPermissionGroup ExtensionPermissions { get; set; }
        public List<JsFileViewModel> ExtensionJSFiles { get; set; }
        public List<UrlViewModel> ExtensionUrls { get; set; }
        public VirusTotalViewModel ExtensionVTResponse { get; set; }

        public AnalysisViewModel() 
        {
            AnalysisId = string.Empty;
            AnalysisDateTime = string.Empty;
            ExtensionName = string.Empty;
            ExtensionProvider = string.Empty;
            ExtensionVersion = string.Empty;
            ExtensionId = string.Empty;
            ExtensionRating = string.Empty;
            ExtensionReviews = string.Empty;
            ExtensionDownloads = string.Empty;
            ExtensionPageUrl = string.Empty;
            ExtensionsIconUrl = string.Empty;
            ExtensionLastUpdate = string.Empty;

            ExtensionPermissions = new ExtensionPermissionGroup();

            ExtensionJSFiles = [];
            ExtensionUrls = [];

            ExtensionVTResponse = new VirusTotalViewModel();
        }

        private static string NumberToString(long number)
        {
            if(number > 1000000)
            {
                int million = (int)Math.Round(number / 1000000.0);
                return string.Format("{0} milhões", million);
            }
            else if(number > 1000) 
            {
                int thousand = (int)Math.Round(number / 1000.0);
                return string.Format("{0} mil", thousand);
            }
            else
            {
                return number.ToString();
            }
        }

        public void ParseExtensionInfo(ExtensionInfoResult result)
        {
            if (result != null)
            {
                AnalysisId = result.AnalysisID;
                AnalysisDateTime = result.DateCompletion.ToString(Constants.DateStringFormat);

                ExtensionName = result.Name;
                ExtensionProvider = result.Provider;
                ExtensionVersion = result.ExtensionVersion;
                ExtensionId = result.ExtensionID;
                ExtensionPageUrl = result.PageURL;
                ExtensionsIconUrl = result.IconUrl;

                ExtensionLastUpdate = result.LastUpdated.ToString(Constants.DateStringFormat);
                ExtensionRating = string.Format("{0:0.0}", result.Rating / 10);
                ExtensionReviews = NumberToString(result.NumReviews);
                ExtensionDownloads = NumberToString(result.NumDownloads);
            }
        }
        public void ParseExtensionJSFiles(ExtensionJSResult result)
        {
            if (result != null)
            {
                foreach (var file in result.ExtensionJSFiles) 
                {
                    ExtensionJSFiles.Add(new JsFileViewModel(file));
                }
                ExtensionJSFiles = [.. ExtensionJSFiles.OrderBy(f => !f.HasMatch).ThenBy(f => f.Name)];
            }
        }
        public void ParseExtensionUrls(ExtensionURLsResult result)
        {
            if (result != null) 
            {
                foreach (var url in result.Urls)
                {
                    ExtensionUrls.Add(new UrlViewModel(url));
                }
                ExtensionUrls = [.. ExtensionUrls.OrderBy(u => u.Threat).ThenBy(u => u.OriginalUrl)];
            }
        }
        public void ParseExtensionPermissions(ExtensionPermissionsResult result) 
        { 
            if (result != null) 
            {
                foreach (var entry in result.Permissions)
                {
                    switch (entry.Type)
                    {
                        case PermissionType.Permission:
                            ExtensionPermissions.Permissions.Add(entry.Name);
                            break;
                        case PermissionType.OptionalPermission:
                            ExtensionPermissions.OptionalPermissions.Add(entry.Name);
                            break;
                        case PermissionType.Host:
                            ExtensionPermissions.Hosts.Add(entry.Name);
                            break;
                        case PermissionType.OptionalHost:
                            ExtensionPermissions.OptionalHosts.Add(entry.Name);
                            break;
                    }
                }
            }
        }
        public void ParseExtensionVTresult(ExtensionVTResult result)
        {
            if (result != null)
            {
                var analysis = VirusTotalHandler.CheckVTAnalysis(result.VirusTotalResultURL);

                ExtensionVTResponse = new VirusTotalViewModel(analysis);
            }
        }
    }
}
