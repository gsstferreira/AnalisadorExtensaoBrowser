using Common.Classes;
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

        public ExtensionVTResult(BrowserExtension extension) : base(extension.Id, extension.Version)
        {
            VirusTotalResultURL = extension.VirusTotalAnalysisUrl;
        }
    }
}
