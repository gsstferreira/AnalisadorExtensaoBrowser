using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.ClassesWeb.GoogleSafeBrowsing
{
    public class GSBThreatEntry
    {
        public string hash { get; set; }
        public string url { get; set; }
        public string digest { get; set; }
    }
}
