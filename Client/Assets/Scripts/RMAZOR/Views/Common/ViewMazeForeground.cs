using System.Linq;
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

        protected override void SetColorsOnTheme(int _LevelIndex, EColorTheme _Theme)
        {
            GetHSV(_LevelIndex, out float h, out float s, out float v);
            switch (_Theme)
            {
                case EColorTheme.Light:
                    ColorProvider.SetColor(ColorIds.Main, Color.white);
                    break;
                case EColorTheme.Dark:
                    ColorProvider.SetColor(ColorIds.Main, Color.HSVToRGB(h, s, v));
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Theme);
            }
        }

        #endregion
        
        protected override void GetHSV(int _LevelIndex, out float _H, out float _S, out float _V)
        {     
            int group = RazorMazeUtils.GetGroupIndex(_LevelIndex);
            _H = GetHForHSV(group);
            _S = 70f / 100f;
            _V = 100f / 100f;
        }
    }
}