using System.Collections.Generic;
using Common;
using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Providers;
using RMAZOR.Models;
using RMAZOR.Views.Common.CongratulationItems;

namespace RMAZOR.Views.Common
{
    public interface IViewBackground : IInit, IOnLevelStageChanged { }
    
    public class ViewBackground : InitBase, IViewBackground
    {
        #region nonpublic members

        private IList<AdditionalColorsPropsAssetItem> m_BackAndFrontColorsSetItemsLight;

        #endregion
        
        #region inject

        private IColorProvider                       ColorProvider          { get; }
        private IViewMazeBackgroundCongratItems      FireworkItems          { get; }
        private IViewMazeBackgroundTextureController TextureController      { get; }

        private ViewBackground(
            IColorProvider                       _ColorProvider,
            IViewMazeBackgroundCongratItems      _FireworkItems,
            IViewMazeBackgroundTextureController _TextureController) 
        {
            ColorProvider          = _ColorProvider;
            FireworkItems          = _FireworkItems;
            TextureController      = _TextureController;
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

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            FireworkItems    .OnLevelStageChanged(_Args);
            TextureController.OnLevelStageChanged(_Args);
        }

        #endregion
    }
}