using System.Collections;
using System.Collections.Generic;
using Common;
using Common.Managers;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Utils;
using RMAZOR.Helpers;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using TMPro;
using UnityEngine;

namespace RMAZOR.Views.Common.ViewUILevelSkippers
{
    public abstract class ViewUILevelSkipperButtonBase : InitBase, IViewUILevelSkipper
    {
        #region nonpublic members

        protected TextMeshPro Text;
        protected GameObject  ButtonObj;
        private   IEnumerator m_ShowButtonCoroutine;

        #endregion

        #region inject
        
        protected IModelGame                          Model                          { get; }
        protected ICameraProvider                     CameraProvider                 { get; }
        protected IColorProvider                      ColorProvider                  { get; }
        private   IViewInputCommandsProceeder         CommandsProceeder              { get; }
        private   IViewBetweenLevelAdShower           BetweenLevelAdShower           { get; }
        private   IViewSwitchLevelStageCommandInvoker SwitchLevelStageCommandInvoker { get; }
        private   ILevelsLoader                       LevelsLoader                   { get; }
        private   IMoneyCounter                       MoneyCounter                   { get; }
        private   ILocalizationManager                LocalizationManager            { get; }
        private   IHapticsManager                     HapticsManager                 { get; }
        private   IViewInputTouchProceeder            TouchProceeder                 { get; }

        protected ViewUILevelSkipperButtonBase(
            IModelGame                          _Model,
            ICameraProvider                     _CameraProvider,
            IColorProvider                      _ColorProvider,
            IViewInputCommandsProceeder         _CommandsProceeder,
            IViewBetweenLevelAdShower           _BetweenLevelAdShower,
            IViewSwitchLevelStageCommandInvoker _SwitchLevelStageCommandInvoker,
            ILevelsLoader                       _LevelsLoader,
            IMoneyCounter                       _MoneyCounter,
            ILocalizationManager                _LocalizationManager,
            IHapticsManager                     _HapticsManager,
            IViewInputTouchProceeder            _TouchProceeder)
        {
            Model                          = _Model;
            CameraProvider                 = _CameraProvider;
            ColorProvider                  = _ColorProvider;
            CommandsProceeder              = _CommandsProceeder;
            BetweenLevelAdShower           = _BetweenLevelAdShower;
            SwitchLevelStageCommandInvoker = _SwitchLevelStageCommandInvoker;
            LevelsLoader                   = _LevelsLoader;
            MoneyCounter                   = _MoneyCounter;
            LocalizationManager            = _LocalizationManager;
            HapticsManager                 = _HapticsManager;
            TouchProceeder                 = _TouchProceeder;
        }

        #endregion

        #region api
        
        public bool LevelSkipped { get; private set; }
        
        public virtual void Init(Vector4 _Offsets)
        {
            CameraProvider.ActiveCameraChanged += OnActiveCameraChanged;
            ColorProvider.ColorChanged += OnColorChanged;
            InitButton();
            base.Init();
        }

        public virtual void OnLevelStageChanged(LevelStageArgs _Args)
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
                case ELevelStage.StartedOrContinued when 
                    _Args.PreviousStage == ELevelStage.ReadyToStart
                    || (_Args.PreviousStage == ELevelStage.Paused 
                        && _Args.PrePreviousStage == ELevelStage.CharacterKilled):
                    ActivateButton(false);
                    string levelType = (string) _Args.Args.GetSafe(CommonInputCommandArg.KeyCurrentLevelType, out _);
                    bool isCurrentLevelBonus = levelType == CommonInputCommandArg.ParameterLevelTypeBonus;
                    if (isCurrentLevelBonus)
                    {
                        Cor.Stop(m_ShowButtonCoroutine);
                        m_ShowButtonCoroutine = ShowButtonCountdownCoroutine();
                        Cor.Run(m_ShowButtonCoroutine);    
                    }
                    break;
                case ELevelStage.Finished:
                    Cor.Stop(m_ShowButtonCoroutine);
                    ActivateButton(false);
                    break;
                case ELevelStage.CharacterKilled:
                    Cor.Stop(m_ShowButtonCoroutine);
                    break;
            }
        }

        #endregion

        #region nonpublic methods
        
        protected virtual void OnColorChanged(int _ColorId, Color _Color)
        {
            Cor.Run(Cor.WaitWhile(() => !Initialized,
                () =>
                {
                    switch (_ColorId)
                    {
                        case ColorIds.UiText:
                            Text.color = _Color;
                            break;
                    }
                }));
        }

        protected abstract void OnActiveCameraChanged(Camera _Camera);

        protected void ActivateButton(bool _Activate)
        {
            ButtonObj.SetActive(_Activate);
        }

        protected abstract void InitButton();

        protected void InitButtonCore(GameObject _Go)
        {
            _Go.transform.SetLocalScaleXY(Vector2.one * 0.3f); 
            var button = _Go.GetCompItem<ButtonOnRaycast>("button");
            button.Init(OnSkipLevelButtonPressed,
                () => Model.LevelStaging.LevelStage, 
                CameraProvider,
                HapticsManager,
                TouchProceeder);
            ButtonObj = _Go;
            Text = _Go.GetCompItem<TextMeshPro>("text");
            Text.sortingOrder = SortingOrders.GameUI + 1;
            var locInfo = new LocalizableTextObjectInfo(Text, ETextType.GameUI, "skip_level");
            LocalizationManager.AddTextObject(locInfo);
            ActivateButton(false);
        }

        protected abstract void OnSkipLevelButtonPressed();
        
        protected abstract IEnumerator ShowButtonCountdownCoroutine();

        protected void SkipLevel()
        {
            var prevArgs = Model.LevelStaging.Arguments;
            var args = new Dictionary<string, object> {{CommonInputCommandArg.KeySkipLevel, true}};
            long levelIndex = Model.LevelStaging.LevelIndex;
            string currentLevelType = (string) prevArgs.GetSafe(
                CommonInputCommandArg.KeyCurrentLevelType, out _);
            bool isCurrentLevelBonus = currentLevelType == CommonInputCommandArg.ParameterLevelTypeBonus;
            LevelSkipped = true;
            BetweenLevelAdShower.ShowAdEnabled = false;
            bool isLastLevelInGroup = RmazorUtils.IsLastLevelInGroup(levelIndex) && !isCurrentLevelBonus;
            if (isLastLevelInGroup)
            {
                int nextBonusLevelIndexToLoad = RmazorUtils.GetLevelsGroupIndex(levelIndex) - 1;
                args.Add(CommonInputCommandArg.KeyNextLevelType, CommonInputCommandArg.ParameterLevelTypeBonus);
                int bonusLevelsCount = LevelsLoader.GetLevelsCount(CommonData.GameId, true);
                EInputCommand inputCommand;
                if (nextBonusLevelIndexToLoad < bonusLevelsCount)
                    inputCommand = EInputCommand.PlayBonusLevelPanel;
                else if (MoneyCounter.CurrentLevelGroupMoney > 0)
                    inputCommand = EInputCommand.FinishLevelGroupPanel;
                else
                {
                    SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.FinishLevel, args);
                    return;
                }
                CommandsProceeder.RaiseCommand(
                    inputCommand,
                    args, 
                    true);
            }
            else if (isCurrentLevelBonus && MoneyCounter.CurrentLevelGroupMoney > 0)
            {
                SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.FinishLevel, args);
                CommandsProceeder.RaiseCommand(
                    EInputCommand.FinishLevelGroupPanel, 
                    args, 
                    true);
            }
            else
            {
                SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.FinishLevel, args);
            }
        }

        #endregion
    }
}