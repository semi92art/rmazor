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
    public class CurrentLevelBackgroundTexturesArgs
    {
        public Color          CurrentColor1  { get; }
        public Color          CurrentColor2  { get; }
        public Color          PreviousColor1 { get; }
        public Color          PreviousColor2 { get; }
        public Color          NextColor1     { get; }
        public Color          NextColor2     { get; }
        public BloomPropsArgs BloomPropsArgs { get; }
        
        public AdditionalColorPropsAdditionalInfo AdditionalInfo { get; }

        public CurrentLevelBackgroundTexturesArgs(
            Color                              _CurrentColor1,
            Color                              _CurrentColor2,
            Color                              _PreviousColor1,
            Color                              _PreviousColor2,
            Color                              _NextColor1,
            Color                              _NextColor2,
            BloomPropsArgs                     _BloomPropsArgs,
            AdditionalColorPropsAdditionalInfo _AdditionalInfo)
        {
            CurrentColor1  = _CurrentColor1;
            CurrentColor2  = _CurrentColor2;
            PreviousColor1 = _PreviousColor1;
            PreviousColor2 = _PreviousColor2;
            NextColor1     = _NextColor1;
            NextColor2     = _NextColor2;
            BloomPropsArgs = _BloomPropsArgs;
            AdditionalInfo = _AdditionalInfo;
        }
    }
    
    public interface IViewMazeBackgroundTextureController : IInit, IOnLevelStageChanged
    {
        public CurrentLevelBackgroundTexturesArgs GetBackgroundColorArgs();
    }

    public abstract class ViewMazeBackgroundTextureControllerBase 
        : InitBase, IViewMazeBackgroundTextureController
    {
        #region nonpublic members
        
        private IList<AdditionalColorsProps> m_AdditionalBackgorundColorsSetItems;

        protected Color
            BackCol1Current,
            BackCol2Current,
            BackCol1Prev,
            BackCol2Prev,
            BackCol1Next,
            BackCol2Next;

        private BloomPropsArgs m_BloomPropsArgs;

        protected AdditionalColorPropsAdditionalInfo AdditionalInfo;

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
            if (_Args.Args != null && _Args.Args.Contains("set_back_editor"))
                return;
            SetColorsOnNewLevel(_Args.LevelIndex);
        }

        public CurrentLevelBackgroundTexturesArgs GetBackgroundColorArgs()
        {
            return new CurrentLevelBackgroundTexturesArgs(
                BackCol1Current,
                BackCol2Current,
                BackCol1Prev,
                BackCol2Prev,
                BackCol1Next,
                BackCol2Next,
                m_BloomPropsArgs,
                AdditionalInfo);
        }

        #endregion

        #region nonpublic methods
        
        protected virtual void LoadSets()
        {
            m_AdditionalBackgorundColorsSetItems = RemoteProperties.BackAndFrontColorsSet
                .Where(_Item => _Item.inUse)
                .ToList();
            if (!m_AdditionalBackgorundColorsSetItems.NullOrEmpty())
                return;
            var additionalBackgroundColorsSet = PrefabSetManager.GetObject<AdditionalColorsSetScriptableObject>
                ("configs", "additional_colors_set");
            m_AdditionalBackgorundColorsSetItems = additionalBackgroundColorsSet.set
                .Where(_Item => _Item.inUse)
                    .ToList();
        }

        private Color GetBackgroundColor(int _ColorId, long _LevelIndex)
        {
            var colorsSet = m_AdditionalBackgorundColorsSetItems;
            int group = RmazorUtils.GetLevelsGroupIndex(_LevelIndex);
            int setItemIdx = (group - 1) % colorsSet.Count;
            if (setItemIdx < 0)
                setItemIdx = 0;
            var props = colorsSet[setItemIdx];
            return _ColorId switch
            {
                ColorIds.Background1     => props.bacground1,
                ColorIds.Background2     => props.bacground2,
                ColorIds.PathItem        => props.GetColor(props.pathItemFillType),
                ColorIds.PathBackground  => props.GetColor(props.pathBackgroundFillType),
                ColorIds.PathFill        => props.GetColor(props.pathFillFillType),
                ColorIds.Character2      => props.GetColor(props.characterBorderFillType),
                ColorIds.UiBackground    => props.GetColor(props.uiBackgroundFillType),
                ColorIds.GameUiAlternative => props.GetColor(props.uiBackgroundFillType),
                _                        => Color.magenta
            };
        }

        private void SetColorsOnNewLevel(long _LevelIndex)
        {
            var colorsSet = m_AdditionalBackgorundColorsSetItems;
            int group = RmazorUtils.GetLevelsGroupIndex(_LevelIndex);
            int setItemIdx = (group - 1) % colorsSet.Count;
            m_BloomPropsArgs = colorsSet[setItemIdx].bloom;
            AdditionalInfo = colorsSet[setItemIdx].additionalInfo;
            ColorProvider.SetColor(ColorIds.PathItem,        GetBackgroundColor(ColorIds.PathItem, _LevelIndex));
            ColorProvider.SetColor(ColorIds.PathFill,        GetBackgroundColor(ColorIds.PathFill, _LevelIndex));
            ColorProvider.SetColor(ColorIds.PathBackground,  GetBackgroundColor(ColorIds.PathBackground, _LevelIndex));
            ColorProvider.SetColor(ColorIds.Character2,      GetBackgroundColor(ColorIds.Character2, _LevelIndex));
            ColorProvider.SetColor(ColorIds.UiBackground,    GetBackgroundColor(ColorIds.UiBackground, _LevelIndex));
            ColorProvider.SetColor(ColorIds.GameUiAlternative, GetBackgroundColor(ColorIds.UiBackground, _LevelIndex));
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