using Common.ClassesDB;
using System.Text.Json.Serialization;

namespace Common.JsonSourceGenerators
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(ExtInfoResult))]
    public partial class ExtensionInfoSG : JsonSerializerContext  {}
}
