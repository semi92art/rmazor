using System.Linq;
using Common;
using Common.CameraProviders;
using Common.Exceptions;
using Common.Providers;
using RMAZOR.Models;
using RMAZOR.Views.Common.CongratulationItems;
using UnityEngine;

namespace RMAZOR.Views.Common
{
    public interface IViewMazeBackground : IInit, IOnLevelStageChanged { }
    
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ViewMazeBackground : ViewMazeGroundBase, IViewMazeBackground
    {
        #region nonpublic members
        
        private Color BackgroundColor
        {
            set => CameraProvider.MainCamera.backgroundColor = value;
        }

        #endregion
        
        #region inject

        private ICameraProvider                 CameraProvider { get; }
        private IViewMazeBackgroundIdleItems    IdleItems      { get; }
        private IViewMazeBackgroundCongradItems CongratItems   { get; }

        public ViewMazeBackground(
            IModelGame                      _Model,
            ICameraProvider                 _CameraProvider,
            IColorProvider                  _ColorProvider,
            IViewMazeBackgroundIdleItems    _IdleItems,
            IViewMazeBackgroundCongradItems _CongratItems) 
            : base(_Model, _ColorProvider)
        {
            CameraProvider = _CameraProvider;
            IdleItems      = _IdleItems;
            CongratItems   = _CongratItems;
        }
        
        #endregion
        
        #region api

        public override void Init()
        {
            ColorProvider.ColorChanged += OnColorChanged;
            IdleItems.Init();
            CongratItems.Init();
            base.Init();
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            IdleItems.OnLevelStageChanged(_Args);
            CongratItems.OnLevelStageChanged(_Args);
            base.OnLevelStageChanged(_Args);
        }

        #endregion

        #region nonpublic methods
        
        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.Background)
                BackgroundColor = _Color;
        }

        protected override void SetColorsOnTheme(int _LevelIndex, EColorTheme _Theme)
        {
            GetHSV(_LevelIndex, out float h, out float s, out float v);
            switch (_Theme)
            {
                case EColorTheme.Light:
                    ColorProvider.SetColor(ColorIds.Background, Color.HSVToRGB(h, s, v));
                    break;
                case EColorTheme.Dark:
                    ColorProvider.SetColor(ColorIds.Background, Color.HSVToRGB(h, s, 5f / 100f));
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Theme);
            }
        }
        
        protected override void GetHSV(int _LevelIndex, out float _H, out float _S, out float _V)
        {
            int group = RazorMazeUtils.GetGroupIndex(_LevelIndex);
            _H = GetHForHSV(group);
            _S = 50f / 100f;
            _V = 50f / 100f;
        }
        
        #endregion
    }
}