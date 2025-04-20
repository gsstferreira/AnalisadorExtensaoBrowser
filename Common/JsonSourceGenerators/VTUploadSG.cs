using Common.ClassesJSON;
using System.Text.Json.Serialization;

namespace Common.JsonSourceGenerators
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(VTUploadUrlJson))]
    public partial class VTUploadSG : JsonSerializerContext  {}
}
