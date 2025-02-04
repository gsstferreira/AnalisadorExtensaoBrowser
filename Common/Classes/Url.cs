using Common.WebClasses.GoogleSafeBrowsing;
using System.Text;

namespace Common.Classes
{
    public class Url
    {
        public string Path { get; set; }
        public GSBThreatType ThreatType { get; set; }

        public Url(string url) 
        {
            Path = url;
            ThreatType = GSBThreatType.UNCHECKED;
        }

        public override string ToString()
        {
            return string.Format("{0} | {1}", Path, ThreatType.ToString());
        }
        public override bool Equals(object obj)
        {
            if (obj is not Url x) return false;
            else
            {
                return Path.Equals(x.Path);
            }
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }   
}
