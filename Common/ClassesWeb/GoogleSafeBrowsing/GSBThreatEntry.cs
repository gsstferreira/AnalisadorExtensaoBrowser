using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.ClassesWeb.GoogleSafeBrowsing
{
    public class GSBThreatEntry
    {
        [JsonPropertyName("hash")]
        public string Hash { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("digest")]
        public string Digest { get; set; }

        public GSBThreatEntry()
        {
            Hash = string.Empty;
            Url = string.Empty;
            Digest = string.Empty;
        }
    }
}
