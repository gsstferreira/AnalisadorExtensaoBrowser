using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.ClassesDB
{
    public class ExtensionVTResult:AnalysisResult
    {
        public string VirusTotalResultURL { get; set; }

        public ExtensionVTResult() : base() 
        {
            VirusTotalResultURL = string.Empty;
        }

        public ExtensionVTResult(string vtUrl, string id, string version) : base(id, version)
        {
            VirusTotalResultURL = vtUrl;
        }
    }
}
