using Entities;
using Ticker;

namespace Games.RazorMaze.Views.UI
{
    public class ViewUI : ViewUIBase
    {
        public ViewUI(IGameObservable _GameObservable, IUITicker _UITicker) 
            : base(_GameObservable, _UITicker) { }
        
        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            // TODO
        }
    }
}