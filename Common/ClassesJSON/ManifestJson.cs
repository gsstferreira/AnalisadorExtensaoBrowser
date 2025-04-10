using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.ClassesJSON
{
    public class ManifestJson
    {
        [JsonPropertyName("name")]
        public string Internalname { get; set; }
        [JsonPropertyName("version")]
        public string Version { get; set; }
        [JsonPropertyName("minium_chrome_version")]
        public string MinimumChromeVersion { get; set; }
        [JsonPropertyName("permissions")]
        public List<string> Permissions { get; set; }
        [JsonPropertyName("optional_permissions")]
        public List<string> OptionalPermissions { get; set; }
        [JsonPropertyName("host_permissions")]
        public List<string> HostPermissions { get; set; }
        [JsonPropertyName("optional_host_permissions")]
        public List<string> OptionalHostPermissions { get; set; }

        public ManifestJson() 
        { 
            Internalname = string.Empty;
            Version = string.Empty;
            MinimumChromeVersion = string.Empty;
            Permissions = [];
            OptionalPermissions = [];
            HostPermissions = [];
            OptionalHostPermissions = [];
        }
    }
}
