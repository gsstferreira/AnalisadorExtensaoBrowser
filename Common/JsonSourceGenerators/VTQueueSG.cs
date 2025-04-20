using Common.ClassesJSON;
using System.Text.Json.Serialization;

namespace Common.JsonSourceGenerators
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(VTQueueJson))]
    public partial class VTQueueSG : JsonSerializerContext  {}
}
