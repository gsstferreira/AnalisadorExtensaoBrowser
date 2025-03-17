using Amazon;
using Amazon.DynamoDBv2;
using Common.Classes;
using System.Buffers.Text;
using System.Text;

namespace DB.Classes
{
    public abstract class AnalysisResult
    {
        protected static readonly AmazonDynamoDBClient DBClient = new(new AmazonDynamoDBConfig()
        {
            RegionEndpoint = RegionEndpoint.SAEast1,
            Profile = new Profile("BrowserExtensionAnalysis")
        });
        public string AnalysisID {get; set;}
        public string ExtensionID { get; set; }
        public string ExtensionVersion { get; set; }
        public DateTimeOffset DateCompletion { get; set; }
        public AnalysisResult(BrowserExtension extension) {
            ExtensionID = extension.ID;
            ExtensionVersion = extension.Version;
            DateCompletion = DateTimeOffset.Now;

            AnalysisID = Convert.ToBase64String(Encoding.UTF8.GetBytes(ExtensionID+ExtensionVersion));
        }
    }
}
