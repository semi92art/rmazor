﻿using Extensions;
using UnityEngine;
using UICreationSystem.Factories;
using Settings;

namespace UICreationSystem.Panels
{
    public class SettingsPanel : IDialogPanel
    {
        #region private members
        
        private readonly IDialogViewer m_DialogViewer;
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
        
        public UiCategory Category => UiCategory.Settings;
        public RectTransform Panel { get; private set; }

        public SettingsPanel(IDialogViewer _DialogViewer)
        {
            m_DialogViewer = _DialogViewer;
        }
        
        public void Show()
        {
            Panel = Create();
            m_DialogViewer.Show(this);
        }
        
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
            InitSettingItem(new SoundSetting());
            InitSettingItem(new LanguageSetting());
            #if DEBUG
            InitSettingItem(new DebugSetting());
            #endif
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

        private SettingItemOnOff CreateOnOffSetting()
        {
            GameObject obj = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Content,
                    SettingItemRectLite),
                "setting_items", "on_off_item");
            return obj.GetComponent<SettingItemOnOff>();
        }

        private SettingItemInPanelSelector CreateInPanelSelectorSetting()
        {
            GameObject obj = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Content,
                    SettingItemRectLite),
                "setting_items", "in_panel_selector_item");
            return obj.GetComponent<SettingItemInPanelSelector>();
        }


        private SettingItemSlider CreateSliderSetting()
        {
            GameObject obj = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Content,
                    SettingItemRectLite),
                "setting_items", "slider_item");
            return obj.GetComponent<SettingItemSlider>();
        }
        
        #endregion
    }
}