using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.ClassesWeb.VirusTotal
{
    public class VTResponseAttributes
    {
        [JsonPropertyName("results")]
        public List<AntivirusScanResult> AntivirusResults { get; set; } = [];
        public VTResponseAttributes() { }
    }
}
