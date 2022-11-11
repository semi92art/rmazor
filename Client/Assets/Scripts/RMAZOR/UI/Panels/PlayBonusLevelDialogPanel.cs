using System.Collections.Generic;
using System.Linq;
using Common.CameraProviders;
using Common.Constants;
using Common.Entities;
using Common.Entities.UI;
using Common.Enums;
using Common.Extensions;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.UI;
using Common.UI.DialogViewers;
using Common.Utils;
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
        private ViewSettings                ViewSettings            { get; }
        private IModelGame                  Model                   { get; }
        private IViewBetweenLevelAdLoader   BetweenLevelAdLoader    { get; }
        private IMoneyCounter               MoneyCounter            { get; }
        private IViewInputCommandsProceeder CommandsProceeder       { get; }

        #region nonpublic members

        private Image m_Background; 
        private TextMeshProUGUI
            m_TitleText,
            m_PlayButtonText,
            m_SkipButtonText;
        private Button 
            m_PlayButton,
            m_SkipButton;
        
        private static AudioClipArgs AudioClipArgsMainTheme =>
            new AudioClipArgs("main_theme", EAudioClipType.Music, _Loop: true);

        #endregion

        #region inject
        
        private PlayBonusLevelDialogPanel(
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            IViewBetweenLevelAdLoader   _BetweenLevelAdLoader,
            IMoneyCounter               _MoneyCounter,
            IViewInputCommandsProceeder _CommandsProceeder,
            IManagersGetter             _Managers,
            IUITicker                   _Ticker,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider)
            : base(
                _Managers, 
                _Ticker, 
                _CameraProvider,
                _ColorProvider)
        {
            ViewSettings         = _ViewSettings;
            Model                = _Model;
            BetweenLevelAdLoader = _BetweenLevelAdLoader;
            MoneyCounter         = _MoneyCounter;
            CommandsProceeder    = _CommandsProceeder;
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
            // const string backgroundSpriteNameRaw = "play_bonus_level_panel_background";
            // string backgroundSpriteName = ViewSettings.playBonusLevelPanelBackgroundVariant switch
            // {
            //     1 => $"{backgroundSpriteNameRaw}_1",
            //     2 => $"{backgroundSpriteNameRaw}_2",
            //     3 => $"{backgroundSpriteNameRaw}_3",
            //     4 => $"{backgroundSpriteNameRaw}_4",
            //     5 => $"{backgroundSpriteNameRaw}_5",
            //     _ => $"{backgroundSpriteNameRaw}_1",
            // };
            // m_Background.sprite = Managers.PrefabSetManager.GetObject<Sprite>(
            //     "views", backgroundSpriteName);
        }

        public override void OnDialogStartAppearing()
        {
            CommandsProceeder.LockCommands(GetCommandsToLock(), nameof(IPlayBonusLevelDialogPanel));
            Managers.AudioManager.PauseClip(AudioClipArgsMainTheme);
            base.OnDialogStartAppearing();
        }

        public override void OnDialogDisappeared()
        {
            CommandsProceeder.UnlockCommands(GetCommandsToLock(), nameof(IPlayBonusLevelDialogPanel));
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
            m_Background     = go.GetCompItem<Image>("background");
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
            int levelsGroup = RmazorUtils.GetLevelsGroupIndex(Model.LevelStaging.LevelIndex);
            int bonusLevelIndex = levelsGroup - 1;
            CommandsProceeder.RaiseCommand(
                EInputCommand.ReadyToUnloadLevel, 
                new object[] {bonusLevelIndex, "bonus"},
                true);
        }

        private void TryLoadFinishLevelsGroupPanel()
        {
            long levelIndex = Model.LevelStaging.LevelIndex;
            if (!RmazorUtils.IsLastLevelInGroup(levelIndex))
                return;
            BetweenLevelAdLoader.ShowAd = false;
            if (MoneyCounter.CurrentLevelGroupMoney > 0)
            {
                CommandsProceeder.RaiseCommand(
                    EInputCommand.FinishLevelGroupPanel, 
                    null, 
                    true);
                return;
            }

        }
        
        private static IEnumerable<EInputCommand> GetCommandsToLock()
        {
            return new[]
                {
                    EInputCommand.ShopPanel,
                    EInputCommand.SettingsPanel
                }
                .Concat(RmazorUtils.MoveAndRotateCommands);
        }

        #endregion
    }
}