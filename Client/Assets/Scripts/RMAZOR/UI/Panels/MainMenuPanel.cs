using System.Collections.Generic;
using Common;
using Common.Constants;
using Common.Entities;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Entities.UI;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.Utils;
using Newtonsoft.Json;
using RMAZOR.Constants;
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
    public interface IMainMenuPanel : IDialogPanel { }

    public class MainMenuPanelFake : FakeDialogPanel, IMainMenuPanel { }
    
    public class MainMenuPanel : DialogPanelBase, IMainMenuPanel
    {
        #region nonpublic members

        private Animator m_Animator;

        private Button
            m_ButtonDisableAds,
            m_ButtonRateGame,
            m_ButtonSettings;
        
        private Button
            m_ButtonPlayMainLevels,
            m_ButtonPlayPuzzleLevels,
            m_ButtonPlayBigLevels,
            m_ButtonPlayDailyChallenge,
            m_ButtonPlayRandomLevels;
        
        private TextMeshProUGUI
            m_ButtonPlayMainLevelsText,
            m_ButtonPlayPuzzleLevelsText,
            m_ButtonPlayBigLevelsText,
            m_ButtonPlayDailyChallengeText,
            m_ButtonPlayRandomLevelsText;
        
        #endregion

        #region inject
        
        private IModelGame    Model        { get; }
        private ILevelsLoader LevelsLoader { get; }

        public MainMenuPanel(
            IModelGame                  _Model,
            ILevelsLoader               _LevelsLoader,
            IManagersGetter             _Managers,
            IUITicker                   _Ticker,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IViewTimePauser             _TimePauser,
            IViewInputCommandsProceeder _CommandsProceeder)
            : base(
                _Managers, 
                _Ticker,
                _CameraProvider,
                _ColorProvider,
                _TimePauser,
                _CommandsProceeder)
        {
            Model        = _Model;
            LevelsLoader = _LevelsLoader;
        }

        #endregion

        #region api

        public override int DialogViewerId => MazorCommonDialogViewerIds.Fullscreen2;

        public override Animator Animator => m_Animator;

        public override void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanel(_Container, _OnClose);
            var go  = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(_Container, RectTransformLite.FullFill),
                CommonPrefabSetNames.DialogPanels, "main_menu_panel");
            PanelRectTransform = go.RTransform();
            PanelRectTransform.SetLocalPosZ(0f);
            PanelRectTransform.SetGoActive(false);
            GetPrefabContentObjects(go);
            LocalizeTexts();
            SubscribeButtonEvents();
        }

        #endregion

        #region nonpublic methods

        private void GetPrefabContentObjects(GameObject _Go)
        {
            m_Animator                     = _Go.GetCompItem<Animator>("animator");
            m_ButtonDisableAds             = _Go.GetCompItem<Button>("button_no_ads");
            m_ButtonRateGame               = _Go.GetCompItem<Button>("button_rate_game");
            m_ButtonSettings               = _Go.GetCompItem<Button>("button_settings");
            m_ButtonPlayMainLevels         = _Go.GetCompItem<Button>("button_play_main_levels");
            m_ButtonPlayPuzzleLevels       = _Go.GetCompItem<Button>("button_play_puzzle_levels");
            m_ButtonPlayBigLevels          = _Go.GetCompItem<Button>("button_play_big_levels");
            m_ButtonPlayDailyChallenge     = _Go.GetCompItem<Button>("button_play_daily_challenge");
            m_ButtonPlayRandomLevels       = _Go.GetCompItem<Button>("button_play_random_levels");
            m_ButtonPlayMainLevelsText     = _Go.GetCompItem<TextMeshProUGUI>("button_play_main_levels_text");
            m_ButtonPlayPuzzleLevelsText   = _Go.GetCompItem<TextMeshProUGUI>("button_play_puzzle_levels_text");
            m_ButtonPlayBigLevelsText      = _Go.GetCompItem<TextMeshProUGUI>("button_play_big_levels_text");
            m_ButtonPlayDailyChallengeText = _Go.GetCompItem<TextMeshProUGUI>("button_play_daily_challenge_text");
            m_ButtonPlayRandomLevelsText   = _Go.GetCompItem<TextMeshProUGUI>("button_play_random_levels_text");
        }

        private void LocalizeTexts()
        {
            var locMan = Managers.LocalizationManager;
            var locInfos = new[]
            {
                new LocalizableTextObjectInfo(m_ButtonPlayMainLevelsText, ETextType.MenuUI, "Play"),
                new LocalizableTextObjectInfo(m_ButtonPlayPuzzleLevelsText, ETextType.MenuUI, "Play"),
                new LocalizableTextObjectInfo(m_ButtonPlayBigLevelsText, ETextType.MenuUI, "Play"),
                new LocalizableTextObjectInfo(m_ButtonPlayDailyChallengeText, ETextType.MenuUI, "Play"),
                new LocalizableTextObjectInfo(m_ButtonPlayRandomLevelsText, ETextType.MenuUI, "Play"),
            };
            foreach (var locInfo in locInfos)
                locMan.AddTextObject(locInfo);
        }

        private void SubscribeButtonEvents()
        {
            m_ButtonDisableAds        .onClick.AddListener(OnDisableAdsButtonPressed);
            m_ButtonRateGame          .onClick.AddListener(OnRateGameButtonPressed);
            m_ButtonSettings          .onClick.AddListener(OnSettingsButtonPressed);
            m_ButtonPlayMainLevels    .onClick.AddListener(OnPlayMainLevelsButtonPressed);
            m_ButtonPlayPuzzleLevels  .onClick.AddListener(OnPlayPuzzleLevelsButtonPressed);
            m_ButtonPlayBigLevels     .onClick.AddListener(OnPlayBigLevelsButtonPressed);
            m_ButtonPlayDailyChallenge.onClick.AddListener(OnPlayDailyChallengeButtonPressed);
            m_ButtonPlayRandomLevels  .onClick.AddListener(OnPlayRandomLevelsButtonPressed);
        }

        private void OnDisableAdsButtonPressed()
        {
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.DisableAdsButton2Pressed);
            CallCommand(EInputCommand.DisableAdsPanel);
        }

        private void OnRateGameButtonPressed()
        {
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.RateGameButton3Pressed);
            Managers.ShopManager.RateGame();
            SaveUtils.PutValue(SaveKeysCommon.GameWasRated, true);
        }

        private void OnSettingsButtonPressed()
        {
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.SettingsButton2Pressed);
            CallCommand(EInputCommand.SettingsPanel);
        }

        private void OnPlayMainLevelsButtonPressed()
        {
            LoadLastMainLevel();
        }

        private void OnPlayPuzzleLevelsButtonPressed()
        {
            
        }

        private void OnPlayBigLevelsButtonPressed()
        {
            
        }

        private void OnPlayDailyChallengeButtonPressed()
        {
            
        }

        private void OnPlayRandomLevelsButtonPressed()
        {
            
        }

        private void LoadLastMainLevel()
        {
            var scoreMan = Managers.ScoreManager;
            const string fName = MazorCommonData.SavedGameFileName;
                var sgEntityRemote = scoreMan.GetSavedGameProgress(fName, false);
                var sgEntityCache = scoreMan.GetSavedGameProgress(fName, true);
                Cor.Run(Cor.WaitWhile(
               () => sgEntityRemote.Result == EEntityResult.Pending,
               () =>
               {
                   bool castSgRemoteSuccess = sgEntityRemote.Value.CastTo(out SavedGame sgRemote);
                   if (sgEntityRemote.Result != EEntityResult.Success || !castSgRemoteSuccess)
                   {
                       Cor.Run(Cor.WaitWhile(
                           () => sgEntityCache.Result == EEntityResult.Pending,
                           () =>
                           {
                               bool castSgCacheSuccess = sgEntityCache.Value.CastTo(out SavedGame sgCache);
                               if (sgEntityCache.Result != EEntityResult.Success || !castSgCacheSuccess)
                               {
                                   Dbg.LogWarning("Failed to load saved game entity: " +
                                                  $"_Result: {sgEntityCache.Result}," +
                                                  $" castSuccess: {castSgCacheSuccess}," +
                                                  $" _Value: {JsonConvert.SerializeObject(sgEntityCache.Value)}");
                                   var savedGame = new SavedGame
                                   {
                                       FileName = fName,
                                       Level = 0,
                                       Money = 100
                                   };
                                   scoreMan.SaveGameProgress(savedGame, true);
                                   LoadMainLevelByIndexAndClosePanel(0, null);
                                   return;
                               }
                               LoadMainLevelByIndexAndClosePanel(sgCache.Level, sgCache.Args);
                           },
                           _Seconds: 1f));
                       return;
                   }
                   LoadMainLevelByIndexAndClosePanel(sgRemote.Level, sgRemote.Args);
               }, _Seconds: 1f));
        }

        private void LoadMainLevelByIndexAndClosePanel(
        long                       _LevelIndex,
        Dictionary<string, object> _Args)
        {
            base.OnClose(null);
            LoadMainLevelByIndex(_LevelIndex, _Args);
        }
        
        private void LoadMainLevelByIndex(
            long                       _LevelIndex,
            Dictionary<string, object> _Args)
        {
            string levelType = (string) _Args.GetSafe(CommonInputCommandArg.KeyNextLevelType, out _);
            bool isBonusLevel = levelType == CommonInputCommandArg.ParameterLevelTypeBonus;
            var info = LevelsLoader.GetLevelInfo(1, _LevelIndex, isBonusLevel);
            Model.LevelStaging.Arguments ??= new Dictionary<string, object>();
            Model.LevelStaging.Arguments.SetSafe(
                CommonInputCommandArg.KeyNextLevelType,
                isBonusLevel ?
                    CommonInputCommandArg.ParameterLevelTypeBonus
                    : CommonInputCommandArg.ParameterLevelTypeMain);
            Model.LevelStaging.LoadLevel(info, _LevelIndex);
        }
        
        private void CallCommand(EInputCommand _Command)
        {
            CommandsProceeder.RaiseCommand(_Command, null);
        }

        #endregion
    }
}