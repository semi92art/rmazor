using Common;
using Common.Providers;
using RMAZOR.Models;
using RMAZOR.Views.Common.CongratulationItems;

namespace RMAZOR.Views.Common
{
    public interface IViewBackground : IInit, IOnLevelStageChanged { }
    
    public class ViewBackground : ViewMazeGroundBase, IViewBackground
    {
        #region inject

        private IViewMazeBackgroundIdleItems         IdleItems            { get; }
        private IViewMazeBackgroundCongradItems      CongratItems         { get; }
        private IViewMazeBackgroundTextureController TextureController    { get; }
        // private IViewMazeAdditionalBackground        AdditionalBackground { get; }

        public ViewBackground(
            IModelGame                           _Model,
            IColorProvider                       _ColorProvider,
            IViewMazeBackgroundIdleItems         _IdleItems,
            IViewMazeBackgroundCongradItems      _CongratItems,
            IViewMazeBackgroundTextureController _TextureController,
            IViewMazeAdditionalBackground        _AdditionalBackground) 
            : base(_Model, _ColorProvider)
        {
            IdleItems            = _IdleItems;
            CongratItems         = _CongratItems;
            TextureController    = _TextureController;
            // AdditionalBackground = _AdditionalBackground;
        }
        
        #endregion
        
        #region api

        public override void Init()
        {
            IdleItems           .Init();
            CongratItems        .Init();
            TextureController   .Init();
            // AdditionalBackground.Init();
            base.Init();
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            IdleItems           .OnLevelStageChanged(_Args);
            CongratItems        .OnLevelStageChanged(_Args);
            TextureController   .OnLevelStageChanged(_Args);
            // AdditionalBackground.OnLevelStageChanged(_Args);
        }

        #endregion
    }
}