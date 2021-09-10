using UnityEngine.Events;

namespace Games.RazorMaze.Views.Common
{
    public class ViewMazeTransitioner : IViewMazeTransitioner
    {
        #region inject
        public IViewMazeCommon ViewMazeCommon { get; }
        
        public ViewMazeTransitioner(IViewMazeCommon _ViewMazeCommon)
        {
            ViewMazeCommon = _ViewMazeCommon;
        }
        
        #endregion
        
        #region api
        
        #endregion

        #region nonpublic methods

        public void OnBeforeLevelStarted(LevelStateChangedArgs _Args, UnityAction _StartLevel)
        {
            
        }

        public void OnLevelStarted(LevelStateChangedArgs _Args)
        {
            AppearAllMazeItems();
        }

        public void OnLevelFinished(LevelFinishedEventArgs _Args, UnityAction _Finish)
        {
            DisappearAllMazeItems();
        }

        #endregion
        
        private void AppearAllMazeItems()
        {
            // TODO
        }

        private void DisappearAllMazeItems()
        {
            // TODO
        }
    }
}