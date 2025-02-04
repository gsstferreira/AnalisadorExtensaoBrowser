using Common.Classes;
using Common.Enums;
using System.Text.Json.Nodes;

namespace Common.Services
{
    public class PermissionCheckService
    {
        public static void ParsePermissions(BrowserExtension extension)
        {
            var manifest = extension.CrxArchive.GetEntry("manifest.json");
            if (manifest != null)
            {
                JsonNode json;

                using (var reader = new StreamReader(manifest.Open()))
                {
                    json = JsonNode.Parse(reader.ReadToEnd());
                }

                var permissions = json["permissions"];
                var op_permissions = json["optional_permissions"];
                var host_permissions = json["host_permissions"];
                var oph_permissions = json["optional_host_permissions"];


                if (permissions != null) 
                {
                    extension.Permissions.AddRange(from JsonNode permission in permissions.AsArray()
                                                   let value = permission.ToString().Trim()
                                                   where !string.IsNullOrEmpty(value)
                                                   select new Permission(value, PermissionType.Permission));
                }

                if (op_permissions != null) 
                {
                    op_permissions = json["optional_permissions"].AsArray();
                    extension.Permissions.AddRange(from JsonNode permission in op_permissions.AsArray()
                                                   let value = permission.ToString().Trim()
                                                   where !string.IsNullOrEmpty(value)
                                                   select new Permission(value, PermissionType.OptionalPermission));
                }

                if (host_permissions != null) 
                {
                    host_permissions = json["host_permissions"].AsArray();
                    extension.Permissions.AddRange(from JsonNode permission in host_permissions.AsArray()
                                                   let value = permission.ToString().Trim()
                                                   where !string.IsNullOrEmpty(value)
                                                   select new Permission(value, PermissionType.Host));
                }

                if (oph_permissions != null) 
                {
                    oph_permissions = json["optional_host_permissions"].AsArray();
                    extension.Permissions.AddRange(from JsonNode permission in oph_permissions.AsArray()
                                                   let value = permission.ToString().Trim()
                                                   where !string.IsNullOrEmpty(value)
                                                   select new Permission(value, PermissionType.OptionalHost));
                }
            }
        }
    }
}
