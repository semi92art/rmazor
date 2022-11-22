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
            object setBackgroundFromEditorArg = _Args.Args.GetSafe(
                CommonInputCommandArg.KeySetBackgroundFromEditor, out bool keyExist);
            if (keyExist && (bool)setBackgroundFromEditorArg)
                return;
            SetColorsOnNewLevel(_Args);
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



        private void SetColorsOnNewLevel(LevelStageArgs _Args)
        {
            var setItemCurrent = GetAdditionalColorsSetItemIndexForLevel(_Args);
            var setItemPrev = GetAdditionalColorsSetItemIndexForLevel(_Args, true);
            var setItemNext = GetAdditionalColorsSetItemIndexForLevel(_Args, _ForNextLevel: true);
            m_BloomPropsArgs = setItemCurrent.bloom;
            AdditionalInfo = setItemCurrent.additionalInfo;
            ColorProvider.SetColor(ColorIds.PathItem,          GetBackgroundColor(ColorIds.PathItem,       setItemCurrent));
            ColorProvider.SetColor(ColorIds.PathFill,          GetBackgroundColor(ColorIds.PathFill,       setItemCurrent));
            ColorProvider.SetColor(ColorIds.PathBackground,    GetBackgroundColor(ColorIds.PathBackground, setItemCurrent));
            ColorProvider.SetColor(ColorIds.Character2,        GetBackgroundColor(ColorIds.Character2,     setItemCurrent));
            ColorProvider.SetColor(ColorIds.UiBackground,      GetBackgroundColor(ColorIds.UiBackground,   setItemCurrent));
            ColorProvider.SetColor(ColorIds.GameUiAlternative, GetBackgroundColor(ColorIds.UiBackground,   setItemCurrent));
            const int idx1 = ColorIds.Background1;
            const int idx2 = ColorIds.Background2;
            BackCol1Current = GetBackgroundColor(idx1, setItemCurrent);
            BackCol2Current = GetBackgroundColor(idx2, setItemCurrent);
            BackCol1Prev = GetBackgroundColor(idx1, setItemPrev);
            BackCol2Prev = GetBackgroundColor(idx2, setItemPrev);
            BackCol1Next = GetBackgroundColor(idx1, setItemNext);
            BackCol2Next = GetBackgroundColor(idx2, setItemNext);
            ColorProvider.SetColor(ColorIds.Background1, BackCol1Current);
            ColorProvider.SetColor(ColorIds.Background2, BackCol2Current);
        }

        private AdditionalColorsProps GetAdditionalColorsSetItemIndexForLevel(
            LevelStageArgs _Args,
            bool           _ForPreviousLevel = false,
            bool           _ForNextLevel     = false)
        {
            string levelType = (string) _Args.Args.GetSafe(CommonInputCommandArg.KeyNextLevelType, out _);
            bool isBonusLevel = levelType == CommonInputCommandArg.ParameterLevelTypeBonus;
            int levelIndex = (int)_Args.LevelIndex;
            if (_ForPreviousLevel && !isBonusLevel)
                levelIndex = (int) _Args.LevelIndex - 1;
            else if (_ForNextLevel && !isBonusLevel)
                levelIndex = (int) _Args.LevelIndex + 1;
            int setItemIdx = isBonusLevel ? levelIndex : RmazorUtils.GetLevelsGroupIndex(levelIndex) - 1;
            setItemIdx %= m_AdditionalBackgorundColorsSetItems.Count;
            if (setItemIdx < 0) setItemIdx = 0;
            var colorsSet = m_AdditionalBackgorundColorsSetItems[setItemIdx];
            return colorsSet;
        }
        
        private static Color GetBackgroundColor(int _ColorId, AdditionalColorsProps _Props)
        {
            return _ColorId switch
            {
                ColorIds.Background1       => _Props.bacground1,
                ColorIds.Background2       => _Props.bacground2,
                ColorIds.PathItem          => _Props.GetColor(_Props.pathItemFillType),
                ColorIds.PathBackground    => _Props.GetColor(_Props.pathBackgroundFillType),
                ColorIds.PathFill          => _Props.GetColor(_Props.pathFillFillType),
                ColorIds.Character2        => _Props.GetColor(_Props.characterBorderFillType),
                ColorIds.UiBackground      => _Props.GetColor(_Props.uiBackgroundFillType),
                ColorIds.GameUiAlternative => _Props.GetColor(_Props.uiBackgroundFillType),
                _                          => Color.magenta
            };
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