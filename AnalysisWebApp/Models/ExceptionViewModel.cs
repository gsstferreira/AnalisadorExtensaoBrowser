using System.Text.Json;

namespace AnalysisWebApp.Models
{
    public class ExceptionViewModel
    {
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string InnerException { get; set; }
        public string Source { get; set; }

        public ExceptionViewModel() 
        { 
            Message = string.Empty;
            StackTrace = string.Empty;
            InnerException = string.Empty;
            Source = string.Empty;
        }

        public ExceptionViewModel(Exception e)
        {
            Message = e.Message;
            StackTrace = e.StackTrace ?? string.Empty;
            InnerException = e.InnerException?.Message ?? string.Empty;
            Source = e.Source ?? string.Empty;
        }

        public string AsJson()
        {
            return JsonSerializer.Serialize(this);
        }

        public static ExceptionViewModel FromJson(string json)
        {
            return JsonSerializer.Deserialize<ExceptionViewModel>(json) ?? new ExceptionViewModel();
        }
    }
}
