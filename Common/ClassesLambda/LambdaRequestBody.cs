using Common.Classes;

namespace Common.ClassesLambda
{
    public class LambdaRequestBody
    {
        public string ExtensionPageUrl { get; set; }
        public string ExtensionId { get; set; }
        public string ExtensionVersion { get; set; }
        public LambdaRequestBody()
        {
            ExtensionPageUrl = string.Empty;
            ExtensionId = string.Empty;
            ExtensionVersion = string.Empty;
        }
        public LambdaRequestBody(BrowserExtension extension)
        {
            ExtensionPageUrl = extension.PageUrl;
            ExtensionId = extension.Id;
            ExtensionVersion = extension.Version;
        }
    }
}
