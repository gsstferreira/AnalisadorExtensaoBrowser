using Common.ClassesWeb.GoogleSafeBrowsing;

namespace Common.Classes
{
    public class Url
    {
        public string Path { get; set; }
        public GSBThreatType ThreatType { get; set; }

        public Url() 
        {
            Path = string.Empty;
            ThreatType = GSBThreatType.UNCHECKED;
        }
        public Url(string url) 
        {
            Path = url;
            ThreatType = GSBThreatType.UNCHECKED;
        }

        public override string ToString()
        {
            return string.Format("{0} | {1}", Path, ThreatType.ToString());
        }
    }   
}
