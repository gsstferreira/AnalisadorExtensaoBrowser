using System.Text.Json.Serialization;

namespace Common.ClassesWeb.GoogleSafeBrowsing
{
    public class GSBResponse
    {
        [JsonPropertyName("matches")]
        public List<GSBThreathMatch> ThreatMatches {get;set;}

        public GSBResponse() {
            ThreatMatches = [];
        }
    }
}
