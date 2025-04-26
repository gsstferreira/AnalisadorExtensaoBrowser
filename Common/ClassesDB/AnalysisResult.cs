namespace Common.ClassesDB
{
    public abstract class AnalysisResult
    {
        public string AnalysisID {get; set;}
        public DateTime DateCompletion { get; set; }
        public AnalysisResult() 
        {
            AnalysisID = string.Empty;
            DateCompletion = DateTime.MinValue;
        }
        public AnalysisResult(string analysisId) {
            DateCompletion = DateTime.Now;

            AnalysisID = analysisId;
        }
    }
}
