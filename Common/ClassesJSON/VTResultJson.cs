using Common.ClassesWeb.VirusTotal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.ClassesJSON
{
    public class VTResultJson
    {
        [JsonPropertyName("data")]
        public VTResultData ResultData { get; set; }
        [JsonPropertyName("meta")]
        public VTResultMetaData MetaData { get; set; }

        public VTResultJson() 
        {
            ResultData = new VTResultData();
            MetaData = new VTResultMetaData();
        }
    }

    public class VTResultData 
    {
        [JsonPropertyName("attributes")]
        public DataAttributes Attributes { get; set; }
        public VTResultData()
        {
            Attributes = new DataAttributes();
        }
    }
    public class VTResultMetaData 
    {
        [JsonPropertyName("file_info")]
        public MetaDataDetails Info { get; set; }

        public VTResultMetaData() 
        {
            Info = new MetaDataDetails();
        }
    }
    public class MetaDataDetails
    {
        [JsonPropertyName("sha256")]
        public string Sha256 { get; set; }
        [JsonPropertyName("md5")]
        public string Md5 { get; set; }
        [JsonPropertyName("sha1")]
        public string Sha1 { get; set; }
        [JsonPropertyName("size")]
        public long Size { get; set; }
        public MetaDataDetails() 
        {
            Sha256 = string.Empty;
            Md5 = string.Empty;
            Sha1 = string.Empty;
            Size = 0;
        }

    }
    public class DataAttributes 
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("date")]
        public  long DateCompletion { get; set; }
        [JsonPropertyName("results")]
        public Dictionary<string, VTEngine> Results { get; set; }
        [JsonPropertyName("stats")]
        public VTResponseStatistics ResultStats { get; set; }

        public DataAttributes()
        {
            Status = string.Empty;
            DateCompletion = 0;
            Results = [];
            ResultStats = new VTResponseStatistics();
        }
    }

    public class VTEngine 
    {
        [JsonPropertyName("method")]
        public string Method { get; set; }
        [JsonPropertyName("engine_name")]
        public string EngineName { get; set; }
        [JsonPropertyName("engine_update")]
        public string EngineUpdate { get; set; }
        [JsonPropertyName("engine_version")]
        public string EngineVersion { get; set; }
        [JsonPropertyName("category")]
        public string Category { get; set; }

        public VTEngine()
        {
            Method = string.Empty;
            EngineName = string.Empty;
            EngineUpdate = string.Empty;
            EngineVersion = string.Empty;
            Category = string.Empty;
        }
    }
}
