using Common.ClassesJSON;
using System.Text.Json.Serialization;

namespace Common.JsonSourceGenerators
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(NpmQueryJSON))]
    public partial class NpmQuerySG : JsonSerializerContext  {}
}
