using Common.Classes;

namespace Common.ClassesDB
{
    public class ExtVTResult:AnalysisResult
    {
        public string VirusTotalResultURL { get; set; }

        public ExtVTResult() : base() 
        {
            VirusTotalResultURL = string.Empty;
        }

        public ExtVTResult(BrowserExtension extension, string analysisId) : base(analysisId)
        {
            VirusTotalResultURL = extension.VirusTotalAnalysisUrl;
        }
    }
}
