using UnityEngine.Events;

namespace Games.RazorMaze.Views.Common
{
    public interface IOnLevelStaging
    {
        void OnBeforeLevelStarted(LevelStateChangedArgs _Args, UnityAction _StartLevel);
        void OnLevelStarted(LevelStateChangedArgs _Args);
        void OnLevelFinished(LevelFinishedEventArgs _Args, UnityAction _Finish);
    }
}