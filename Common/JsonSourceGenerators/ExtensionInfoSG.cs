using Common.ClassesDB;
using System.Text.Json.Serialization;

namespace Common.JsonSourceGenerators
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(ExtensionInfoResult))]
    public partial class ExtensionInfoSG : JsonSerializerContext  {}
}
