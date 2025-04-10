using Amazon;
using Amazon.DynamoDBv2;
using Common.Classes;
using System.Buffers.Text;
using System.Text;

namespace Common.ClassesDB
{
    public abstract class AnalysisResult
    {
        public string AnalysisID {get; set;}
        public string ExtensionID { get; set; }
        public string ExtensionVersion { get; set; }
        public DateTimeOffset DateCompletion { get; set; }
        public AnalysisResult() 
        {
            AnalysisID = string.Empty;
            ExtensionID = string.Empty;
            ExtensionVersion = string.Empty;
            DateCompletion = DateTimeOffset.MinValue;
        }
        public AnalysisResult(string id, string version) {
            ExtensionID = id;
            ExtensionVersion = version;
            DateCompletion = DateTimeOffset.Now;

            AnalysisID = Convert.ToBase64String(Encoding.UTF8.GetBytes(ExtensionID+ExtensionVersion));
        }
    }
}
