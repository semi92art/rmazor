using Games.RazorMaze.Views.Characters;

namespace Games.RazorMaze.Views.Common
{
    public class ViewMazeEffector : IViewMazeEffector
    {
        #region inject
        private IViewMazeCommon ViewMazeCommon { get; }
        private IViewCharacter Character { get; }

        public ViewMazeEffector(IViewMazeCommon _ViewMazeCommon, IViewCharacter _Character)
        {
            ViewMazeCommon = _ViewMazeCommon;
            Character = _Character;
        }
        
        #endregion
        
        #region api
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage == ELevelStage.Loaded)
                OnLevelLoad();
            else if (_Args.Stage == ELevelStage.Unloaded)
                OnLevelUnload();
        }
        
        #endregion

        #region nonpublic methods

        private void OnLevelLoad()
        {
            // TODO
        }

        private void OnLevelUnload()
        {
            //TODO
        }

        #endregion
    }
}