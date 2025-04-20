using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.ClassesWeb.GoogleSafeBrowsing
{
    //Enum contêm valores equivalentes ao enum "ThreatType" do Google Safe Browsing APIv4 
    [JsonConverter(typeof(JsonStringEnumConverter<GSBThreatType>))]
    public enum GSBThreatType
    {
        THREAT_TYPE_UNSPECIFIED = 0,
        MALWARE = 1,
        SOCIAL_ENGINEERING = 2,
        UNWANTED_SOFTWARE = 3,
        POTENTIALLY_HARMFUL_APPLICATION = 4,
        UNCHECKED = 5,
        SAFE = 6,
    }
}
