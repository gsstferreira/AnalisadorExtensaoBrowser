using System.Text.Json.Serialization;

namespace Common.ClassesJSON
{
    public class NpmPackageJson
    {
        [JsonPropertyName("time")]
        public Dictionary<string,DateTime> DateVersions { get; set; }
        [JsonPropertyName("versions")]
        public SortedList<string, PackageVersionJson> Versions { get; set; }
        [JsonPropertyName("dist-tags")]
        public DistributionTagsJson DistributionTags { get; set; }

        public NpmPackageJson() 
        {
            DateVersions = [];
            Versions = [];
            DistributionTags = new DistributionTagsJson();
        }
    }
    public class DistributionTagsJson
    {
        [JsonPropertyName("latest")]
        public string LatestVersionStable { get; set; }
        [JsonPropertyName("beta")]
        public string LatestVersionDevelopment { get; set; }

        public DistributionTagsJson()
        {
            LatestVersionStable = string.Empty;
            LatestVersionDevelopment = string.Empty;
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
        [JsonPropertyName("tarball")]
        public string TarballUrl { get; set; }

        public VersionDistribution() 
        {
            TarballUrl = string.Empty;
        }
    }
}
