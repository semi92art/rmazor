using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;
using RMAZOR.Models;

namespace RMAZOR.Views.Common.CongratulationItems
{
    public interface IViewMazeBackgroundCongratItems: IInit, IOnLevelStageChanged { }
    
    public class ViewMazeBackgroundCongratItemsFake : InitBase, IViewMazeBackgroundCongratItems
    {
        public void OnLevelStageChanged(LevelStageArgs _Args) { }
    }
}