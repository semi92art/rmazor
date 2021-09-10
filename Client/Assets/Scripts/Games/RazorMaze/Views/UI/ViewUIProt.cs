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
        
        public override void OnBeforeLevelStarted(LevelStateChangedArgs _Args, UnityAction _StartLevel)
        {
            throw new System.NotImplementedException();
        }

        public override void OnLevelStarted(LevelStateChangedArgs _Args)
        {
            throw new System.NotImplementedException();
        }

        public override void OnLevelFinished(LevelStateChangedArgs _Args, UnityAction _Finish)
        {
            throw new System.NotImplementedException();
        }
    }
}