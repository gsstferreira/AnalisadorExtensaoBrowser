using Common.ClassesLambda;
using System.Text.Json.Serialization;

namespace Common.JsonSourceGenerators
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(JsQueryLambdaPayload))]
    public partial class JsQueryLambdaSG : JsonSerializerContext  {}
}
