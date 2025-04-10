using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ThirdParty.Json.LitJson;

namespace Common.ClassesJSON
{
    public class NpmPackageJson
    {
        [JsonPropertyName("time")]
        public SortedList<string,DateTime> DateVersions { get; set; }
        [JsonPropertyName("versions")]
        public SortedList<string, PackageVersionJson> Versions { get; set; }

        public NpmPackageJson() 
        {
            DateVersions = [];
            Versions = [];
        }
    }

    public class PackageVersionJson
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("dist")]
        public VersionDistribution DstributionDetails { get; set; }

        public PackageVersionJson()
        {
            Name = string.Empty;
            DstributionDetails = new VersionDistribution();
        }
    }

    public class VersionDistribution
    {
        [JsonPropertyName("shasum")]
        public string Shasum { get; set; }
        [JsonPropertyName("tarball")]
        public string TarballUrl { get; set; }

        public VersionDistribution() 
        {
            Shasum = string.Empty;
            TarballUrl = string.Empty;
        }
    }
}
