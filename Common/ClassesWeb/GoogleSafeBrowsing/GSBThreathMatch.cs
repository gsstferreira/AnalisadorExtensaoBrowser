using System.Text.Json.Serialization;

namespace Common.ClassesWeb.GoogleSafeBrowsing
{
    public class GSBThreathMatch
    {
        [JsonPropertyName("threatType")]
        public GSBThreatType ThreatType { get; set; }
        [JsonPropertyName("platformType")]
        public GSBPlatformType PlatformType { get; set; }
        [JsonPropertyName("threat")]
        public GSBThreatEntry ThreatEntry { get; set; }
        public GSBThreathMatch() 
        {
            ThreatType = GSBThreatType.UNCHECKED;
            PlatformType = GSBPlatformType.PLATFORM_TYPE_UNSPECIFIED;
            ThreatEntry = new GSBThreatEntry();
        }
    }
}
