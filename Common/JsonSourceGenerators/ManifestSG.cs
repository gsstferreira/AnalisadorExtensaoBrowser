using Common.ClassesJSON;
using System.Text.Json.Serialization;

namespace Common.JsonSourceGenerators
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(ManifestJson))]
    public partial class ManifestSG : JsonSerializerContext  {}
}
