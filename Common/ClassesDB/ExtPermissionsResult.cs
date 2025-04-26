using Common.Classes;


namespace Common.ClassesDB
{
    public class ExtPermissionsResult : AnalysisResult
    {
        public List<Permission> Permissions { get; set; }

        public ExtPermissionsResult() : base() 
        {
            Permissions = [];
        }

        public ExtPermissionsResult(BrowserExtension extension, string analysisId) : base(analysisId)
        {
            Permissions = [];
            Permissions.AddRange(extension.Permissions);
        }
    }
}
