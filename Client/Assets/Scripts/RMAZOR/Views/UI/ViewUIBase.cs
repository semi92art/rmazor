using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Views.Rotation;

namespace RMAZOR.Views.UI
{
    public interface IViewUI
        : IInit, 
          IOnLevelStageChanged, 
          ICharacterMoveFinished, 
          IMazeRotationFinished
    {
        IViewUIGameControls GameControls { get; }
    }
    
    public abstract class ViewUIBase : InitBase, IViewUI
    {
        #region inject

        public IViewUIGameControls GameControls { get; }

        protected ViewUIBase(IViewUIGameControls _GameControls)
        {
            GameControls = _GameControls;
        }

        #endregion
        
        #region api

        public abstract void OnLevelStageChanged(LevelStageArgs                       _Args);
        public abstract void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args);
        public abstract void OnMazeRotationFinished(MazeRotationEventArgs _Args);

        #endregion

    }
}
