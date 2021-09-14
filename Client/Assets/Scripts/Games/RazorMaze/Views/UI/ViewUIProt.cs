using Ticker;
using UnityEngine.Events;

namespace Games.RazorMaze.Views.UI
{
    public class ViewUIProt : ViewUIBase
    {
        #region inject

        public ViewUIProt(ITicker _Ticker) : base(_Ticker)
        { }
        
        #endregion
        
        #region api
        
        public override void OnBeforeLevelStarted(LevelStateChangedArgs _Args, UnityAction _StartLevel)
        {
            // TODO
        }

        public override void OnLevelStarted(LevelStateChangedArgs _Args)
        {
            // TODO
        }

        public override void OnLevelFinished(LevelFinishedEventArgs _Args, UnityAction _Finish)
        {
            // TODO
        }
        
        #endregion
    }
}