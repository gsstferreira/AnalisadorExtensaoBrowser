using Common.ClassesJSON;
using System.Text.Json.Serialization;

namespace Common.JsonSourceGenerators
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(NpmPackageJson))]
    public partial class NpmPackageSG : JsonSerializerContext  {}
}
