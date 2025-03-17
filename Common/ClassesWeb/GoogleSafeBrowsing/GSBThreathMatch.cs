namespace Common.ClassesWeb.GoogleSafeBrowsing
{
    public class GSBThreathMatch
    {
        public GSBThreatType threatType { get; set; }
        public GSBPlatformType platformType { get; set; }
        public GSBThreatEntry threat { get; set; }
        public GSBThreathMatch() { }

        public string GetUrl()
        {
            return this.threat.url;
        }
    }
}
