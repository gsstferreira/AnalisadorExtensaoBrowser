using Common.Classes;

namespace Common.ClassesDB
{
    public class ExtensionJSResult: AnalysisResult
    {
        public List<ExtensionJSFile> ExtensionJSFiles { get; set; }

        public ExtensionJSResult():base() 
        {
            ExtensionJSFiles = [];
        }

        public ExtensionJSResult(BrowserExtension extension) : base(extension.Id, extension.Version)
        {
            ExtensionJSFiles = [];

            foreach(var file in extension.ContainedJSFiles)
            {
                ExtensionJSFiles.Add(new ExtensionJSFile(file));
            }
        }
    }
}
