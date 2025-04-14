namespace AnalysisWebApp.Models
{
    public class ExtensionPermissionGroup
    {
        public List<string> Permissions { get; set; }
        public List<string> OptionalPermissions { get; set; }
        public List<string> Hosts { get; set; }
        public List<string> OptionalHosts { get; set; }

        public ExtensionPermissionGroup() 
        {
            Permissions = [];
            OptionalPermissions = [];
            Hosts = [];
            OptionalHosts = [];
        }
    }
}
