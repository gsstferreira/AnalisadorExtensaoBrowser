using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.ClassesJSON
{
    public class VTQueueJson
    {
        [JsonPropertyName("data")]
        public VTQueueInfo Information { get; set; }
        public VTQueueJson() 
        { 
            Information = new VTQueueInfo();
        }
    }

    public class VTQueueInfo
    {
        [JsonPropertyName("type")]
        public string OperationType { get; set; }
        [JsonPropertyName("id")]
        public string OperationId { get; set; }
        [JsonPropertyName("links")]
        public VTQueueLinks AnalysisLinks { get; set; }

        public VTQueueInfo() 
        {
            AnalysisLinks = new VTQueueLinks();
            OperationType = string.Empty;
            OperationId = string.Empty;
        }
    }

    public class VTQueueLinks
    {
        [JsonPropertyName("self")]
        public string AnalysisUrl { get; set; }

        public VTQueueLinks()
        {
            AnalysisUrl = string.Empty;
        }
    } 
}
