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
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            // TODO
        }
        
        #endregion

        #region nonpublic methods

        private void AppearAllMazeItems()
        {
            // TODO
        }

        private void DisappearAllMazeItems()
        {
            // TODO
        }

        #endregion



    }
}