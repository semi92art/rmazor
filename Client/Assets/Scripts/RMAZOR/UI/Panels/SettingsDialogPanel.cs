using System;
using System.Globalization;
using System.Linq;
using Common.Constants;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Entities.UI;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Exceptions;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Settings;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.UI.DialogViewers;
using mazing.common.Runtime.Utils;
using RMAZOR.Constants;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Settings;
using RMAZOR.UI.PanelItems.Setting_Panel_Items;
using RMAZOR.Views.Common;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RMAZOR.UI.Panels
{
    public interface ISettingDialogPanel : IDialogPanel { }
    
    public class SettingsDialogPanel : DialogPanelBase, ISettingDialogPanel
    {
        #region constants

        private const string SettingItemsPrefabSetName = "setting_items";

        #endregion
        
        #region private members

        private RectTransform   m_MiniButtonsContent, m_SettingsContent;
        private TextMeshProUGUI m_TitleText;
        private Button          m_ButtonClose;
        private SimpleUiItem    m_PanelView;
        private Image           m_BackGlowDark;
        
        private readonly RectTransformLite m_SettingItemRectLite = new RectTransformLite
        {
            Anchor = UiAnchor.Create(0, 1, 0, 1),
            AnchoredPosition = Vector2.zero,
            Pivot = Vector2.one * 0.5f,
            SizeDelta = new Vector2(406f, 100f)
        };

        private readonly RectTransformLite m_MiniButtonRectLite = new RectTransformLite
        {
            Anchor = UiAnchor.Create(0, 1, 0, 1),
            AnchoredPosition = new Vector2(0f, 0f),
            Pivot = Vector2.one * 0.5f,
            SizeDelta = new Vector2(87f, 87f)
        };
        
        protected override string PrefabName => "settings_panel";
        
        #endregion

        #region inject

        private IModelGame                  Model                   { get; }
        private ISettingLanguageDialogPanel LanguagePanel           { get; }
        private ISettingsGetter             SettingsGetter          { get; }
        private IDialogViewersController    DialogViewersController { get; }
        private IViewLevelStageSwitcher     LevelStageSwitcher      { get; }

        private SettingsDialogPanel(
            IModelGame                  _Model,
            ISettingLanguageDialogPanel _LanguagePanel,
            IManagersGetter             _Managers,
            IUITicker                   _UITicker,
            ISettingsGetter             _SettingsGetter,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IViewTimePauser             _TimePauser,
            IDialogViewersController    _DialogViewersController,
            IViewLevelStageSwitcher     _LevelStageSwitcher,
            IViewInputCommandsProceeder _CommandsProceeder)
            : base(
                _Managers,
                _UITicker,
                _CameraProvider,
                _ColorProvider,
                _TimePauser,
                _CommandsProceeder)
        {
            Model                   = _Model;
            LanguagePanel           = _LanguagePanel;
            SettingsGetter          = _SettingsGetter;
            DialogViewersController = _DialogViewersController;
            LevelStageSwitcher      = _LevelStageSwitcher;
        }

        #endregion
        
        #region api

        public override int DialogViewerId => DialogViewerIdsCommon.FullscreenCommon;

        #endregion

        #region nonpublic methods
        
        protected override void LoadPanelCore(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanelCore(_Container, _OnClose);
            m_PanelView.Init(
                Ticker,
                Managers.AudioManager,
                Managers.LocalizationManager);
            m_MiniButtonsContent.gameObject.DestroyChildrenSafe();
            m_SettingsContent.gameObject.DestroyChildrenSafe();
            InitSettingItems();
            InitOtherButtons();
        }
        
        protected override void OnDialogStartAppearing()
        {
            m_BackGlowDark.enabled = Model.LevelStaging.LevelStage == ELevelStage.None;
            TimePauser.PauseTimeInGame();
            base.OnDialogStartAppearing();
        }

        protected override void OnDialogDisappeared()
        {
            TimePauser.UnpauseTimeInGame();
            base.OnDialogDisappeared();
        }

        protected override void GetPrefabContentObjects(GameObject _Go)
        {
            m_MiniButtonsContent = _Go.GetCompItem<RectTransform>("mini_buttons_content");
            m_SettingsContent    = _Go.GetCompItem<RectTransform>("settings_content");
            m_TitleText          = _Go.GetCompItem<TextMeshProUGUI>("title_text");
            m_ButtonClose        = _Go.GetCompItem<Button>("button_close");
            m_PanelView          = _Go.GetCompItem<SimpleUiItem>("panel");
            m_BackGlowDark       = _Go.GetCompItem<Image>("back_glow_dark");
        }

        protected override void LocalizeTextObjectsOnLoad()
        {
            var logInfo = new LocTextInfo(m_TitleText, ETextType.MenuUI_H1, "settings", 
                _T => _T.ToUpper(CultureInfo.CurrentCulture));
            Managers.LocalizationManager.AddLocalization(logInfo);
        }

        protected override void SubscribeButtonEvents()
        {
            m_ButtonClose.onClick.AddListener(OnButtonCloseClick);
        }

        private void OnButtonCloseClick()
        {
            OnClose(() => LevelStageSwitcher.SwitchLevelStage(EInputCommand.UnPauseLevel));
            PlayButtonClickSound();
        }

        private void InitSettingItems()
        {
            InitSettingItem(SettingsGetter.MusicSetting);
            InitSettingItem(SettingsGetter.SoundSetting);
#if !YANDEX_GAMES
            InitSettingItem(SettingsGetter.NotificationSetting);
            InitSettingItem(SettingsGetter.HapticsSetting);
#endif
            InitSettingItem(SettingsGetter.LanguageSetting);
        }

        private void InitOtherButtons()
        {
            // TODO доработать лидерборды для яндекса
#if !YANDEX_GAMES
            InitLeaderboardsButton();
            InitRestorePurchasesButton();
#endif
            InitRateUsButton();
            InitRetroModeSettingItem(SettingsGetter.RetroModeSetting);
        }

        private void InitSettingItem<T>(ISetting<T> _Setting)
        {
            switch (_Setting.Location)
            {
                case ESettingLocation.MiniButtons:
                    InitMiniButtonItem(_Setting); break;
                case ESettingLocation.Main:
                    switch (_Setting.Type)
                    {
                        case ESettingType.OnOff:
                            InitOnOffItem(_Setting); break;
                        case ESettingType.InPanelSelector:
                            InitLanguageSelectorSettingItem(_Setting); break;
                        default: throw new SwitchCaseNotImplementedException(_Setting.Type);
                    }
                    break;
                default: throw new SwitchCaseNotImplementedException(_Setting.Location);
            }
        }

        private void InitMiniButtonItem<T>(ISetting<T> _Setting)
        {
            var itemMiniButton = CreateMiniButtonSetting();
            itemMiniButton.Init(
                Ticker,
                Managers.AudioManager,
                Managers.LocalizationManager,
                Convert.ToBoolean(_Setting.Get()),
                _IsOn => _Setting.Put(ConvertValue<T>(_IsOn)),
                GetSettingsIconFromPrefabs(_Setting.SpriteOnKey),
                GetSettingsIconFromPrefabs(_Setting.SpriteOffKey));
        }

        private void InitOnOffItem<T>(ISetting<T> _Setting)
        {
            var itemOnOff = CreateOnOffSetting();
            itemOnOff.Init(
                Ticker,
                Managers.AudioManager,
                Managers.LocalizationManager,
                Convert.ToBoolean(_Setting.Get()),
                _Setting.TitleKey, 
                _IsOn => _Setting.Put(ConvertValue<T>(_IsOn)));
        }
        
        private void InitRetroModeSettingItem(IRetroModeSetting _Setting)
        {
            var retroModeSettingItem = CreateRetroModeSetting();
            _Setting.StateUpdated += retroModeSettingItem.CheckIfSettingLocked;
            retroModeSettingItem.Init(
                Ticker,
                Managers.AudioManager,
                Managers.LocalizationManager,
                Managers.ScoreManager,
                Convert.ToBoolean(_Setting.Get()),
                _Setting.TitleKey, 
                _IsOn => _Setting.Put(ConvertValue<bool>(_IsOn)));
        }

        private void InitLanguageSelectorSettingItem<T>(ISetting<T> _Setting)
        {
            var itemLangSelector = CreateLanguageSelectorSettingItem();
            itemLangSelector.Init(
                Ticker,
                Managers.AudioManager,
                Managers.LocalizationManager,
                DialogViewersController.GetViewer(LanguagePanel.DialogViewerId),
                _Language =>
                {
                    void SetLangSetting()
                    {
                        var lang = ConvertValue<ELanguage>(_Language);
                        T val = ConvertValue<T>(lang);
                        _Setting.Put(val);
                        Managers.LocalizationManager.SetLanguage(
                            Managers.LocalizationManager.GetCurrentLanguage());
                        Managers.AnalyticsManager.SendAnalytic(AnalyticIds.GetLanguageChangedAnalyticId(_Language));
                    }
                    itemLangSelector.languageIcon.sprite = GetLanguageIcon(_Language);
                    SetLangSetting();
                },
                () => _Setting.Values.Select(_Val => ConvertValue<ELanguage>(_Val)).ToList(),
                LanguagePanel,
                GetLanguageIcon);
        }

        private void InitRateUsButton()
        {
            var item = CreateActionSetting();
            item.Init(
                Ticker,
                Managers.AudioManager,
                Managers.LocalizationManager,
                () =>
                {
                    Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.RateGameButton1Click);
                    Managers.ShopManager.RateGame();
                },
                Managers.PrefabSetManager.GetObject<Sprite>(
                    CommonPrefabSetNames.Views, "five_stars_icon"),
                Managers.PrefabSetManager.GetObject<Sprite>(
                CommonPrefabSetNames.Views, "rate_game_button_background"));
            var info = new LocTextInfo(item.title, ETextType.MenuUI_H1, "rate_game");
            Managers.LocalizationManager.AddLocalization(info);
            item.Highlighted = true;
        }

        private void InitLeaderboardsButton()
        {
            var item = CreateActionSetting();
            item.Init(
                Ticker,
                Managers.AudioManager,
                Managers.LocalizationManager,
                () =>
                {
                    Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.LeaderboardsButtonClick);
                    var scoreEntity = Managers.ScoreManager.GetScoreFromLeaderboard(DataFieldIds.Level, false);
                    Cor.Run(Cor.WaitWhile(
                        () => scoreEntity.Result == EEntityResult.Pending,
                        () =>
                        {
                            switch (scoreEntity.Result)
                            {
                                case EEntityResult.Pending:
                                    Dbg.LogWarning("Timeout when getting score from leaderboard");
                                    return;
                                case EEntityResult.Fail:
                                    Dbg.LogError("Failed to get score from leaderboard");
                                    return;
                                case EEntityResult.Success:
                                {
                                    var score = scoreEntity.GetFirstScore();
                                    if (!score.HasValue)
                                    {
                                        Dbg.LogError("Failed to get score from leaderboard");
                                        return;
                                    }
                                    Managers.ScoreManager.SetScoreToLeaderboard(
                                        DataFieldIds.Level, 
                                        score.Value + 1, 
                                        false);
                                    break;
                                }
                                default:
                                    throw new SwitchCaseNotImplementedException(scoreEntity.Result);
                            }
                        },
                        _Seconds: 3f,
                        _Ticker: Ticker));
                    Managers.ScoreManager.ShowLeaderboard(DataFieldIds.Level);
                },
                Managers.PrefabSetManager.GetObject<Sprite>(
                    CommonPrefabSetNames.Views, "leaderboard_icon"),
                Managers.PrefabSetManager.GetObject<Sprite>(
                    CommonPrefabSetNames.Views, "leaderboard_button_background"));
            var info = new LocTextInfo(item.title, ETextType.MenuUI_H1, "show_leaderboards");
            Managers.LocalizationManager.AddLocalization(info);
        }

        private void InitRestorePurchasesButton()
        {
            var item = CreateActionSetting();
            item.Init(
                Ticker,
                Managers.AudioManager,
                Managers.LocalizationManager,
                () =>
                {
                    Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.RestorePurchasesButtonClick);
                    Managers.ShopManager.RestorePurchases();
                },
                Managers.PrefabSetManager.GetObject<Sprite>(
                    CommonPrefabSetNames.Views, "refund_icon"));
            var info = new LocTextInfo(item.title, ETextType.MenuUI_H1, "restore_purchases");
            Managers.LocalizationManager.AddLocalization(info);
        }

        private SettingItemMiniButton CreateMiniButtonSetting()
        {
            var obj = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    m_MiniButtonsContent,
                    m_MiniButtonRectLite),
                SettingItemsPrefabSetName, "mini_button_item");
            return obj.GetComponent<SettingItemMiniButton>();
        }

        private SettingItemOnOff CreateOnOffSetting()
        {
            var obj = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    m_SettingsContent,
                    m_SettingItemRectLite),
                SettingItemsPrefabSetName, "on_off_item");
            return obj.GetComponent<SettingItemOnOff>();
        }
        
        private RetroModeSettingItem CreateRetroModeSetting()
        {
            var obj = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    m_SettingsContent,
                    m_SettingItemRectLite),
                SettingItemsPrefabSetName, "retro_mode_item");
            return obj.GetComponent<RetroModeSettingItem>();
        }

        private SettingLanguageSelectorItem CreateLanguageSelectorSettingItem()
        {
            var obj = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    m_SettingsContent,
                    m_SettingItemRectLite),
                SettingItemsPrefabSetName, "in_panel_selector_item");
            return obj.GetComponent<SettingLanguageSelectorItem>();
        }

        private SettingItemAction CreateActionSetting()
        {
            var obj = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    m_SettingsContent,
                    m_SettingItemRectLite),
                SettingItemsPrefabSetName, "action_item");
            return obj.GetComponent<SettingItemAction>();
        }

        private Sprite GetSettingsIconFromPrefabs(string _Key)
        {
            return Managers.PrefabSetManager.GetObject<Sprite>(SettingItemsPrefabSetName, _Key);
        }

        private static T ConvertValue<T>(object _Value)
        {
            if (typeof(T).IsEnum)
                return (T) Enum.Parse(typeof(T), _Value.ToString());
            return (T) Convert.ChangeType(_Value, typeof(T));
        }

        private Sprite GetLanguageIcon(ELanguage _Language)
        {
            return Managers.PrefabSetManager.GetObject<Sprite>(
                "lang_icons", 
                _Language.ToString().ToLowerInvariant());
        }

        #endregion
    }
}