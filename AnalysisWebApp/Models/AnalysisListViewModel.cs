using Common.ClassesDB;

namespace AnalysisWebApp.Models
{
    public class AnalysisListViewModel
    {
        public int CurrentPostion { get; set; }
        public List<ExtInfoResult> AnalysisInfos { get; set; }

        public AnalysisListViewModel() 
        {
            AnalysisInfos = [];
            CurrentPostion = 0;
        }
        public AnalysisListViewModel(int currentPostion, ICollection<ExtInfoResult> list)  
        {
            CurrentPostion = currentPostion;
            AnalysisInfos = [.. list.OrderBy(x => x.Name).ThenByDescending(x => x.Version)];
        }
    }
}
