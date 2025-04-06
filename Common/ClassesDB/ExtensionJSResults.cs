using Common.Classes;

namespace Common.ClassesDB
{
    public class ExtensionJSResults: AnalysisResult
    {
        public List<ExtensionJSFile> ExtensionJSFiles { get; set; }

        public ExtensionJSResults():base() 
        {
            ExtensionJSFiles = [];
        }

        public ExtensionJSResults(BrowserExtension extension) : base(extension.ID, extension.Version)
        {
            ExtensionJSFiles = [];

            foreach(var file in extension.ContainedJSFiles)
            {
                ExtensionJSFiles.Add(new ExtensionJSFile(file));
            }
        }
    }
}
