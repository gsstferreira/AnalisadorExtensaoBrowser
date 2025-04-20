using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.ClassesLambda
{
    public class JsQueryLambdaPayload : LambdaAnalysisPayload
    {
        public int StartAt { get; set; }

        public JsQueryLambdaPayload() : base()
        {
            StartAt = 0;
        }

        public JsQueryLambdaPayload(string pageUrl, string analysisId, int startAt) : base(pageUrl, analysisId)
        {
            StartAt = startAt;
        }

        public JsQueryLambdaPayload(LambdaAnalysisPayload payload, int startAt)
        {
            ExtensionPageUrl = payload.ExtensionPageUrl;
            AnalysisId = payload.AnalysisId;
            StartAt = startAt;
        }
    }
}
