using System;
using System.Collections.Generic;
using Settings;
using UICreationSystem.Factories;
using UnityEngine;
using Utils;

namespace UICreationSystem
{
    public class SettingsPanelUi
    {
        private RectTransform m_LoadingPanel;
        private RectTransform m_Content;

        public static SettingsPanelUi Create(
            RectTransform _Parent)
        {
            SettingsPanelUi res = new SettingsPanelUi();
            res.m_LoadingPanel = UiFactory.UiRectTransform(
                _Parent, "SettingsPanel", RtrLites.DialogWindow);
            GameObject prefab = PrefabInitializer.InitUiPrefab(
                res.m_LoadingPanel, "settings_panel", "settings_panel");
            res.m_Content = prefab.GetContentItemRTransform("content");
            return res;
        }

        private void InitSettingItems()
        {
            InitSettingItem(new SoundSetting());
            InitSettingItem(new LanguageSetting());
        }

        private void InitSettingItem(ISetting _Setting)
        {
            // GameObject sItem = PrefabInitializer.InitUiPrefab(
            //     UiFactory.UiRectTransform(
            //         m_Content,
            //         $"Setting {_Setting.GetType().Name}",
            //         
            //         )
            //     )
        }
    }
}