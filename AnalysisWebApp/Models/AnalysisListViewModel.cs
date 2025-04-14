using Common.ClassesDB;

namespace AnalysisWebApp.Models
{
    public class AnalysisListViewModel
    {
        public int CurrentPostion { get; set; }
        public List<ExtensionInfoResult> AnalysisInfos { get; set; }

        public AnalysisListViewModel() 
        {
            AnalysisInfos = [];
            CurrentPostion = 0;
        }
        public AnalysisListViewModel(int currentPostion, ICollection<ExtensionInfoResult> list)  
        {
            CurrentPostion = currentPostion;
            AnalysisInfos = [.. list];
        }
    }
}
