using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Entities;
using Common.Entities.UI;
using Common.Enums;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.Settings;
using Common.Ticker;
using Common.UI;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Settings;
using RMAZOR.UI.PanelItems.Setting_Panel_Items;
using UnityEngine;

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
            AnchoredPosition = new Vector2(213f, -54.6f),
            Pivot = Vector2.one * 0.5f,
            SizeDelta = new Vector2(406f, 87f)
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

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private CommonGameSettings          GameSettings   { get; }
        private ISettingSelectorDialogPanel SelectorPanel  { get; }
        private ISettingsGetter             SettingsGetter { get; }

        public SettingsDialogPanel(
            CommonGameSettings          _GameSettings,
            ISettingSelectorDialogPanel _SelectorPanel,
            IBigDialogViewer            _DialogViewer,
            IManagersGetter             _Managers,
            IUITicker                   _UITicker,
            ISettingsGetter             _SettingsGetter,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider)
            : base(
                _Managers,
                _UITicker,
                _DialogViewer,
                _CameraProvider,
                _ColorProvider)
        {
            GameSettings = _GameSettings;
            SelectorPanel  = _SelectorPanel;
            SettingsGetter = _SettingsGetter;
        }

        #endregion
        
        #region api
        
        public override EUiCategory Category      => EUiCategory.Settings;
        public override bool        AllowMultiple => false;

        public override void LoadPanel()
        {
            base.LoadPanel();
            var sp = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(DialogViewer.Container, RectTransformLite.FullFill),
                CommonPrefabSetNames.DialogPanels, "settings_panel");
            m_MiniButtonsContent = sp.GetCompItem<RectTransform>("mini_buttons_content");
            m_SettingsContent = sp.GetCompItem<RectTransform>("settings_content");
            m_MiniButtonsContent.gameObject.DestroyChildrenSafe();
            m_SettingsContent.gameObject.DestroyChildrenSafe();
            InitSettingItems();
            InitOtherButtons();
            InitDebugSettingItem();
            PanelObject = sp.RTransform();
        }

        #endregion

        #region nonpublic methods

        private void InitSettingItems()
        {
            InitSettingItem(SettingsGetter.SoundSetting);
            InitSettingItem(SettingsGetter.MusicSetting);
            InitSettingItem(SettingsGetter.LanguageSetting);
            // InitSettingItem(SettingsGetter.NotificationSetting);
            InitSettingItem(SettingsGetter.HapticsSetting);
            if (ColorProvider.DarkThemeAvailable)
                InitSettingItem(SettingsGetter.DarkThemeSetting);
        }

        private void InitOtherButtons()
        {
            InitRateUsButton();
            InitLeaderboardsButton();
            InitRestorePurchasesButton();
        }

        private void InitDebugSettingItem()
        {
            if (Application.isEditor || GameSettings.debugEnabled)
                InitSettingItem(SettingsGetter.DebugSetting);
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
                            InitInPanelSelectorItem(_Setting); break;
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
                Managers,
                Ticker,
                ColorProvider,
                Convert.ToBoolean(_Setting.Get()),
                _IsOn => _Setting.Put(ConvertValue<T>(_IsOn)),
                GetSettingsIconFromPrefabs(_Setting.SpriteOnKey),
                GetSettingsIconFromPrefabs(_Setting.SpriteOffKey));
        }

        private void InitOnOffItem<T>(ISetting<T> _Setting)
        {
            var itemOnOff = CreateOnOffSetting();
            itemOnOff.Init(
                Managers,
                Ticker,
                ColorProvider,
                Convert.ToBoolean(_Setting.Get()),
                _Setting.TitleKey, 
                _IsOn => _Setting.Put(ConvertValue<T>(_IsOn)));
        }

        private void InitInPanelSelectorItem<T>(ISetting<T> _Setting)
        {
            var itemSelector = CreateInPanelSelectorSetting();
            itemSelector.Init(
                Managers,
                Ticker,
                DialogViewer,
                ColorProvider,
                _Setting.TitleKey,
                () =>
                {
                    if (typeof(T) == typeof(Language))
                        return GetLanguageTitle(ConvertValue<Language>(_Setting.Get()));
                    return Convert.ToString(_Setting.Get());
                },
                _Value =>
                    {
                        itemSelector.setting.text = _Value;
                        if (typeof(T) == typeof(Language))
                        {
                            Managers.AnalyticsManager.SendAnalytic(AnalyticIds.LanguageButtonPressed);
                            var lang = LanguageTitles.FirstOrDefault(_Kvp => _Kvp.Value == _Value).Key;
                            T val = ConvertValue<T>(lang);
                            _Setting.Put(val);
                            Managers.LocalizationManager.SetLanguage(
                                Managers.LocalizationManager.GetCurrentLanguage());
                        }
                        else
                            _Setting.Put(ConvertValue<T>(_Value));
                    },
                () =>
                {
                    if (typeof(T) == typeof(Language))
                    {
                        return _Setting.Values
                            .Select(_Val => GetLanguageTitle(ConvertValue<Language>(_Val))).ToList();
                    }
                    return _Setting.Values.Cast<string>().ToList();
                },
                SelectorPanel);
        }

        private void InitRateUsButton()
        {
            var item = CreateActionSetting();
            item.Init(
                Managers,
                Ticker,
                ColorProvider,
                () =>
                {
                    Managers.AnalyticsManager.SendAnalytic(AnalyticIds.RateGameButton1Pressed);
                    Managers.ShopManager.RateGame(false);
                });
            Managers.LocalizationManager.AddTextObject(item.title, "rate_game");
            item.Highlighted = true;
        }

        private void InitLeaderboardsButton()
        {
            var item = CreateActionSetting();
            item.Init(
                Managers,
                Ticker,
                ColorProvider,
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
                });
            Managers.LocalizationManager.AddTextObject(item.title, "show_leaderboards");
        }

        private void InitRestorePurchasesButton()
        {
            var item = CreateActionSetting();
            item.Init(
                Managers,
                Ticker,
                ColorProvider,
                () =>
                {
                    Managers.AnalyticsManager.SendAnalytic(AnalyticIds.RestorePurchasesButtonPressed);
                    Managers.ShopManager.RestorePurchases();
                });
            Managers.LocalizationManager.AddTextObject(item.title, "restore_purchases");
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

        private SettingItemInPanelSelector CreateInPanelSelectorSetting()
        {
            var obj = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    m_SettingsContent,
                    m_SettingItemRectLite),
                PrefabSetName, "in_panel_selector_item");
            return obj.GetComponent<SettingItemInPanelSelector>();
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

        private T ConvertValue<T>(object _Value)
        {
            if (typeof(T).IsEnum)
                return (T) Enum.Parse(typeof(T), _Value.ToString());
            return (T) Convert.ChangeType(_Value, typeof(T));
        }

        private static string GetLanguageTitle(Language _Lang)
        {
            return LanguageTitles[_Lang];
        } 
        
        private static Dictionary<Language, string> LanguageTitles = new Dictionary<Language, string>
        {
            {Language.English, "English"},
            {Language.Portugal, "Portugués"},
            {Language.Spanish, "Español"},
            {Language.Russian, "Русский"},
            {Language.German, "Deutsch"}
        };

        #endregion
    }
}