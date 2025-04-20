using Common.Classes;

namespace Common.ClassesDB
{
    public class ExtensionVTResult:AnalysisResult
    {
        public string VirusTotalResultURL { get; set; }

        public ExtensionVTResult() : base() 
        {
            VirusTotalResultURL = string.Empty;
        }

        public ExtensionVTResult(BrowserExtension extension, string analysisId) : base(analysisId)
        {
            VirusTotalResultURL = extension.VirusTotalAnalysisUrl;
        }
    }
}
