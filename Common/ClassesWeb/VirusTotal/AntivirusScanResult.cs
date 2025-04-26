using System.Text.Json.Serialization;

namespace Common.ClassesWeb.VirusTotal
{
    public class AntivirusScanResult
    {
        [JsonPropertyName("engine_name")]
        public string EngineName { get; set; }
        [JsonPropertyName("engine_version")]
        public string EngineVersion { get; set; }
        public DateTime EngineUpdate { get; set; }
        [JsonPropertyName("method")]
        public string Method { get; set; }
        [JsonPropertyName("category")]
        public string Category { get; set; }
        public AntivirusScanResult() 
        { 
            EngineName = string.Empty;
            EngineVersion = string.Empty;
            Method = string.Empty;
            Category = string.Empty;
            EngineUpdate = DateTime.MinValue;
        }
    }
}
