using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.WebClasses.GoogleSafeBrowsing
{
    public class GSBResponse
    {
        public List<GSBThreathMatch> matches {get;set;}

        public GSBResponse() {
            matches = [];
        }
    }
}
