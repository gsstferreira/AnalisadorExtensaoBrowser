using Common.Classes;
using Common.ClassesJSON;
using Common.Enums;
using System.Text.Json;

namespace Common.Services
{
    public class PermissionCheckService
    {
        public static void ParsePermissions(BrowserExtension extension)
        {
            var manifest = extension.CrxArchive.GetEntry("manifest.json");
            if (manifest != null)
            {
                string content = string.Empty;

                using(var reader = new StreamReader(manifest.Open()))
                {
                    content = reader.ReadToEnd();
                }

                var manifestJson = JsonSerializer.Deserialize<ManifestJson>(content) ?? new ManifestJson();
                
                foreach (var permission in manifestJson.Permissions) 
                {
                    extension.Permissions.Add(new Permission(permission, PermissionType.Permission));
                }

                foreach (var permission in manifestJson.OptionalPermissions)
                {
                    extension.Permissions.Add(new Permission(permission, PermissionType.OptionalPermission));
                }

                foreach (var permission in manifestJson.HostPermissions)
                {
                    extension.Permissions.Add(new Permission(permission, PermissionType.Host));
                }

                foreach (var permission in manifestJson.OptionalHostPermissions)
                {
                    extension.Permissions.Add(new Permission(permission, PermissionType.OptionalHost));
                }
            }
        }
    }
}
