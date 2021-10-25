using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using DI.Extensions;
using DialogViewers;
using Entities;
using GameHelpers;
using LeTai.Asset.TranslucentImage;
using Settings;
using Ticker;
using TMPro;
using UI.Entities;
using UI.Factories;
using UI.PanelItems.Setting_Panel_Items;
using UnityEngine;
using Utils;

namespace UI.Panels
{
    public interface ISettingDialogPanel : IDialogPanel { }
    
    public class SettingsPanel : DialogPanelBase, ISettingDialogPanel
    {
        #region private members

        private readonly Dictionary<TextMeshProUGUI, string> m_settingTitles
            = new Dictionary<TextMeshProUGUI, string>();
        private RectTransform m_MiniButtonsContent;
        private RectTransform m_SettingsContent;
        
        private RectTransformLite m_SettingItemRectLite = new RectTransformLite
        {
            Anchor = UiAnchor.Create(0, 1, 0, 1),
            AnchoredPosition = new Vector2(213f, -54.6f),
            Pivot = Vector2.one * 0.5f,
            SizeDelta = new Vector2(406f, 87f)
        };

        private RectTransformLite m_MiniButtonRectLite = new RectTransformLite
        {
            Anchor = UiAnchor.Create(0, 1, 0, 1),
            AnchoredPosition = new Vector2(0f, 0f),
            Pivot = Vector2.one * 0.5f,
            SizeDelta = new Vector2(87f, 87f)
        };
        
        #endregion

        #region inject
        
        private ISettingSelectorDialogPanel SelectorPanel { get; }
        private ISettingsGetter SettingsGetter { get; }
        private ICameraProvider CameraProvider { get; }

        public SettingsPanel(
            ISettingSelectorDialogPanel _SelectorPanel,
            IDialogViewer _DialogViewer,
            IManagersGetter _Managers,
            IUITicker _UITicker,
            ISettingsGetter _SettingsGetter,
            ICameraProvider _CameraProvider) 
            : base(_Managers, _UITicker, _DialogViewer)
        {
            SelectorPanel = _SelectorPanel;
            SettingsGetter = _SettingsGetter;
            CameraProvider = _CameraProvider;
        }

        #endregion
        
        #region api
        
        public override EUiCategory Category => EUiCategory.Settings;
        
        public override void Init()
        {
            base.Init();
            var sp = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(DialogViewer.Container, RtrLites.FullFill),
                CommonPrefabSetNames.DialogPanels, "settings_panel");
            m_MiniButtonsContent = sp.GetCompItem<RectTransform>("mini_buttons_content");
            m_SettingsContent = sp.GetCompItem<RectTransform>("settings_content");
            var translBack = sp.GetCompItem<TranslucentImage>("translucent_background");
            translBack.source = CameraProvider.MainCamera.GetComponent<TranslucentImageSource>();
            
            m_MiniButtonsContent.gameObject.DestroyChildrenSafe();
            m_SettingsContent.gameObject.DestroyChildrenSafe();
            InitSettingItems();
            Panel = sp.RTransform();
        }

        #endregion

        #region nonpublic methods

        private void InitSettingItems()
        {
            InitSettingItem(SettingsGetter.SoundSetting);
            InitSettingItem(SettingsGetter.MusicSetting);
            InitSettingItem(SettingsGetter.LanguageSetting);
            // InitSettingItem(SettingsGetter.NotificationSetting);
            InitSettingItem(SettingsGetter.VibrationSetting);
            InitRateUsButton();
            InitLeaderboardsButton();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            InitSettingItem(SettingsGetter.DebugSetting);
#endif
        }
        
        private void InitSettingItem<T>(ISetting<T> _Setting)
        {
            switch (_Setting.Location)
            {
                case ESettingLocation.MiniButtons:
                    var itemMiniButton = CreateMiniButtonSetting();
                    itemMiniButton.Init(
                        Managers,
                        Ticker,
                        Convert.ToBoolean(_Setting.Get()),
                        _IsOn => _Setting.Put(ConvertValue<T>(_IsOn)),
                        GetSettingsIconFromPrefabs(_Setting.SpriteOnKey),
                        GetSettingsIconFromPrefabs(_Setting.SpriteOffKey));
                    break;
                case ESettingLocation.Main:
                    switch (_Setting.Type)
                    {
                        case ESettingType.OnOff:
                            var itemOnOff = CreateOnOffSetting();
                            Managers.LocalizationManager.AddTextObject(itemOnOff.title, _Setting.TitleKey);
                            itemOnOff.Init(
                                Managers,
                                Ticker,
                                Convert.ToBoolean(_Setting.Get()),
                                _Setting.TitleKey, 
                                _IsOn => _Setting.Put(ConvertValue<T>(_IsOn)));
                            break;
                        case ESettingType.InPanelSelector:
                            var itemSelector = CreateInPanelSelectorSetting();
                            Managers.LocalizationManager.AddTextObject(itemSelector.title, _Setting.TitleKey);
                            itemSelector.Init(
                                Managers,
                                Ticker,
                                DialogViewer,
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
                                            Dbg.Log("Selected language: " + _Value);
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
                            break;
                        case ESettingType.Slider:
                            var itemSlider = CreateSliderSetting();
                            Managers.LocalizationManager.AddTextObject(itemSlider.title, _Setting.TitleKey);
                            bool wholeNumbers = _Setting.Get() is int;
                            itemSlider.Init(
                                _Setting.TitleKey, 
                                (float)_Setting.Min,
                                (float)_Setting.Max, 
                                Convert.ToSingle(_Setting.Get()),
                                wholeNumbers);
                            break;
                    }
                    break;
            }
        }

        private void InitRateUsButton()
        {
            var item = CreateActionSetting();
            item.Init(
                Managers,
                Ticker,
                Managers.LocalizationManager.GetTranslation("rate_game"),
                () => Managers.ShopManager.GoToRatePage());
            Managers.LocalizationManager.AddTextObject(item.title, "rate_game");
        }

        private void InitLeaderboardsButton()
        {
            var item = CreateActionSetting();
            item.Init(
                Managers,
                Ticker,
                Managers.LocalizationManager.GetTranslation("show_leaderboards"),
                () => Managers.ShopManager.GoToRatePage());
            Managers.LocalizationManager.AddTextObject(item.title, "show_leaderboards");
        }

        private SettingItemMiniButton CreateMiniButtonSetting()
        {
            var obj = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_MiniButtonsContent,
                    m_MiniButtonRectLite),
                GetPrefabSetName(), "mini_button_item");
            return obj.GetComponent<SettingItemMiniButton>();
        }

        private SettingItemOnOff CreateOnOffSetting()
        {
            var obj = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_SettingsContent,
                    m_SettingItemRectLite),
                GetPrefabSetName(), "on_off_item");
            return obj.GetComponent<SettingItemOnOff>();
        }

        private SettingItemInPanelSelector CreateInPanelSelectorSetting()
        {
            var obj = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_SettingsContent,
                    m_SettingItemRectLite),
                GetPrefabSetName(), "in_panel_selector_item");
            return obj.GetComponent<SettingItemInPanelSelector>();
        }
        
        private SettingItemSlider CreateSliderSetting()
        {
            var obj = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_SettingsContent,
                    m_SettingItemRectLite),
                GetPrefabSetName(), "slider_item");
            return obj.GetComponent<SettingItemSlider>();
        }

        private SettingItemAction CreateActionSetting()
        {
            var obj = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_SettingsContent,
                    m_SettingItemRectLite),
                GetPrefabSetName(), "action_item");
            return obj.GetComponent<SettingItemAction>();
        }

        private Sprite GetSettingsIconFromPrefabs(string _Key)
        {
            return PrefabUtilsEx.GetObject<Sprite>(GetPrefabSetName(), _Key);
        }

        private string GetPrefabSetName()
        {
            return "setting_items";
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