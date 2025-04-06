using Common.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
            ExtensionId = extension.ID;
            ExtensionVersion = extension.Version;
        }
    }
}
