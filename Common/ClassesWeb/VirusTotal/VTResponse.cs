using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.ClassesWeb.VirusTotal
{
    public class VTResponse
    {
        [JsonPropertyName("attributes")]
        public List<AntivirusScanResult> ScanResults { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("date")]
        public DateTime Date {  get; set; }
        [JsonPropertyName("stats")]
        public VTResponseStatistics Statistics { get; set; }
        public VTResponse() 
        { 
            ScanResults = [];
            Status = string.Empty;
            Date = DateTime.MinValue;
            Statistics = new VTResponseStatistics();
        }
    }
}
