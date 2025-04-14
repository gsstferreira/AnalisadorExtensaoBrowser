using Common.ClassesWeb.GoogleSafeBrowsing;
using Common.Enums;

namespace Common.Classes
{
    public class Url
    {
        public string OriginalUrl { get; set; }
        public string Host { get; set; }
        public string RedirectUrl { get; set; }
        public bool IsHttps { get; set; }
        public UrlType Type { get; set; }
        public GSBThreatType ThreatType { get; set; }

        public Url() 
        {
            OriginalUrl = string.Empty;
            Host = string.Empty;
            RedirectUrl = string.Empty;
            IsHttps = false;
            Type = UrlType.UNCHECKED;
            ThreatType = GSBThreatType.UNCHECKED;
        }
        public Url(string url) 
        {
            OriginalUrl = url;
            Host = string.Empty;
            IsHttps = false;
            RedirectUrl= string.Empty;
            Type = UrlType.UNCHECKED;
            ThreatType = GSBThreatType.UNCHECKED;
        }

        public override string ToString()
        {
            return string.Format("{0} | {1}", OriginalUrl, ThreatType.ToString());
        }
    }   
}
