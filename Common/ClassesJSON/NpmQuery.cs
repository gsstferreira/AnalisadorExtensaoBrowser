using System.Text.Json.Serialization;

namespace Common.ClassesJSON
{
    public class NpmQueryJSON
    {
        [JsonPropertyName("objects")]
        public List<NpmRegistryJson> QueryResults { get; set; }
        [JsonPropertyName("total")]
        public int Total { get; set; }
        [JsonPropertyName("time")]
        public DateTime DateQuery { get; set; }

        public NpmQueryJSON()
        {
            QueryResults = [];
            Total = 0;
            DateQuery = DateTime.MinValue;
        }
    }
    public class NpmRegistryJson
    {
        [JsonPropertyName("package")]
        public NpmRegistryEntryJson Registry { get; set; }
        public NpmRegistryJson()
        {
            Registry = new NpmRegistryEntryJson();
        }
    }
    public class NpmRegistryEntryJson
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("version")]
        public string Version { get; set; }
        [JsonPropertyName("links")]
        public NpmLinks Links { get; set; }

        public NpmRegistryEntryJson() 
        {
            Name = string.Empty;
            Version = string.Empty;
            Links = new NpmLinks();
        }
    }
    public class NpmLinks
    {
        [JsonPropertyName("homepage")]
        public string Homepage { get; set; }
        [JsonPropertyName("repository")]
        public string Repository { get; set; }
        [JsonPropertyName("bugs")]
        public string BugTracker { get; set; }
        [JsonPropertyName("npm")]
        public string Npm { get; set; }
        public NpmLinks()
        {
            Homepage = string.Empty;
            Repository = string.Empty;
            BugTracker = string.Empty;
            Npm = string.Empty;
        }
    }
}
