using Common.Classes;

namespace AnalysisWebApp.Models
{
    public class ExtensionViewModel
    {
        public string PageUrl { get; set; }
        public string DownloadUrl { get; set; }
        public string SimpleName { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Provider { get; set; }
        public float Rating { get; set; }
        public int NumReviews { get; set; }
        public long NumDownloads { get; set; }
        public DateTime LastUpdated { get; set; }

        public ExtensionViewModel()
        {
            PageUrl = string.Empty;
            DownloadUrl = string.Empty;
            SimpleName = string.Empty;
            ID = string.Empty;
            Name = string.Empty;
            Version = string.Empty;
            Provider = string.Empty;
            Rating = -1;
            NumReviews = -1;
            NumDownloads = -1;
            LastUpdated = DateTime.MinValue;
        }
        public ExtensionViewModel(BrowserExtension ext) 
        {
            PageUrl = ext.PageUrl;
            DownloadUrl = ext.DownloadUrl;
            SimpleName = ext.SimpleName;
            ID = ext.Id;
            Name = ext.Name;
            Version = ext.Version;
            Provider = ext.Provider;
            Rating = ext.Rating;
            NumReviews = ext.NumReviews;
            NumDownloads = ext.NumDownloads;
            LastUpdated = ext.LastUpdated;
        }
    }
}
