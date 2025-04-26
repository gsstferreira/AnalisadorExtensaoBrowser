using Common.Classes;

namespace Common.ClassesLambda
{
    public class JSFileLambdaPayload
    {
        public string AnalysisId { get; set; }
        public string Name { get; set; }
        public string Directory { get; set; }
        public string Content { get; set; }
        public List<NpmRegistry> NPMRegistries { get; set; }

        public JSFileLambdaPayload()
        {
            AnalysisId = string.Empty;
            Name = string.Empty;
            Directory = string.Empty;
            Content = string.Empty;
            NPMRegistries = [];
        }

        public JSFileLambdaPayload(JSFile file, string analysisId)
        {
            AnalysisId = analysisId;
            Name = file.Name;
            Directory = file.Path;
            Content = file.Content;
            NPMRegistries = file.NPMRegistries;
        }
    }
}
