using System.Collections.Generic;
using System.Globalization;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Entities;
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
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RMAZOR.UI.Panels
{
    public interface IPlayBonusLevelDialogPanel : IDialogPanel { }

    public class PlayBonusLevelDialogPanelFake : DialogPanelFake, IPlayBonusLevelDialogPanel { }
    
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
        
        protected override string PrefabName => "play_bonus_level_panel";

        #endregion

        #region inject

        private IModelGame                Model                { get; }
        private IViewBetweenLevelAdShower BetweenLevelAdShower { get; }
        private IRewardCounter            RewardCounter        { get; }
        private IViewLevelStageSwitcher   LevelStageSwitcher   { get; }

        private PlayBonusLevelDialogPanel(
            IModelGame                  _Model,
            IViewBetweenLevelAdShower   _BetweenLevelAdShower,
            IRewardCounter               _RewardCounter,
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewTimePauser             _TimePauser,
            IManagersGetter             _Managers,
            IUITicker                   _Ticker,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IViewLevelStageSwitcher     _LevelStageSwitcher)
            : base(
                _Managers, 
                _Ticker, 
                _CameraProvider,
                _ColorProvider,
                _TimePauser,
                _CommandsProceeder)
        {
            Model                = _Model;
            BetweenLevelAdShower = _BetweenLevelAdShower;
            RewardCounter         = _RewardCounter;
            LevelStageSwitcher   = _LevelStageSwitcher;
        }
        
        #endregion

        #region api

        public override int DialogViewerId => DialogViewerIdsCommon.MediumCommon;

        #endregion

        #region nonpublic methods
        
        protected override void OnDialogStartAppearing()
        {
            TimePauser.PauseTimeInGame();
            base.OnDialogStartAppearing();
        }

        protected override void OnDialogDisappeared()
        {
            TimePauser.UnpauseTimeInGame();
            base.OnDialogDisappeared();
        }

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
        
        protected override void GetPrefabContentObjects(GameObject _GameObject)
        {
            var go = _GameObject;
            m_TitleText      = go.GetCompItem<TextMeshProUGUI>("title_text");
            m_PlayButtonText = go.GetCompItem<TextMeshProUGUI>("button_play_text");
            m_SkipButtonText = go.GetCompItem<TextMeshProUGUI>("button_skip_text");
            m_PlayButton     = go.GetCompItem<Button>("button_play");
            m_SkipButton     = go.GetCompItem<Button>("button_skip");
        }
        
        protected override void LocalizeTextObjectsOnLoad()
        {
            static string TextFormula(string _Text) => _Text.ToUpper(CultureInfo.CurrentUICulture);
            var locTextInfos = new[]
            {
                new LocTextInfo(m_TitleText, ETextType.MenuUI_H1, "play_bonus_level_title", TextFormula),
                new LocTextInfo(m_PlayButtonText, ETextType.MenuUI_H1, "Play", TextFormula),
                new LocTextInfo(m_SkipButtonText, ETextType.MenuUI_H1, "skip", TextFormula)
            };
            foreach (var locTextInfo in locTextInfos)
                Managers.LocalizationManager.AddLocalization(locTextInfo);
        }

        protected override void SubscribeButtonEvents()
        {
            m_PlayButton.onClick.AddListener(OnPlayButtonClick);
            m_SkipButton.onClick.AddListener(OnSkipButtonClick);
        }

        private void LoadBonusLevel()
        {
            var args = new Dictionary<string, object>
            {
                {ComInComArg.KeySource, ComInComArg.ParameterSourcePlayBonusLevelPanel},
            };
            LevelStageSwitcher.SwitchLevelStage(
                EInputCommand.StartUnloadingLevel, 
                args);
        }

        private void TryLoadFinishLevelsGroupPanel()
        {
            long levelIndex = Model.LevelStaging.LevelIndex;
            if (!RmazorUtils.IsLastLevelInGroup(levelIndex))
                return;
            BetweenLevelAdShower.ShowAdEnabled = false;
            if (RewardCounter.CurrentLevelGroupMoney <= 0) return;
            CommandsProceeder.RaiseCommand(
                EInputCommand.FinishLevelGroupPanel, 
                null, 
                true);
        }

        #endregion
    }
}