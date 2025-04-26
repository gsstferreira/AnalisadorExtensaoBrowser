using System.Text.Json.Serialization;

namespace Common.ClassesWeb.VirusTotal
{
    public class VTResponseAttributes
    {
        [JsonPropertyName("results")]
        public List<AntivirusScanResult> AntivirusResults { get; set; } = [];
        public VTResponseAttributes() { }
    }
}
