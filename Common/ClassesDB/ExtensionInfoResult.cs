using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime.Internal.Transform;
using Common.Classes;
using System.Text.Json;

namespace Common.ClassesDB
{
    public class ExtensionInfoResult:AnalysisResult
    {
        public string PageURL { get; set; }
        public string IconUrl { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public string Version { get; set; }
        public string Provider { get; set; }
        public float Rating { get; set; }
        public int NumReviews { get; set; }
        public long NumDownloads { get; set; }
        public DateTime LastUpdated { get; set; }

        public ExtensionInfoResult() : base() 
        { 
            PageURL = string.Empty;
            IconUrl = string.Empty;
            Name = string.Empty;
            Provider = string.Empty;
            Id = string.Empty;
            Version = string.Empty;
            Rating = -1;
            NumReviews = -1;
            NumDownloads = -1;
            LastUpdated = DateTime.MinValue;
        }
        public ExtensionInfoResult(BrowserExtension extension, string analysisId) : base(analysisId) 
        {
            PageURL = extension.PageUrl;
            IconUrl = extension.IconUrl;
            Name = extension.Name;
            Provider = extension.Provider;
            Rating = extension.Rating;
            NumReviews = extension.NumReviews;
            NumDownloads = extension.NumDownloads;
            LastUpdated = extension.LastUpdated;
            Id = extension.Id;
            Version = extension.Version;
        }
    }
}
