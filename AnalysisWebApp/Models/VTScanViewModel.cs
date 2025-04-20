using AnalysisWebApp.Enums;
using Common.ClassesWeb.VirusTotal;
using Res;

namespace AnalysisWebApp.Models
{
    public class VTScanViewModel
    {
        public string EngineName { get; set; }
        public string EngineVersion { get; set; }
        public string EngineVersionDate { get; set; }
        public VTResponseType EngineResult { get; set; }

        public VTScanViewModel() 
        {
            EngineName = string.Empty;
            EngineVersion = string.Empty;
            EngineVersionDate = string.Empty;
            EngineResult = VTResponseType.OTHER;
        }

        public VTScanViewModel(AntivirusScanResult result)
        {
            EngineName = result.EngineName;
            EngineVersion = result.EngineVersion;
            EngineVersionDate = result.EngineUpdate.ToString(Params.DateStringFormat);

            EngineResult = result.Category switch
            {
                "undetected" => VTResponseType.UNDETECTED,
                "harmless" => VTResponseType.HARMLESS,
                "suspicious" => VTResponseType.SUSPICIOUS,
                "malicious" => VTResponseType.MALICIOUS,
                _ => VTResponseType.OTHER,
            };
        }
    }
}
