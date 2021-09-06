using DialogViewers;
using System.Collections.Generic;
using Extensions;
using GameHelpers;
using Settings;
using TMPro;
using UI.Entities;
using UI.Factories;
using UI.Managers;
using UI.PanelItems;
using UnityEngine;
using Constants;
using Ticker;

namespace UI.Panels
{
    public class SettingsPanel : DialogPanelBase, IMenuUiCategory
    {
        #region private members
        
        private static List<GameObject> _settingGoList;
        private static List<ISetting> _settingList;
        
        private readonly IMenuDialogViewer m_DialogViewer;
        private RectTransform m_Content;
        
        private RectTransformLite SettingItemRectLite => new RectTransformLite
        {
            Anchor = UiAnchor.Create(0, 1, 0, 1),
            AnchoredPosition = new Vector2(213f, -54.6f),
            Pivot = Vector2.one * 0.5f,
            SizeDelta = new Vector2(406f, 87f)
        };
        
        #endregion
        
        #region api
        
        public MenuUiCategory Category => MenuUiCategory.Settings;

        public SettingsPanel(IMenuDialogViewer _DialogViewer, ITicker _Ticker) : base(_Ticker)
        {
            m_DialogViewer = _DialogViewer;
        }
        
        public override void Init()
        {
            var sp = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(m_DialogViewer.Container, RtrLites.FullFill),
                CommonPrefabSetNames.MainMenuDialogPanels, "settings_panel");
            _settingGoList = new List<GameObject>();
            _settingList = new List<ISetting>();
            m_Content = sp.GetCompItem<RectTransform>("content");
            InitSettingItems();
            Panel = sp.RTransform();
        }

        #endregion

        #region nonpublic methods

        private void InitSettingItems()
        {
            var soundSetting = new SoundSetting(Ticker);
            soundSetting.AddObservers(GetObservers());
            _settingList.Add(soundSetting);
            _settingList.Add(new LanguageSetting());
            #if DEBUG
            _settingList.Add(new DebugSetting());
            #endif
            foreach (var setting  in _settingList)
            {
                InitSettingItem(setting);
            }
        }
        
        private void InitSettingItem(ISetting _Setting)
        {
            switch (_Setting.Type)
            {
                case SettingType.OnOff:
                    var itemOnOff = CreateOnOffSetting();
                    itemOnOff.Init(
                        (bool)_Setting.Get(),
                        _Setting.Name, 
                        _IsOn =>
                    {
                        _Setting.Put(_IsOn);
                    }, GetObservers(), Ticker);
                    break;
                case SettingType.InPanelSelector:
                    var itemSelector = CreateInPanelSelectorSetting();
                    itemSelector.Init(
                        m_DialogViewer,
                        () => (string)_Setting.Get(),
                        _Setting.Name,
                        () => _Setting.Values,
                        _Value =>
                        {
                            itemSelector.setting.text = _Value;
                            _Setting.Put(_Value);
                        }, GetObservers(), Ticker);
                    break;
                case SettingType.Slider:
                    var itemSlider = CreateSliderSetting();
                    bool wholeNumbers = _Setting.Get() is int;
                    if (wholeNumbers)
                        itemSlider.Init(_Setting.Name, (float)_Setting.Min, (float)_Setting.Max, (float)_Setting.Get());
                    else
                        itemSlider.Init(_Setting.Name, (int)_Setting.Min, (int)_Setting.Max, (int)_Setting.Get());
                    break;
            }
        }

        public static void UpdateSetting()
        {
            for (int i = 0; i < _settingList.Count; i++)
            {
                TextMeshProUGUI tmp = _settingGoList[i].transform.Find("Title").GetComponent<TextMeshProUGUI>();
                tmp.text = _settingList[i].Name;
            }
        }

        private SettingItemOnOff CreateOnOffSetting()
        {
            GameObject obj = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Content,
                    SettingItemRectLite),
                "setting_items", "on_off_item");
            _settingGoList.Add(obj);
            return obj.GetComponent<SettingItemOnOff>();
        }

        private SettingItemInPanelSelector CreateInPanelSelectorSetting()
        {
            GameObject obj = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Content,
                    SettingItemRectLite),
                "setting_items", "in_panel_selector_item");
            _settingGoList.Add(obj);
            return obj.GetComponent<SettingItemInPanelSelector>();
        }


        private SettingItemSlider CreateSliderSetting()
        {
            GameObject obj = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Content,
                    SettingItemRectLite),
                "setting_items", "slider_item");
            _settingGoList.Add(obj);
            return obj.GetComponent<SettingItemSlider>();
        }
        
        #endregion
    }
}