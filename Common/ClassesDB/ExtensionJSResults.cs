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

        public ExtensionJSResults(ICollection<JSFile> jsFiles, string id, string version) : base(id, version)
        {
            ExtensionJSFiles = [];

            foreach(var file in jsFiles)
            {
                ExtensionJSFiles.Add(new ExtensionJSFile(file));
            }
        }
    }
}
