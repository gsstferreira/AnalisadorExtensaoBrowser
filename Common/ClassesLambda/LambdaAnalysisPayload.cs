using System.Text.Json.Serialization;

namespace Common.ClassesLambda
{
    public  class LambdaAnalysisPayload
    {
        [JsonPropertyName("ExtensionPageUrl")]
        public string ExtensionPageUrl { get; set; }
        [JsonPropertyName("AnalysisId")]
        public string AnalysisId { get; set; }

        public LambdaAnalysisPayload() 
        {
            ExtensionPageUrl = string.Empty;
            AnalysisId = string.Empty;
        }
        public LambdaAnalysisPayload(string extensionPageUrl, string analysisId)
        {
            ExtensionPageUrl = extensionPageUrl;
            AnalysisId = analysisId;
        }
    }
}
