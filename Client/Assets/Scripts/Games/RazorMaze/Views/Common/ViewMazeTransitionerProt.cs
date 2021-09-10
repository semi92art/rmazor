using UnityEngine.Events;

namespace Games.RazorMaze.Views.Common
{
    public class ViewMazeTransitionerProt : IViewMazeTransitioner
    {
        public void OnBeforeLevelStarted(LevelStateChangedArgs _Args, UnityAction _StartLevel)
        { }

        public void OnLevelStarted(LevelStateChangedArgs _Args)
        { }

        public void OnLevelFinished(LevelFinishedEventArgs _Args, UnityAction _Finish)
        { }
    }
}