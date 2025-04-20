using Common.Classes;

namespace Common.ClassesDB
{
    public class ExtensionJSResult: AnalysisResult
    {
        public int TotalCount { get; set; }
        public List<ExtensionJSFile> ExtensionJSFiles { get; set; }

        public ExtensionJSResult():base() 
        {
            ExtensionJSFiles = [];
            TotalCount = 0;
        }

        public ExtensionJSResult(BrowserExtension extension, string analysisId) : base(analysisId)
        {
            ExtensionJSFiles = [];
            TotalCount = extension.ContainedJSFiles.Count;
        }
        public ExtensionJSResult(ICollection<JSFile> files, string analysisId) : base(analysisId)
        {
            ExtensionJSFiles = [];
            TotalCount = files.Count;
        }
    }
}
