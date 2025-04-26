using Common.Classes;

namespace Common.ClassesDB
{
    public class ExtJSResult: AnalysisResult
    {
        public int TotalCount { get; set; }
        public List<ExtensionJSFile> ExtensionJSFiles { get; set; }

        public ExtJSResult():base() 
        {
            ExtensionJSFiles = [];
            TotalCount = 0;
        }

        public ExtJSResult(BrowserExtension extension, string analysisId) : base(analysisId)
        {
            ExtensionJSFiles = [];
            TotalCount = extension.ContainedJSFiles.Count;
        }
        public ExtJSResult(ICollection<JSFile> files, string analysisId) : base(analysisId)
        {
            ExtensionJSFiles = [];
            TotalCount = files.Count;
        }
    }
}
