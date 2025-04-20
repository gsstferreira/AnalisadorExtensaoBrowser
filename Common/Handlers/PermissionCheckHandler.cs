using Common.Classes;
using Common.ClassesJSON;
using Common.Enums;
using Common.JsonSourceGenerators;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Common.Handlers
{
    public class PermissionCheckHandler
    {
        public static void ParsePermissions(BrowserExtension extension)
        {
            var manifest = extension.ExtensionContent.GetEntry("manifest.json");
            if (manifest is not null)
            {
                string content = string.Empty;

                using(var reader = new StreamReader(manifest.Open()))
                {
                    content = reader.ReadToEnd();
                }

                var manifestJson = JsonSerializer.Deserialize(content, ManifestSG.Default.ManifestJson) ?? new ManifestJson();
                
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
