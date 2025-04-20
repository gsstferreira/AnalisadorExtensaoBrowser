using Common.ClassesWeb.GoogleSafeBrowsing;
using System.Text.Json.Serialization;

namespace Common.JsonSourceGenerators
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(GSBResponse))]
    public partial class GSBResponseSG : JsonSerializerContext  {}
}
