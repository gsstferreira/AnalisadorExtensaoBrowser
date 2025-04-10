using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
