using DialogViewers;
﻿using System.Collections;
using System.Collections.Generic;
using Extensions;
using Helpers;
using Settings;
using TMPro;
using UI.Entities;
using UI.Factories;
using UI.Managers;
using UI.PanelItems;
using UnityEngine;

namespace UI.Panels
{
    public class SettingsPanel : IMenuDialogPanel
    {
        #region private members
        
        private readonly IMenuDialogViewer m_DialogViewer;
        private RectTransform m_Content;
        public static List<GameObject> SettingGOList = new List<GameObject>();
        public static List<ISetting> SettingList = new List<ISetting>();
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
        public RectTransform Panel { get; private set; }

        public SettingsPanel(IMenuDialogViewer _DialogViewer)
        {
            m_DialogViewer = _DialogViewer;
        }
        
        public void Show()
        {
            Panel = Create();
            m_DialogViewer.Show(this);
        }

        public void OnEnable() { }

        #endregion

        #region private methods
        
        private RectTransform Create()
        {
            GameObject sp = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "main_menu", "settings_panel");

            m_Content = sp.GetCompItem<RectTransform>("content");
            InitSettingItems();
            return sp.RTransform();
        }
        
        
        private void InitSettingItems()
        {
            SettingList.Add(new SoundSetting());
            SettingList.Add(new LanguageSetting());
            #if DEBUG
            SettingList.Add(new DebugSetting());
            #endif
            foreach (ISetting setting  in SettingList)
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
                    itemOnOff.Init((bool)_Setting.Get(), _Setting.Name, _IsOn =>
                    {
                        _Setting.Put(_IsOn);
                    });
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
                        });
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
            for (int i = 0; i < SettingList.Count; i++)
            {
                TextMeshProUGUI tmp = SettingGOList[i].transform.Find("Title").GetComponent<TextMeshProUGUI>();
                tmp.text = SettingList[i].Name;
            }
        }

        private SettingItemOnOff CreateOnOffSetting()
        {
            GameObject obj = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Content,
                    SettingItemRectLite),
                "setting_items", "on_off_item");
            SettingGOList.Add(obj);
            return obj.GetComponent<SettingItemOnOff>();
        }

        private SettingItemInPanelSelector CreateInPanelSelectorSetting()
        {
            GameObject obj = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Content,
                    SettingItemRectLite),
                "setting_items", "in_panel_selector_item");
            SettingGOList.Add(obj);
            return obj.GetComponent<SettingItemInPanelSelector>();
        }


        private SettingItemSlider CreateSliderSetting()
        {
            GameObject obj = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Content,
                    SettingItemRectLite),
                "setting_items", "slider_item");
            SettingGOList.Add(obj);
            return obj.GetComponent<SettingItemSlider>();
        }
        
        #endregion
    }
}