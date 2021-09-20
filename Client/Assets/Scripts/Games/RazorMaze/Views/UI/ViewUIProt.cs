using Ticker;

namespace Games.RazorMaze.Views.UI
{
    public class ViewUIProt : ViewUIBase
    {
        public ViewUIProt(ITicker _Ticker) : base(_Ticker) { }
        public override void OnLevelStageChanged(LevelStageArgs _Args) { }
    }
}