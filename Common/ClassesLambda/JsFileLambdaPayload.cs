using Common.Classes;
using Common.Handlers;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.ClassesLambda
{
    public class JSFileLambdaPayload
    {
        public string AnalysisId { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public List<NpmRegistry> NPMRegistries { get; set; }

        public JSFileLambdaPayload()
        {
            AnalysisId = string.Empty;
            Name = string.Empty;
            Content = string.Empty;
            NPMRegistries = [];
        }

        public JSFileLambdaPayload(JSFile file, string analysisId)
        {
            AnalysisId = analysisId;
            Name = file.Name;
            Content = file.Content;
            NPMRegistries = file.NPMRegistries;
        }
    }
}
