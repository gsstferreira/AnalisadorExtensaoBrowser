using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
        public AntivirusScanResult() { }
    }
}
