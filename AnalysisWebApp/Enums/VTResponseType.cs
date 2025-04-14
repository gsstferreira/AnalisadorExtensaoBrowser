namespace AnalysisWebApp.Enums
{
    public enum VTResponseType
    {
        UNDETECTED = 0,
        HARMLESS = 1,
        SUSPICIOUS = 2,
        MALICIOUS = 3,
        OTHER = 4
    }

    public static class VTResponseTypeMethods
    {
        public static string GetName(this VTResponseType type)
        {
            return type switch
            {
                VTResponseType.UNDETECTED => "Nenhuma ameaça detectada",
                VTResponseType.HARMLESS => "Considerado seguro",
                VTResponseType.SUSPICIOUS => "Suspeito de possível ameaça",
                VTResponseType.MALICIOUS => "Ameaça conhecida detectada",
                VTResponseType.OTHER => string.Empty,
                _ => "Erro na coleta de informações",
            };
        }

        public static bool IsThreat(this VTResponseType type)
        {
            switch (type)
            {
                case VTResponseType.SUSPICIOUS:
                case VTResponseType.MALICIOUS:
                    return true;
                default:
                    return false;
            }
        }
    }
}
