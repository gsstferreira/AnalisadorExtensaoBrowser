using Common.ClassesWeb.VirusTotal;
using Res;

namespace AnalysisWebApp.Models
{
    public class VirusTotalViewModel
    {
        public bool IsComplete { get; set; }
        public string DateCompletion { get; set; }
        public List<VTScanViewModel> EngineResults { get; set; }
        public VirusTotalViewModel() 
        {
            EngineResults = [];
            IsComplete = false;
            DateCompletion = string.Empty;
        }

        public VirusTotalViewModel(VTResponse response)
        {
            IsComplete = response.Status.Equals("completed");
            DateCompletion = response.Date.ToString(Params.DateStringFormat);
            EngineResults = [];

            foreach(var scan in response.ScanResults)
            {
                var model = new VTScanViewModel(scan);

                if(model.EngineResult != Enums.VTResponseType.OTHER)
                {
                    EngineResults.Add(model);
                }
            }

            EngineResults = [.. EngineResults.OrderBy(r => r.EngineResult).ThenBy(r => r.EngineName)];
        }
    }
}
