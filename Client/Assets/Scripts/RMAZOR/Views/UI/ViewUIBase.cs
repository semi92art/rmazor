using Common;
using Common.Helpers;
using RMAZOR.Models;

namespace RMAZOR.Views.UI
{
    public interface IViewUI : IInit, IOnLevelStageChanged
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

        public abstract void OnLevelStageChanged(LevelStageArgs _Args);

        #endregion
    }
}
