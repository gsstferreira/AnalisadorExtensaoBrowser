using Common.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.ClassesDB
{
    public class ExtensionURLsResult : AnalysisResult
    {
        public List<Url> Urls { get; set; }

        public ExtensionURLsResult(): base() 
        {
            Urls = [];
        }

        public ExtensionURLsResult(BrowserExtension extension) : base(extension.Id, extension.Version)
        {
            Urls = [];
            Urls.AddRange(extension.ContainedURLs);
        }
    }
}
