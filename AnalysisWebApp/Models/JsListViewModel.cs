namespace AnalysisWebApp.Models
{
    public class JsListViewModel
    {
        public List<JsFileViewModel> JsFiles { get; set; }
        public int TotalCount { get; set; }

        public JsListViewModel() 
        {
            JsFiles = [];
            TotalCount = 0;
        }

    }
}
