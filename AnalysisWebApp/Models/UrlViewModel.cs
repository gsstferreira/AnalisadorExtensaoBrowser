using Common.Classes;
using Common.ClassesWeb.GoogleSafeBrowsing;
using Common.Enums;
using System.Reflection;

namespace AnalysisWebApp.Models
{
    public class UrlViewModel
    {
        public string OriginalUrl { get; set; }
        public string Host { get; set; }
        public string RedirectUrl { get; set; }
        public bool IsHttps { get; set; }
        public string Threat { get; set; }
        public string UrlType { get; set; }

        public UrlViewModel() 
        {
            IsHttps = false;
            OriginalUrl = string.Empty;
            Host = string.Empty;
            RedirectUrl = string.Empty;
            Threat = string.Empty;
            UrlType = string.Empty;
        }

        public UrlViewModel(Url url)
        {
            IsHttps = url.IsHttps;
            OriginalUrl = url.OriginalUrl;
            Host = url.Host;
            RedirectUrl = url.RedirectUrl;

            Threat = url.ThreatType switch
            {
                GSBThreatType.THREAT_TYPE_UNSPECIFIED => "Risco detectado - Não especificado",
                GSBThreatType.MALWARE => "Risco detectado - Malware",
                GSBThreatType.SOCIAL_ENGINEERING => "Risco detectado - Engenharia Social/Phishing",
                GSBThreatType.UNWANTED_SOFTWARE => "Risco detectado - Software indesejado/suspeito",
                GSBThreatType.POTENTIALLY_HARMFUL_APPLICATION => "Risco detectado - Aplicação Potencialmente Nociva",
                GSBThreatType.SAFE => "Sem risco detectado",
                _ => "Análise não realizada",
            };

            UrlType = url.Type switch
            {
                Common.Enums.UrlType.PUBLIC => "Público",
                Common.Enums.UrlType.LOCAL => "Local",
                Common.Enums.UrlType.OWN_DEVICE => "Próprio dispositivo",
                _ => "Não identificato",
            };
        }
    }
}
