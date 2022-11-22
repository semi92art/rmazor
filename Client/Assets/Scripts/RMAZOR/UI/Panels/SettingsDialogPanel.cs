using System;
using System.Globalization;
using System.Linq;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Entities;
using Common.Entities.UI;
using Common.Enums;
using Common.Exceptions;
using Common.Extensions;
using Common.Managers;
using Common.Providers;
using Common.Settings;
using Common.Ticker;
using Common.UI;
using Common.UI.DialogViewers;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Settings;
using RMAZOR.UI.PanelItems.Setting_Panel_Items;
using RMAZOR.Views.Common;
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

        private const string PrefabSetName = "setting_items";

        #endregion
        
        #region private members

        private RectTransform m_MiniButtonsContent;
        private RectTransform m_SettingsContent;
        
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
        
        #endregion

        #region inject

        private IModelGame                          Model                          { get; }
        private ISettingLanguageDialogPanel         LanguagePanel                  { get; }
        private ISettingsGetter                     SettingsGetter                 { get; }
        private IViewInputCommandsProceeder         CommandsProceeder              { get; }
        private IDialogViewersController            DialogViewersController        { get; }
        private IViewSwitchLevelStageCommandInvoker SwitchLevelStageCommandInvoker { get; }

        private SettingsDialogPanel(
            IModelGame                          _Model,
            ISettingLanguageDialogPanel         _LanguagePanel,
            IManagersGetter                     _Managers,
            IUITicker                           _UITicker,
            ISettingsGetter                     _SettingsGetter,
            ICameraProvider                     _CameraProvider,
            IColorProvider                      _ColorProvider,
            IViewInputCommandsProceeder         _CommandsProceeder,
            IDialogViewersController            _DialogViewersController,
            IViewSwitchLevelStageCommandInvoker _SwitchLevelStageCommandInvoker)
            : base(
                _Managers,
                _UITicker,
                _CameraProvider,
                _ColorProvider)
        {
            Model                          = _Model;
            LanguagePanel                  = _LanguagePanel;
            SettingsGetter                 = _SettingsGetter;
            CommandsProceeder              = _CommandsProceeder;
            DialogViewersController        = _DialogViewersController;
            SwitchLevelStageCommandInvoker = _SwitchLevelStageCommandInvoker;
        }

        #endregion
        
        #region api

        public override EDialogViewerType DialogViewerType => EDialogViewerType.Medium1;

        public override void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanel(_Container, _OnClose);
            var go  = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(_Container, RectTransformLite.FullFill),
                CommonPrefabSetNames.DialogPanels, "settings_panel");
            m_MiniButtonsContent = go.GetCompItem<RectTransform>("mini_buttons_content");
            m_SettingsContent    = go.GetCompItem<RectTransform>("settings_content");
            var title            = go.GetCompItem<TextMeshProUGUI>("title_text");
            var closeButton      = go.GetCompItem<Button>("button_close");
            var panelView        = go.GetCompItem<SimpleUiDialogPanelView>("panel");
            var logInfo = new LocalizableTextObjectInfo(title, ETextType.MenuUI, "settings", 
                _T => _T.ToUpper(CultureInfo.CurrentCulture));
            Managers.LocalizationManager.AddTextObject(logInfo);
            closeButton.onClick.AddListener(OnButtonCloseClick);
            panelView.Init(
                Ticker,
                Managers.AudioManager,
                Managers.LocalizationManager);
            m_MiniButtonsContent.gameObject.DestroyChildrenSafe();
            m_SettingsContent.gameObject.DestroyChildrenSafe();
            InitSettingItems();
            InitOtherButtons();
            PanelRectTransform = go.RTransform();
            PanelRectTransform.SetGoActive(false);
        }

        #endregion

        #region nonpublic methods

        private void OnButtonCloseClick()
        {
            base.OnClose(() =>
            {
                SwitchLevelStageCommandInvoker.SwitchLevelStage(
                    EInputCommand.UnPauseLevel, true);
            });
            Managers.AudioManager.PlayClip(CommonAudioClipArgs.UiButtonClick);
        }

        private void InitSettingItems()
        {
            InitSettingItem(SettingsGetter.SoundSetting);
            InitSettingItem(SettingsGetter.MusicSetting);
            InitSettingItem(SettingsGetter.LanguageSetting);
            InitSettingItem(SettingsGetter.NotificationSetting);
            InitSettingItem(SettingsGetter.HapticsSetting);
        }

        private void InitOtherButtons()
        {
            InitRateUsButton();
            InitLeaderboardsButton();
            InitRestorePurchasesButton();
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

        private void InitLanguageSelectorSettingItem<T>(ISetting<T> _Setting)
        {
            var itemLangSelector = CreateLanguageSelectorSettingItem();
            itemLangSelector.Init(
                Ticker,
                Managers.AudioManager,
                Managers.LocalizationManager,
                DialogViewersController.GetViewer(LanguagePanel.DialogViewerType),
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
                    Managers.AnalyticsManager.SendAnalytic(AnalyticIds.RateGameButton1Pressed);
                    Managers.ShopManager.RateGame();
                },
                Managers.PrefabSetManager.GetObject<Sprite>(
                    CommonPrefabSetNames.Views, "five_stars_icon"),
                Managers.PrefabSetManager.GetObject<Sprite>(
                CommonPrefabSetNames.Views, "rate_game_button_background"));
            var info = new LocalizableTextObjectInfo(item.title, ETextType.MenuUI, "rate_game");
            Managers.LocalizationManager.AddTextObject(info);
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
                    Managers.AnalyticsManager.SendAnalytic(AnalyticIds.LeaderboardsButtonPressed);
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
            var info = new LocalizableTextObjectInfo(item.title, ETextType.MenuUI, "show_leaderboards");
            Managers.LocalizationManager.AddTextObject(info);
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
                    Managers.AnalyticsManager.SendAnalytic(AnalyticIds.RestorePurchasesButtonPressed);
                    Managers.ShopManager.RestorePurchases();
                },
                Managers.PrefabSetManager.GetObject<Sprite>(
                    CommonPrefabSetNames.Views, "refund_icon"));
            var info = new LocalizableTextObjectInfo(item.title, ETextType.MenuUI, "restore_purchases");
            Managers.LocalizationManager.AddTextObject(info);
        }

        private SettingItemMiniButton CreateMiniButtonSetting()
        {
            var obj = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    m_MiniButtonsContent,
                    m_MiniButtonRectLite),
                PrefabSetName, "mini_button_item");
            return obj.GetComponent<SettingItemMiniButton>();
        }

        private SettingItemOnOff CreateOnOffSetting()
        {
            var obj = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    m_SettingsContent,
                    m_SettingItemRectLite),
                PrefabSetName, "on_off_item");
            return obj.GetComponent<SettingItemOnOff>();
        }

        private SettingLanguageSelectorItem CreateLanguageSelectorSettingItem()
        {
            var obj = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    m_SettingsContent,
                    m_SettingItemRectLite),
                PrefabSetName, "in_panel_selector_item");
            return obj.GetComponent<SettingLanguageSelectorItem>();
        }

        private SettingItemAction CreateActionSetting()
        {
            var obj = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    m_SettingsContent,
                    m_SettingItemRectLite),
                PrefabSetName, "action_item");
            return obj.GetComponent<SettingItemAction>();
        }

        private Sprite GetSettingsIconFromPrefabs(string _Key)
        {
            return Managers.PrefabSetManager.GetObject<Sprite>(PrefabSetName, _Key);
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