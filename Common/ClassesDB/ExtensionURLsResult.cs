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

        public ExtensionURLsResult(ICollection<Url> urls, string id, string version) : base(id, version)
        {
            Urls = [];
            Urls.AddRange(urls);
        }
    }
}
