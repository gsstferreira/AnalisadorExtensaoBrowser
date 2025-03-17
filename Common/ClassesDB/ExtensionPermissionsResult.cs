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

        public ExtensionPermissionsResult(ICollection<Permission> permissions, string id, string version) : base(id, version)
        {
            Permissions = [];
            Permissions.AddRange(permissions);
        }
    }
}
