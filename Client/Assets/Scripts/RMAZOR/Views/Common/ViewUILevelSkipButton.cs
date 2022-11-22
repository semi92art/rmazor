using System.Collections;
using System.Collections.Generic;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Enums;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Managers.Advertising;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Helpers;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.UI;
using RMAZOR.Views.Utils;
using TMPro;
using UnityEngine;

namespace RMAZOR.Views.Common
{
    public interface IViewUILevelSkipper : IInit, IInitViewUIItem, IOnLevelStageChanged 
    {
        bool LevelSkipped { get; }
    }

    public class ViewUILevelSkipperFake : InitBase, IViewUILevelSkipper
    {
        public bool LevelSkipped                                 => false;
        public void Init(Vector4                       _Offsets) { }
        public void OnLevelStageChanged(LevelStageArgs _Args)    { }
        public void SkipLevel()                                  { }
    }
    
    public class ViewUILevelSkipperButton : InitBase, IViewUILevelSkipper
    {
        #region nonpublic members

        private float          m_TopOffset;
        private GameObject     m_ButtonObj;
        private TextMeshPro    m_Text;
        private SpriteRenderer m_Border, m_Background;
        private IEnumerator    m_ShowButtonCoroutine;

        #endregion
        
        #region inject

        private ViewSettings                        ViewSettings                   { get; }
        private IModelGame                          Model                          { get; }
        private IContainersGetter                   ContainersGetter               { get; }
        private IPrefabSetManager                   PrefabSetManager               { get; }
        private IHapticsManager                     HapticsManager                 { get; }
        private IAudioManager                       AudioManager                   { get; }
        private IViewInputTouchProceeder            TouchProceeder                 { get; }
        private ICameraProvider                     CameraProvider                 { get; }
        private ILocalizationManager                LocalizationManager            { get; }
        private IAdsManager                         AdsManager                     { get; }
        private IColorProvider                      ColorProvider                  { get; }
        private IViewGameTicker                     ViewGameTicker                 { get; }
        private IModelGameTicker                    ModelGameTicker                { get; }
        private IViewBetweenLevelAdLoader           BetweenLevelAdLoader           { get; }
        private IViewSwitchLevelStageCommandInvoker SwitchLevelStageCommandInvoker { get; }
        private IMoneyCounter                       MoneyCounter                   { get; }
        private IViewInputCommandsProceeder         CommandsProceeder              { get; }
        private ILevelsLoader                       LevelsLoader                   { get; }

        private ViewUILevelSkipperButton(
            ViewSettings                        _ViewSettings,
            IModelGame                          _Model,
            IContainersGetter                   _ContainersGetter,
            IPrefabSetManager                   _PrefabSetManager,
            IHapticsManager                     _HapticsManager,
            IAudioManager                       _AudioManager,
            IViewInputTouchProceeder            _TouchProceeder,
            ICameraProvider                     _CameraProvider,
            ILocalizationManager                _LocalizationManager,
            IAdsManager                         _AdsManager,
            IColorProvider                      _ColorProvider,
            IViewGameTicker                     _ViewGameTicker,
            IModelGameTicker                    _ModelGameTicker,
            IViewBetweenLevelAdLoader           _BetweenLevelAdLoader,
            IViewSwitchLevelStageCommandInvoker _SwitchLevelStageCommandInvoker,
            IMoneyCounter                       _MoneyCounter,
            IViewInputCommandsProceeder         _CommandsProceeder,
            ILevelsLoader _LevelsLoader)
        {
            ViewSettings                   = _ViewSettings;
            Model                          = _Model;
            ContainersGetter               = _ContainersGetter;
            PrefabSetManager               = _PrefabSetManager;
            HapticsManager                 = _HapticsManager;
            AudioManager                   = _AudioManager;
            TouchProceeder                 = _TouchProceeder;
            CameraProvider                 = _CameraProvider;
            LocalizationManager            = _LocalizationManager;
            AdsManager                     = _AdsManager;
            ColorProvider                  = _ColorProvider;
            ViewGameTicker                 = _ViewGameTicker;
            ModelGameTicker                = _ModelGameTicker;
            BetweenLevelAdLoader           = _BetweenLevelAdLoader;
            SwitchLevelStageCommandInvoker = _SwitchLevelStageCommandInvoker;
            MoneyCounter                   = _MoneyCounter;
            CommandsProceeder              = _CommandsProceeder;
            LevelsLoader                   = _LevelsLoader;
        }

        #endregion

        #region api
        
        public void Init(Vector4 _Offsets)
        {
            m_TopOffset = _Offsets.w;
            ColorProvider.ColorChanged += OnColorChanged;
            CameraProvider.ActiveCameraChanged += OnActiveCameraChanged;
            InitButton();
            Init();
        }

        public bool LevelSkipped { get; private set; }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.LevelStage)
            {
                case ELevelStage.Loaded:
                    LevelSkipped = false;
                    ActivateButton(false);
                    break;
                case ELevelStage.ReadyToStart:
                    ActivateButton(false);
                    break;
                case ELevelStage.StartedOrContinued:
                    if (_Args.PreviousStage != ELevelStage.ReadyToStart
                        || (_Args.PrePreviousStage != ELevelStage.Loaded
                        && _Args.PrePreviousStage != ELevelStage.CharacterKilled))
                    {
                        return;
                    }
                    Cor.Stop(m_ShowButtonCoroutine);
                    m_ShowButtonCoroutine = ShowButtonCountdownCoroutine();
                    Cor.Run(m_ShowButtonCoroutine);
                    break;
                case ELevelStage.Finished:
                    
                    bool MustStartUnloadingLevel()
                    {
                        string currentLevelType = (string)_Args.Args.GetSafe(CommonInputCommandArg.KeyCurrentLevelType, out _);
                        bool isCurrentLevelBonus = currentLevelType == CommonInputCommandArg.ParameterLevelTypeBonus;
                        bool isLastLevelInGroup = RmazorUtils.IsLastLevelInGroup(_Args.LevelIndex);
                        return LevelSkipped && !isCurrentLevelBonus && !isLastLevelInGroup;
                    }
                    if (MustStartUnloadingLevel())
                    {
                        SwitchLevelStageCommandInvoker.SwitchLevelStage(
                            EInputCommand.StartUnloadingLevel, 
                            true);
                    }
                    Cor.Stop(m_ShowButtonCoroutine);
                    ActivateButton(false);
                    break;
                case ELevelStage.CharacterKilled:
                    Cor.Stop(m_ShowButtonCoroutine);
                    break;
                case ELevelStage.ReadyToUnloadLevel:
                case ELevelStage.Unloaded:
                case ELevelStage.Paused:
                    break;
                default: throw new SwitchCaseNotImplementedException(_Args.LevelStage);
            }
        }

        #endregion

        #region nonpublic methods
        
        private void OnActiveCameraChanged(Camera _Camera)
        {
            m_ButtonObj.SetParent(_Camera.transform);
            var tr = m_ButtonObj.transform;
            var screenBounds = GraphicUtils.GetVisibleBounds(_Camera);
            float yPos = screenBounds.max.y - m_TopOffset - 8f;
            tr.SetLocalPosXY(screenBounds.center.x, yPos);
        }

        private void OnColorChanged(int _ColorId, Color _Color)
        {
            Cor.Run(Cor.WaitWhile(() => !Initialized,
                () =>
                {
                    switch (_ColorId)
                    {
                        case ColorIds.UiBorder:      m_Border.color     = _Color; break;
                        case ColorIds.UiBackground:  m_Background.color = _Color; break;
                    }
                }));
        }
        
        private void OnSkipLevelButtonPressed()
        {
            void OnBeforeAdShown()
            {
                AudioManager.MuteAudio(EAudioClipType.Music);
                TickerUtils.PauseTickers(true, ViewGameTicker, ModelGameTicker);
            }
            void OnReward()
            {
                var prevArgs = Model.LevelStaging.Arguments;
                var args = new Dictionary<string, object> {{CommonInputCommandArg.KeySkipLevel, true}};
                long levelIndex = Model.LevelStaging.LevelIndex;
                string levelType = (string) prevArgs.GetSafe(
                    CommonInputCommandArg.KeyCurrentLevelType, out _);
                bool isBonusLevel = levelType == CommonInputCommandArg.ParameterLevelTypeBonus;
                LevelSkipped = true;
                BetweenLevelAdLoader.ShowAd = false;
                bool isLastLevelInGroup = RmazorUtils.IsLastLevelInGroup(levelIndex) && !isBonusLevel;
                if (isLastLevelInGroup)
                {
                    int nextBonusLevelIndexToLoad = RmazorUtils.GetLevelsGroupIndex(levelIndex) - 1;
                    args.Add(CommonInputCommandArg.KeyNextLevelType, CommonInputCommandArg.ParameterLevelTypeBonus);
                    int bonusLevelsCount = LevelsLoader.GetLevelsCount(CommonData.GameId, args);
                    EInputCommand inputCommand;
                    if (nextBonusLevelIndexToLoad < bonusLevelsCount)
                        inputCommand = EInputCommand.PlayBonusLevelPanel;
                    else if (MoneyCounter.CurrentLevelGroupMoney > 0)
                        inputCommand = EInputCommand.FinishLevelGroupPanel;
                    else
                        inputCommand = EInputCommand.FinishLevel;
                    CommandsProceeder.RaiseCommand(
                        inputCommand,
                        args, 
                        true);
                }
                else if (isBonusLevel && MoneyCounter.CurrentLevelGroupMoney > 0)
                {
                    CommandsProceeder.RaiseCommand(
                        EInputCommand.FinishLevelGroupPanel, 
                        args, 
                        true);
                }
                else
                {
                    SwitchLevelStageCommandInvoker.SwitchLevelStage(
                        EInputCommand.FinishLevel, 
                        true, 
                        args);
                }
            }
            void OnAdClosed()
            {
                AudioManager.UnmuteAudio(EAudioClipType.Music);
                TickerUtils.PauseTickers(false, ViewGameTicker, ModelGameTicker);
                ActivateButton(false);
            }
            AdsManager.ShowRewardedAd(
                OnBeforeAdShown,
                _OnReward: OnReward,
                _OnClosed: OnAdClosed);
        }
        
        private void InitButton()
        {
            var parent = ContainersGetter.GetContainer(ContainerNames.GameUI);
            var go = PrefabSetManager.InitPrefab(
                parent, CommonPrefabSetNames.UiGame, "skip_level_button");
            go.transform.SetLocalScaleXY(Vector2.one * 0.3f); 

            var button = go.GetCompItem<ButtonOnRaycast>("button");
            button.Init(OnSkipLevelButtonPressed,
                () => Model.LevelStaging.LevelStage, 
                CameraProvider,
                HapticsManager,
                TouchProceeder);
            m_ButtonObj = go;
            m_Border = go.GetCompItem<SpriteRenderer>("border");
            m_Border.color = ColorProvider.GetColor(ColorIds.UiBorder);
            m_Border.sortingOrder = SortingOrders.GameUI + 1;
            m_Background = go.GetCompItem<SpriteRenderer>("background");
            m_Background.color = ColorProvider.GetColor(ColorIds.UiBackground);
            m_Background.sortingOrder = SortingOrders.GameUI;
            go.GetCompItem<SpriteRenderer>("no_ads_icon").sortingOrder = SortingOrders.GameUI + 1;
            m_Text = go.GetCompItem<TextMeshPro>("text");
            m_Text.sortingOrder = SortingOrders.GameUI + 1;
            var locInfo = new LocalizableTextObjectInfo(m_Text, ETextType.GameUI, "skip_level");
            LocalizationManager.AddTextObject(locInfo);
            ActivateButton(false);
        }
        
        private IEnumerator ShowButtonCountdownCoroutine()
        {
            yield return Cor.Delay(ViewSettings.skipLevelSeconds, 
                ViewGameTicker,
                () =>
            {
                if (AdsManager.RewardedAdReady)
                    ActivateButton(true);
            });
        }

        private void ActivateButton(bool _Active)
        {
            m_ButtonObj.SetActive(_Active);
        }

        #endregion
    }
}