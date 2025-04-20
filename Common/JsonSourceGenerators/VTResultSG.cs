using Common.ClassesJSON;
using System.Text.Json.Serialization;

namespace Common.JsonSourceGenerators
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(VTResultJson))]
    public partial class VTResultSG : JsonSerializerContext  {}
}
