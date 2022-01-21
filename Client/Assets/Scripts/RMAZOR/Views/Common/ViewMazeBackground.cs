using System.Linq;
using Common;
using Common.CameraProviders;
using RMAZOR.Models;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.Views.Common
{
    public interface IViewMazeBackground : IInit, IOnLevelStageChanged { }
    
    public class ViewMazeBackground : IViewMazeBackground
    {
        #region nonpublic members
        
        private Color BackgroundColor
        {
            set => CameraProvider.MainCamera.backgroundColor = value;
        }

        #endregion
        
        #region inject

        private ICameraProvider                 CameraProvider { get; }
        private IColorProvider                  ColorProvider  { get; }
        private IViewMazeBackgroundIdleItems    IdleItems      { get; }
        private IViewMazeBackgroundCongradItems CongradItems   { get; }

        public ViewMazeBackground(
            ICameraProvider _CameraProvider,
            IColorProvider _ColorProvider,
            IViewMazeBackgroundIdleItems _IdleItems,
            IViewMazeBackgroundCongradItems _CongradItems)
        {
            CameraProvider = _CameraProvider;
            ColorProvider = _ColorProvider;
            IdleItems = _IdleItems;
            CongradItems = _CongradItems;
        }
        
        #endregion
        
        #region api

        public bool              Initialized { get; private set; }
        public event UnityAction Initialize;

        public void Init()
        {
            ColorProvider.ColorChanged += OnColorChanged;
            IdleItems.Init();
            CongradItems.Init();
            
            Initialize?.Invoke();
            Initialized = true;
        }

        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.Background)
                BackgroundColor = _Color;
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            IdleItems.OnLevelStageChanged(_Args);
            CongradItems.OnLevelStageChanged(_Args);
            if (_Args.Stage == ELevelStage.Loaded)
            {
                SetForegroundColors(_Args.LevelIndex);
            }
        }

        #endregion

        #region nonpublic methods
        
        private void SetForegroundColors(int _LevelIndex)
        {
            int group = RazorMazeUtils.GetGroupIndex(_LevelIndex);
            float h = GetHForHSV(group);
            float s = 70f / 100f;
            float v = 100f / 100f;
            var newMainColor = Color.HSVToRGB(h, s, v);
            ColorProvider.SetColor(ColorIds.Main, newMainColor);
            ColorProvider.SetColor(ColorIds.Background, Color.HSVToRGB(h, s, 5f / 100f));
        }

        private static float GetHForHSV(int _Group)
        {
            var values = new float[]
            {
                30,  // 1
                185, // 5
                55,  // 2
                225, // 6
                80,  // 3
                265, // 7
                140, // 4
                305, // 8
                // 330  // 9
            }.Select(_H => _H / 360f).ToArray();
            int idx = _Group % values.Length;
            return values[idx];
        }

        #endregion
    }
}