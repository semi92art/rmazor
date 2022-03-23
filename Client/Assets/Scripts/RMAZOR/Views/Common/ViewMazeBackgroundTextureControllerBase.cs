using System.Collections.Generic;
using System.Linq;
using Common;
using Common.CameraProviders;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using RMAZOR.Models;
using UnityEngine;

namespace RMAZOR.Views.Common
{
    public interface IViewMazeBackgroundTextureController : IInit, IOnLevelStageChanged
    {
        public void GetBackgroundColors(
            out Color _Current1, 
            out Color _Current2, 
            out Color _Previous1,
            out Color _Previous2,
            out Color _Next1,
            out Color _Next2);
    }

    public abstract class ViewMazeBackgroundTextureControllerBase 
        : InitBase, IViewMazeBackgroundTextureController
    {
        #region nonpublic members
        
        private IList<BackAndFrontColorsSetItem> m_BackAndFrontColorsSetItemsLight;

        protected Color
            BackCol1Current,
            BackCol2Current,
            BackCol1Prev,
            BackCol2Prev,
            BackCol1Next,
            BackCol2Next;

        #endregion

        #region inject
        
        protected RemoteProperties  RemoteProperties { get; }
        protected IColorProvider    ColorProvider    { get; }
        protected IPrefabSetManager PrefabSetManager { get; }

        protected ViewMazeBackgroundTextureControllerBase(
            RemoteProperties  _RemoteProperties,
            IColorProvider    _ColorProvider,
            IPrefabSetManager _PrefabSetManager)
        {
            RemoteProperties = _RemoteProperties;
            ColorProvider    = _ColorProvider;
            PrefabSetManager = _PrefabSetManager;
        }

        #endregion

        #region api
        
        public override void Init()
        {
            LoadSets();
            base.Init();
        }

        public virtual void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage != ELevelStage.Loaded)
                return;
            if (_Args.Args.Contains("set_back_editor"))
                return;
            SetColorsOnNewLevelOrNewTheme(
                _Args.LevelIndex,
                ColorProvider.CurrentTheme);
        }

        public void GetBackgroundColors(
            out Color _Current1,
            out Color _Current2,
            out Color _Previous1,
            out Color _Previous2,
            out Color _Next1, 
            out Color _Next2)
        {
            _Current1  = BackCol1Current;
            _Current2  = BackCol2Current;
            _Previous1 = BackCol1Prev;
            _Previous2 = BackCol2Prev;
            _Next1     = BackCol1Next;
            _Next2     = BackCol2Next;
        }

        #endregion

        #region nonpublic methods
        
        protected virtual void LoadSets()
        {
            m_BackAndFrontColorsSetItemsLight = RemoteProperties.BackAndFrontColorsSet;
            if (m_BackAndFrontColorsSetItemsLight.NullOrEmpty())
            {
                var backgroundColorsSetLight = PrefabSetManager.GetObject<BackAndFrontColorsSetScriptableObject>
                    ("configs", "back_and_front_colors_set_light");
                m_BackAndFrontColorsSetItemsLight = backgroundColorsSetLight.set;
            }
        }

        private Color GetBackgroundColor(int _ColorId, long _LevelIndex, EColorTheme _Theme)
        {
            var colorsSet = m_BackAndFrontColorsSetItemsLight;
            int group = RazorMazeUtils.GetGroupIndex(_LevelIndex);
            int setItemIdx = group % colorsSet.Count;
            var setItem = colorsSet[setItemIdx];
            if (_ColorId == ColorIds.Background1)
                return setItem.bacground1;
            return _ColorId == ColorIds.Background2 ? setItem.bacground2 : Color.magenta;
        }
        
        protected void SetColorsOnNewLevelOrNewTheme(
            long        _LevelIndex,
            EColorTheme _Theme)
        {
            int idx1 = ColorIds.Background1;
            int idx2 = ColorIds.Background2;
            BackCol1Current = GetBackgroundColor(idx1, _LevelIndex, _Theme);
            BackCol2Current = GetBackgroundColor(idx2, _LevelIndex, _Theme);
            BackCol1Prev    = GetBackgroundColor(idx1, _LevelIndex - 1, _Theme);
            BackCol2Prev    = GetBackgroundColor(idx2, _LevelIndex - 1, _Theme);
            BackCol1Next    = GetBackgroundColor(idx1, _LevelIndex + 1, _Theme);
            BackCol2Next    = GetBackgroundColor(idx2, _LevelIndex + 1, _Theme);
            ColorProvider.SetColor(ColorIds.Background1, BackCol1Current);
            ColorProvider.SetColor(ColorIds.Background2, BackCol2Current);
        }

        #endregion
    }
    
    public class ViewMazeBackgroundTextureControllerFake : ViewMazeBackgroundTextureControllerBase
    {
        private ICameraProvider CameraProvider { get; }

        public ViewMazeBackgroundTextureControllerFake(
            RemoteProperties  _RemoteProperties,
            IColorProvider    _ColorProvider,
            IPrefabSetManager _PrefabSetManager,
            ICameraProvider   _CameraProvider)
            : base(
                _RemoteProperties,
                _ColorProvider,
                _PrefabSetManager)
        {
            CameraProvider = _CameraProvider;
        }

        public override void Init()
        {
            ColorProvider.ColorChanged += OnColorChanged;
            base.Init();
        }

        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.Background1)
                CameraProvider.MainCamera.backgroundColor = _Color;
        }
    }
}