using Common;
using Common.Providers;
using RMAZOR.Models;
using RMAZOR.Views.Common.BackgroundIdleItems;
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

        public ViewBackground(
            IModelGame                           _Model,
            IColorProvider                       _ColorProvider,
            IViewMazeBackgroundIdleItems         _IdleItems,
            IViewMazeBackgroundCongradItems      _CongratItems,
            IViewMazeBackgroundTextureController _TextureController) 
            : base(_Model, _ColorProvider)
        {
            IdleItems            = _IdleItems;
            CongratItems         = _CongratItems;
            TextureController    = _TextureController;
        }
        
        #endregion
        
        #region api

        public override void Init()
        {
            ColorProvider.AddIgnorableForThemeSwitchColor(ColorIds.Background1);
            ColorProvider.AddIgnorableForThemeSwitchColor(ColorIds.Background2);
            IdleItems           .Init();
            CongratItems        .Init();
            TextureController   .Init();
            base.Init();
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            CongratItems        .OnLevelStageChanged(_Args);
            TextureController   .OnLevelStageChanged(_Args);
        }

        #endregion
    }
}