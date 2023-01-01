using System.Collections.Generic;
using Common.Constants;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Entities.UI;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.Utils;
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.Common;
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RMAZOR.UI.Panels
{
    public interface IPlayBonusLevelDialogPanel : IDialogPanel { }

    public class PlayBonusLevelDialogPanelFake : IPlayBonusLevelDialogPanel
    {
        public EDialogViewerType DialogViewerType   => default;
        public EAppearingState   AppearingState     { get; set; }
        public RectTransform     PanelRectTransform => null;
        public Animator          Animator           => null;
        
        public void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose) { }
    }
    
    public class PlayBonusLevelDialogPanel : DialogPanelBase, IPlayBonusLevelDialogPanel
    {
        #region nonpublic members

        private TextMeshProUGUI
            m_TitleText,
            m_PlayButtonText,
            m_SkipButtonText;
        private Button 
            m_PlayButton,
            m_SkipButton;

        #endregion

        #region inject
        
        private IModelGame                          Model                          { get; }
        private IViewBetweenLevelAdShower           BetweenLevelAdShower           { get; }
        private IMoneyCounter                       MoneyCounter                   { get; }
        private IViewSwitchLevelStageCommandInvoker SwitchLevelStageCommandInvoker { get; }

        private PlayBonusLevelDialogPanel(
            IModelGame                          _Model,
            IViewBetweenLevelAdShower           _BetweenLevelAdShower,
            IMoneyCounter                       _MoneyCounter,
            IViewInputCommandsProceeder         _CommandsProceeder,
            IViewTimePauser                     _TimePauser,
            IManagersGetter                     _Managers,
            IUITicker                           _Ticker,
            ICameraProvider                     _CameraProvider,
            IColorProvider                      _ColorProvider,
            IViewSwitchLevelStageCommandInvoker _SwitchLevelStageCommandInvoker)
            : base(
                _Managers, 
                _Ticker, 
                _CameraProvider,
                _ColorProvider,
                _TimePauser,
                _CommandsProceeder)
        {
            Model                          = _Model;
            BetweenLevelAdShower           = _BetweenLevelAdShower;
            MoneyCounter                   = _MoneyCounter;
            SwitchLevelStageCommandInvoker = _SwitchLevelStageCommandInvoker;
        }
        
        #endregion

        #region api
        
        public override EDialogViewerType DialogViewerType => EDialogViewerType.Medium1;

        public override void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanel(_Container, _OnClose);
            var go = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    _Container,
                    RectTransformLite.FullFill),
                CommonPrefabSetNames.DialogPanels, "play_bonus_level_panel");
            PanelRectTransform = go.RTransform();
            PanelRectTransform.SetGoActive(false);
            GetPrefabContentObjects(go);
            LocalizeTextObjectsOnLoad();
            m_PlayButton.onClick.AddListener(OnPlayButtonClick);
            m_SkipButton.onClick.AddListener(OnSkipButtonClick);
        }

        public override void OnDialogStartAppearing()
        {
            TimePauser.PauseTimeInGame();
            base.OnDialogStartAppearing();
        }

        public override void OnDialogDisappeared()
        {
            TimePauser.UnpauseTimeInGame();
            base.OnDialogDisappeared();
        }

        #endregion

        #region nonpublic methods

        private void OnPlayButtonClick()
        {
            OnClose(LoadBonusLevel);
        }

        private void OnSkipButtonClick()
        {
            Cor.Run(Cor.WaitNextFrame(() =>
            {
                OnClose(TryLoadFinishLevelsGroupPanel);
            }));
        }
        
        private void GetPrefabContentObjects(GameObject _GameObject)
        {
            var go = _GameObject;
            m_TitleText      = go.GetCompItem<TextMeshProUGUI>("title_text");
            m_PlayButtonText = go.GetCompItem<TextMeshProUGUI>("button_play_text");
            m_SkipButtonText = go.GetCompItem<TextMeshProUGUI>("button_skip_text");
            m_PlayButton     = go.GetCompItem<Button>("button_play");
            m_SkipButton     = go.GetCompItem<Button>("button_skip");
        }
        
        private void LocalizeTextObjectsOnLoad()
        {
            var locMan = Managers.LocalizationManager;
            locMan.AddTextObject(
                new LocalizableTextObjectInfo(m_TitleText, ETextType.MenuUI, "play_bonus_level_title",
                    _T => _T.ToUpper()));
            locMan.AddTextObject(
                new LocalizableTextObjectInfo(m_PlayButtonText, ETextType.MenuUI, "Play",
                    _T => _T.ToUpper()));
            locMan.AddTextObject(
                new LocalizableTextObjectInfo(m_SkipButtonText, ETextType.MenuUI, "skip",
                    _T => _T.ToUpper()));
        }

        private void LoadBonusLevel()
        {
            var args = new Dictionary<string, object>
            {
                {CommonInputCommandArg.KeySource, CommonInputCommandArg.ParameterPlayBonusLevelPanel},
            };
            SwitchLevelStageCommandInvoker.SwitchLevelStage(
                EInputCommand.StartUnloadingLevel, 
                args);
        }

        private void TryLoadFinishLevelsGroupPanel()
        {
            long levelIndex = Model.LevelStaging.LevelIndex;
            if (!RmazorUtils.IsLastLevelInGroup(levelIndex))
                return;
            BetweenLevelAdShower.ShowAd = false;
            if (MoneyCounter.CurrentLevelGroupMoney <= 0) return;
            CommandsProceeder.RaiseCommand(
                EInputCommand.FinishLevelGroupPanel, 
                null, 
                true);
        }

        #endregion
    }
}