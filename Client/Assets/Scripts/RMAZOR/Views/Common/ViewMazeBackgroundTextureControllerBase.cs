using System.Collections.Generic;
using System.Linq;
using Common;
using Common.CameraProviders;
using Common.CameraProviders.Camera_Effects_Props;
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
            out Color _Next2,
            out BloomPropsAlt _BloomProps);
    }

    public abstract class ViewMazeBackgroundTextureControllerBase 
        : InitBase, IViewMazeBackgroundTextureController
    {
        #region nonpublic members
        
        private IList<AdditionalColorsProps> m_BackAndFrontColorsSetItemsLight;

        protected Color
            BackCol1Current,
            BackCol2Current,
            BackCol1Prev,
            BackCol2Prev,
            BackCol1Next,
            BackCol2Next;

        private BloomPropsAlt m_BloomPropsAlt;

        #endregion

        #region inject
        
        protected IRemotePropertiesRmazor RemoteProperties { get; }
        protected IColorProvider          ColorProvider    { get; }
        protected IPrefabSetManager       PrefabSetManager { get; }

        protected ViewMazeBackgroundTextureControllerBase(
            IRemotePropertiesRmazor _RemoteProperties,
            IColorProvider          _ColorProvider,
            IPrefabSetManager       _PrefabSetManager)
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
            if (_Args.LevelStage != ELevelStage.Loaded)
                return;
            if (_Args.Args.Contains("set_back_editor"))
                return;
            SetColorsOnNewLevel(_Args.LevelIndex);
        }

        public void GetBackgroundColors(
            out Color         _Current1,
            out Color         _Current2,
            out Color         _Previous1,
            out Color         _Previous2,
            out Color         _Next1,
            out Color         _Next2,
            out BloomPropsAlt _BloomProps)
        {
            _Current1   = BackCol1Current;
            _Current2   = BackCol2Current;
            _Previous1  = BackCol1Prev;
            _Previous2  = BackCol2Prev;
            _Next1      = BackCol1Next;
            _Next2      = BackCol2Next;
            _BloomProps = m_BloomPropsAlt;
        }

        #endregion

        #region nonpublic methods
        
        protected virtual void LoadSets()
        {
            m_BackAndFrontColorsSetItemsLight = RemoteProperties.BackAndFrontColorsSet
                .Where(_Item => _Item.inUse)
                .ToList();
            if (!m_BackAndFrontColorsSetItemsLight.NullOrEmpty())
                return;
            var backgroundColorsSetLight = PrefabSetManager.GetObject<AdditionalColorsSetScriptableObject>
                ("configs", "additional_colors_set");
            m_BackAndFrontColorsSetItemsLight = backgroundColorsSetLight.set
                .Where(_Item => _Item.inUse)
                    .ToList();
        }

        private Color GetBackgroundColor(int _ColorId, long _LevelIndex)
        {
            var colorsSet = m_BackAndFrontColorsSetItemsLight;
            int group = RmazorUtils.GetGroupIndex(_LevelIndex);
            int setItemIdx = group % colorsSet.Count;
            var props = colorsSet[setItemIdx];
            return _ColorId switch
            {
                ColorIds.Background1    => props.bacground1,
                ColorIds.Background2    => props.bacground2,
                ColorIds.PathItem       => props.GetColor(props.pathItemFillType),
                ColorIds.PathBackground => props.GetColor(props.pathBackgroundFillType),
                ColorIds.PathFill       => props.GetColor(props.pathFillFillType),
                ColorIds.Character2     => props.GetColor(props.characterBorderFillType),
                ColorIds.UiBackground   => props.GetColor(props.uiBackgroundFillType).SetA(0.7f),
                _                       => Color.magenta
            };
        }

        private void SetColorsOnNewLevel(long _LevelIndex)
        {
            ColorProvider.SetColor(ColorIds.PathItem, GetBackgroundColor(ColorIds.PathItem, _LevelIndex));
            ColorProvider.SetColor(ColorIds.PathFill, GetBackgroundColor(ColorIds.PathFill, _LevelIndex));
            ColorProvider.SetColor(ColorIds.PathBackground, GetBackgroundColor(ColorIds.PathBackground, _LevelIndex));
            ColorProvider.SetColor(ColorIds.Character2,  GetBackgroundColor(ColorIds.Character2, _LevelIndex));
            ColorProvider.SetColor(ColorIds.UiBackground,  GetBackgroundColor(ColorIds.UiBackground, _LevelIndex));
            const int idx1 = ColorIds.Background1;
            const int idx2 = ColorIds.Background2;
            BackCol1Current = GetBackgroundColor(idx1, _LevelIndex);
            BackCol2Current = GetBackgroundColor(idx2, _LevelIndex);
            BackCol1Prev = GetBackgroundColor(idx1, _LevelIndex - 1);
            BackCol2Prev = GetBackgroundColor(idx2, _LevelIndex - 1);
            BackCol1Next = GetBackgroundColor(idx1, _LevelIndex + 1);
            BackCol2Next = GetBackgroundColor(idx2, _LevelIndex + 1);
            ColorProvider.SetColor(ColorIds.Background1, BackCol1Current);
            ColorProvider.SetColor(ColorIds.Background2, BackCol2Current);
            
            var colorsSet = m_BackAndFrontColorsSetItemsLight;
            int group = RmazorUtils.GetGroupIndex(_LevelIndex);
            int setItemIdx = group % colorsSet.Count;
            m_BloomPropsAlt = colorsSet[setItemIdx].bloom;
        }

        #endregion
    }
    
    public class ViewMazeBackgroundTextureControllerFake : ViewMazeBackgroundTextureControllerBase
    {
        private ICameraProvider CameraProvider { get; }

        public ViewMazeBackgroundTextureControllerFake(
            IRemotePropertiesRmazor _RemoteProperties,
            IColorProvider          _ColorProvider,
            IPrefabSetManager       _PrefabSetManager,
            ICameraProvider         _CameraProvider)
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
                CameraProvider.Camera.backgroundColor = _Color;
        }
    }
}