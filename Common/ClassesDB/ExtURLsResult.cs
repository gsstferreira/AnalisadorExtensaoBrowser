using Common.Classes;

namespace Common.ClassesDB
{
    public class ExtURLsResult : AnalysisResult
    {
        public List<Url> Urls { get; set; }

        public ExtURLsResult(): base() 
        {
            Urls = [];
        }

        public ExtURLsResult(BrowserExtension extension, string analysisId) : base(analysisId)
        {
            Urls = [];
            Urls.AddRange(extension.ContainedURLs);
        }
    }
}
