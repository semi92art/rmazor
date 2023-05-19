using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Common.Constants;
using Common.Helpers;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.CameraProviders.Camera_Effects_Props;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Settings;
using RMAZOR.Views.Utils;
using UnityEngine;
using static RMAZOR.Models.ComInComArg;
using static Common.ColorIds;

namespace RMAZOR.Views.Common
{
    public class CurrentLevelBackgroundTexturesArgs
    {
        public BloomPropsArgs                     BloomPropsArgs { get; }
        public AdditionalColorPropsAdditionalInfo AdditionalInfo { get; }

        public CurrentLevelBackgroundTexturesArgs(
            BloomPropsArgs                     _BloomPropsArgs,
            AdditionalColorPropsAdditionalInfo _AdditionalInfo)
        {
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
        
        private IList<AdditionalColorsPropsAssetItem> m_ColorPropsList;

        protected Color
            BackCol1Current,
            BackCol2Current,
            BackCol1Prev,
            BackCol2Prev;

        private BloomPropsArgs m_BloomPropsArgs;

        protected AdditionalColorPropsAdditionalInfo AdditionalInfo;

        private int  m_MainModeBonusLevelsColorPropsIndex;
        private bool m_IsRetroMode;

        #endregion

        #region inject

        private   GlobalGameSettings      GlobalGameSettings { get; }
        private   ViewSettings            ViewSettings       { get; }
        protected IRemotePropertiesRmazor RemoteProperties   { get; }
        private   IModelGame              Model              { get; }
        protected IColorProvider          ColorProvider      { get; }
        protected IPrefabSetManager       PrefabSetManager   { get; }
        private   IRetroModeSetting       RetroModeSetting   { get; }

        protected ViewMazeBackgroundTextureControllerBase(
            GlobalGameSettings      _GlobalGameSettings,
            ViewSettings            _ViewSettings,
            IRemotePropertiesRmazor _RemoteProperties,
            IModelGame              _Model,
            IColorProvider          _ColorProvider,
            IPrefabSetManager       _PrefabSetManager,
            IRetroModeSetting       _RetroModeSetting)
        {
            GlobalGameSettings = _GlobalGameSettings;
            ViewSettings       = _ViewSettings;
            RemoteProperties   = _RemoteProperties;
            Model              = _Model;
            ColorProvider      = _ColorProvider;
            PrefabSetManager   = _PrefabSetManager;
            RetroModeSetting   = _RetroModeSetting;
        }

        #endregion

        #region api
        
        public override void Init()
        {
            RetroModeSetting.ValueSet += OnRetroModeSettingValueSet;
            LoadSets();
            base.Init();
        }

        public virtual void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.LevelStage != ELevelStage.Loaded)
                return;
            object setBackgroundFromEditorArg = _Args.Arguments.GetSafe(
                KeySetBackgroundFromEditor, out bool keyExist);
            if (keyExist && (bool)setBackgroundFromEditorArg)
                return;
            SetColors(_Args);
        }

        public CurrentLevelBackgroundTexturesArgs GetBackgroundColorArgs()
        {
            return new CurrentLevelBackgroundTexturesArgs(
                m_BloomPropsArgs,
                AdditionalInfo);
        }

        #endregion

        #region nonpublic methods
        
        private void OnRetroModeSettingValueSet(bool _Value)
        {
            var levelStageArgs = Model.LevelStaging.GetCurrentLevelStageArguments();
            if (levelStageArgs.LevelStage == ELevelStage.None)
                return;
            SetColors(levelStageArgs);
            ActivateAndShowBackgroundTexture(levelStageArgs);
        }
        
        protected virtual void LoadSets()
        {
            m_ColorPropsList = RemoteProperties.BackAndFrontColorsSet
                .Where(_Item => _Item.inUse)
                .ToList();
            if (!m_ColorPropsList.NullOrEmpty())
                return;
            var additionalBackgroundColorsSet = PrefabSetManager.GetObject<AdditionalColorsSetScriptableObject>(
                CommonPrefabSetNames.Configs, "additional_colors_set");
            m_ColorPropsList = additionalBackgroundColorsSet.set
                .Where(_Item => _Item.inUse)
                .ToList();
        }

        private void SetColors(LevelStageArgs _Args)
        {
            if (_Args.LevelStage == ELevelStage.None)
                return;
            var setItemCurrent = GetAdditionalColorsSetItemIndexForLevel(_Args);
            BackCol1Current    = GetAdditionalColor(Background1, setItemCurrent);
            BackCol2Current    = GetAdditionalColor(Background2, setItemCurrent);
            var colorIdsSetItemCurrent = new[]
                {Main, PathItem, PathFill, PathBackground, UiBackground, GameUiAlternative, Background1, Background2};
            foreach (int colorId in colorIdsSetItemCurrent)
                ColorProvider.SetColor(colorId, GetAdditionalColor(colorId, setItemCurrent));
            var setItemPrev = GetAdditionalColorsSetItemIndexForLevel(_Args, true);
            BackCol1Prev = GetAdditionalColor(Background1, setItemPrev);
            BackCol2Prev = GetAdditionalColor(Background2, setItemPrev);
            m_BloomPropsArgs = setItemCurrent.bloom;
            AdditionalInfo = setItemCurrent.additionalInfo;
            ColorProvider.SetColor(MazeItem1, RetroModeSetting.Get()
                ? Color.red
                : Color.white);
        }

        private AdditionalColorsPropsAssetItem GetAdditionalColorsSetItemIndexForLevel(
            LevelStageArgs _Args,
            bool           _ForPreviousLevel = false,
            bool           _ForNextLevel     = false)
        {
            string gameMode = (string) _Args.Arguments[KeyGameMode];
            return gameMode switch
            {
                ParameterGameModeMain =>
                    GetAdditionalColorPropsForGameModeMain(_Args, _ForPreviousLevel, _ForNextLevel),
                ParameterGameModeRandom         => GetAdditionalColorPropsForGameModeRandom(),
                ParameterGameModeDailyChallenge => GetAdditionalColorPropsForGameModeDailyChallenge(),
                ParameterGameModePuzzles        => GetAdditionalColorPropsForGameModePuzzles(_Args),
                ParameterGameModeBigLevels      => GetAdditionalColorPropsForGameModeBigLevels(),
                _                               => throw new SwitchExpressionException(gameMode)
            };
        }

        private AdditionalColorsPropsAssetItem GetAdditionalColorPropsForGameModeMain(
            LevelStageArgs _Args,
            bool           _ForPreviousLevel,
            bool           _ForNextLevel)
        {
            if (RetroModeSetting.Get())
                return GetAdditionalColorPropsForGameModeRandom();
            string levelType = ViewLevelStageSwitcherUtils.GetNextLevelType(_Args.Arguments);
            if (levelType == ParameterLevelTypeBonus)
                return GetAdditionalColorPropsForGameModeMainBonusLevels(_Args);
            var propsForLevelTypeMain = m_ColorPropsList
                .Where(_Props => !_Props.additionalInfo.arguments.Contains("disable_for_main"))
                .ToList();
            bool isBonusLevel = levelType == ParameterLevelTypeBonus;
            int levelIndex = (int)_Args.LevelIndex;
            if (_ForPreviousLevel && !isBonusLevel)
                levelIndex = (int) _Args.LevelIndex - 1;
            else if (_ForNextLevel && !isBonusLevel)
                levelIndex = (int) _Args.LevelIndex + 1;
            int setItemIdx = isBonusLevel ?
                levelIndex * GlobalGameSettings.extraLevelEveryNStage - GlobalGameSettings.extraLevelFirstStage
                : RmazorUtils.GetLevelsGroupIndex(levelIndex) - 1;
            setItemIdx %= propsForLevelTypeMain.Count;
            if (setItemIdx < 0) setItemIdx = 0;
            var colorsSet = propsForLevelTypeMain[setItemIdx];
            return colorsSet;
        }

        private AdditionalColorsPropsAssetItem GetAdditionalColorPropsForGameModeMainBonusLevels(LevelStageArgs _Args)
        {
            var subList = m_ColorPropsList
                .Where(_Props => _Props.additionalInfo.arguments.Contains("bonus"))
                .ToList();
            int index = ((int)_Args.LevelIndex) % subList.Count;
            return subList[index];
        }

        private AdditionalColorsPropsAssetItem GetAdditionalColorPropsForGameModeRandom()
        {
            return GetAdditionalColorPropsWithConcreteArgument("random");
        }

        private AdditionalColorsPropsAssetItem GetAdditionalColorPropsForGameModeDailyChallenge()
        {
            if (RetroModeSetting.Get())
                return GetAdditionalColorPropsForGameModeRandom();
            return GetAdditionalColorPropsWithConcreteArgument("daily_challenge");
        }

        private AdditionalColorsPropsAssetItem GetAdditionalColorPropsForGameModePuzzles(LevelStageArgs _Args)
        {
            if (RetroModeSetting.Get())
                return GetAdditionalColorPropsForGameModeRandom();
            object showHintArg1 = _Args.Arguments.GetSafe(KeyShowPuzzleLevelHint, out bool showHintArgKeyExist);
            string puzzlesArgName = showHintArgKeyExist && (bool) showHintArg1
                ? "puzzle_hint"
                : "puzzle_default";
            return GetAdditionalColorPropsWithConcreteArgument(puzzlesArgName);
        }

        private AdditionalColorsPropsAssetItem GetAdditionalColorPropsForGameModeBigLevels()
        {
            if (RetroModeSetting.Get())
                return GetAdditionalColorPropsForGameModeRandom();
            return GetAdditionalColorPropsWithConcreteArgument("big_levels");
        }

        private AdditionalColorsPropsAssetItem GetAdditionalColorPropsWithConcreteArgument(string _Argument)
        {
            return m_ColorPropsList
                .FirstOrDefault(_Props =>
                    _Props.additionalInfo.arguments.Contains(_Argument));
        }
        
        private Color GetAdditionalColor(int _ColorId, AdditionalColorsPropsAssetItem _Props)
        {
            return _ColorId switch
            {
                Main              => _Props.main,
                Background1       => _Props.bacground1,
                Background2       => _Props.bacground2,
                PathItem          => _Props.GetColor(_Props.pathItemFillType),
                PathBackground    => _Props.GetColor(_Props.pathBackgroundFillType),
                PathFill          => GetAdditionalColorPathItemFill(_Props),
                Character2        => _Props.GetColor(_Props.characterBorderFillType),
                UiBackground      => _Props.GetColor(_Props.uiBackgroundFillType),
                GameUiAlternative => _Props.GetColor(_Props.uiBackgroundFillType),
                _                 => Color.magenta
            };
        }

        private Color GetAdditionalColorPathItemFill(AdditionalColorsPropsAssetItem _Props)
        {
            if (!ViewSettings.mazeItemBlockColorEqualsMainColor)
                return _Props.GetColor(_Props.pathFillFillType);
            EBackAndFrontColorType colorType = _Props.pathFillFillType switch
            {
                EBackAndFrontColorType.Main => _Props.pathBackgroundFillType switch
                {
                    EBackAndFrontColorType.Main        => _Props.pathFillFillType,
                    EBackAndFrontColorType.Background1 => EBackAndFrontColorType.Background2,
                    EBackAndFrontColorType.Background2 => EBackAndFrontColorType.Background1,
                    _                                  => throw new SwitchExpressionException(_Props.pathBackgroundFillType)
                },
                EBackAndFrontColorType.Background1 => EBackAndFrontColorType.Background1,
                EBackAndFrontColorType.Background2 => EBackAndFrontColorType.Background2,
                _                                  => throw new SwitchExpressionException(_Props.pathFillFillType)
            };
            return _Props.GetColor(colorType);
        }

        protected abstract void ActivateAndShowBackgroundTexture(LevelStageArgs _Args);

        #endregion
    }
    
    public class ViewMazeBackgroundTextureControllerFake : ViewMazeBackgroundTextureControllerBase
    {
        private ICameraProvider CameraProvider { get; }

        public ViewMazeBackgroundTextureControllerFake(
            GlobalGameSettings      _GlobalGameSettings,
            ViewSettings            _ViewSettings,
            IRemotePropertiesRmazor _RemoteProperties,
            IModelGame              _Model,
            IColorProvider          _ColorProvider,
            IPrefabSetManager       _PrefabSetManager,
            ICameraProvider         _CameraProvider,
            IRetroModeSetting       _RetroModeSetting)
            : base(
                _GlobalGameSettings,
                _ViewSettings,
                _RemoteProperties,
                _Model,
                _ColorProvider,
                _PrefabSetManager,
                _RetroModeSetting)
        {
            CameraProvider = _CameraProvider;
        }

        public override void Init()
        {
            ColorProvider.ColorChanged += OnColorChanged;
            base.Init();
        }

        protected override void ActivateAndShowBackgroundTexture(LevelStageArgs _Args) { }
        
        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == Background1)
                CameraProvider.Camera.backgroundColor = _Color;
        }
    }
}