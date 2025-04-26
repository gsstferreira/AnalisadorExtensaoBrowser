using System.Text.Json.Serialization;

namespace Common.ClassesJSON
{
    public class VTUploadUrlJson
    {
        [JsonPropertyName("data")]
        public string UploadUrl { get; set; }

        public VTUploadUrlJson() 
        {
            UploadUrl = string.Empty;
        }
    }
}
