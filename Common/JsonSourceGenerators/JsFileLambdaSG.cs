using Common.ClassesLambda;
using System.Text.Json.Serialization;

namespace Common.JsonSourceGenerators
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(JSFileLambdaPayload))]
    public partial class JsFileLambdaSG : JsonSerializerContext  {}
}
