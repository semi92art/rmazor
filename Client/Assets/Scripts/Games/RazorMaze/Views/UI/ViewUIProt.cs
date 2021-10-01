using Entities;
using Ticker;

namespace Games.RazorMaze.Views.UI
{
    public class ViewUIProt : ViewUIBase
    {
        public ViewUIProt(IManagersGetter _Managers,  IUITicker _UITicker) 
            : base(_Managers, _UITicker) { }
        public override void OnLevelStageChanged(LevelStageArgs _Args) { }
    }
}