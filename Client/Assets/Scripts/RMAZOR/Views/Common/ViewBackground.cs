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

        private IViewMazeBackgroundCongradItems      FireworkItems     { get; }
        private IViewMazeBackgroundTextureController TextureController { get; }

        public ViewBackground(
            IColorProvider                       _ColorProvider,
            IViewMazeBackgroundCongradItems      _FireworkItems,
            IViewMazeBackgroundTextureController _TextureController) 
            : base(_ColorProvider)
        {
            FireworkItems     = _FireworkItems;
            TextureController = _TextureController;
        }
        
        #endregion
        
        #region api

        public override void Init()
        {
            ColorProvider.AddIgnorableForThemeSwitchColor(ColorIds.Background1);
            ColorProvider.AddIgnorableForThemeSwitchColor(ColorIds.Background2);
            FireworkItems    .Init();
            TextureController.Init();
            base.Init();
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            FireworkItems    .OnLevelStageChanged(_Args);
            TextureController.OnLevelStageChanged(_Args);
        }

        #endregion
    }
}