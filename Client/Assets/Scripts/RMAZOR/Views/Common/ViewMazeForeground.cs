using Common;
using Common.Exceptions;
using Common.Providers;
using RMAZOR.Models;
using UnityEngine;

namespace RMAZOR.Views.Common
{
    public interface IViewMazeForeground : IInit, IOnLevelStageChanged { }
    
    public class ViewMazeForeground : ViewMazeGroundBase, IViewMazeForeground
    {
        #region inject
        
        public ViewMazeForeground(
            IModelGame _Model,
            IColorProvider _ColorProvider) 
            : base(_Model, _ColorProvider) { }
        
        #endregion 
        
        #region nonpublic methods

        protected override void SetColorsOnNewLevelOrNewTheme(long _LevelIndex, EColorTheme _Theme)
        {
            GetHSV(_LevelIndex, _Theme, out float h, out float s, out float v);
            ColorProvider.SetColor(ColorIds.Main, Color.HSVToRGB(h, s, v));
        }

        #endregion
        
        private static void GetHSV(
            long _LevelIndex,
            EColorTheme _Theme,
            out float _H,
            out float _S,
            out float _V)
        {     
            int group = RazorMazeUtils.GetGroupIndex(_LevelIndex);
            _H = ViewMazeBackgroundUtils.GetHForHSV(group);
            _V = 100f / 100f;
            _S = _Theme switch
            {
                EColorTheme.Light => 0f,
                EColorTheme.Dark  => 70f / 100f,
                _                 => throw new SwitchCaseNotImplementedException(_Theme)
            };
        }
    }
}