using System.Text.Json.Serialization;

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
