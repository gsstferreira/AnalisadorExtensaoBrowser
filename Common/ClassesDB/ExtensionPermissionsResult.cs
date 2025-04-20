using Common.Classes;
using System.Collections.ObjectModel;


namespace Common.ClassesDB
{
    public class ExtensionPermissionsResult : AnalysisResult
    {
        public List<Permission> Permissions { get; set; }

        public ExtensionPermissionsResult() : base() 
        {
            Permissions = [];
        }

        public ExtensionPermissionsResult(BrowserExtension extension, string analysisId) : base(analysisId)
        {
            Permissions = [];
            Permissions.AddRange(extension.Permissions);
        }
    }
}
