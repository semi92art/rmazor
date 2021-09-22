using Ticker;

namespace Games.RazorMaze.Views.UI
{
    public class ViewUIProt : ViewUIBase
    {
        public ViewUIProt(IUITicker _UITicker) : base(_UITicker) { }
        public override void OnLevelStageChanged(LevelStageArgs _Args) { }
    }
}