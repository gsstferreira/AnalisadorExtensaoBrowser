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
        public string Name { get; set; }
        public string Provider { get; set; }
        public float Rating { get; set; }
        public int NumReviews { get; set; }
        public long NumDownloads { get; set; }
        public DateTime LastUpdated { get; set; }

        public ExtensionInfoResult() : base() { }
        public ExtensionInfoResult(BrowserExtension extension) : base(extension.ID, extension.Version) 
        {
            PageURL = extension.PageUrl;
            Name = extension.Name;
            Provider = extension.Provider;
            Rating = extension.Rating;
            NumReviews = extension.NumReviews;
            NumDownloads = extension.NumDownloads;
            LastUpdated = extension.LastUpdated;
        }
    }
}
