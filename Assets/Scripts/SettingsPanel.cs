using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UICreationSystem.Factories;
using UICreationSystem;
using Utils;
using UnityEngine.UI;
using TMPro;
using DefaultNamespace;
using Object = UnityEngine.Object;
using System.Linq;

namespace UICreationSystem.Panels
{
    public class SettingsPanel
    {
        public static void CreatePanel(RectTransform _Container)
        {
            float indent = 75f;
            float panelIndent = 125f;
            float smallIndent = 60f;
            float positionY = -26.3f;
            float panelPositionY = -10;

            RectTransform settingPanel = UICreatorImage.Create(
                _Container,
                "settings_panel",
                UiAnchor.Create(Vector2.zero, Vector2.one),
                new Vector2(0, 10),
                Utility.HalfOne,
                new Vector2(-65, -300),
                "white_panel");

            RectTransform innerPanel = UICreatorImage.Create(
                settingPanel,
                "inner_panel",
                UiAnchor.Create(Vector2.zero, Vector2.one),
                new Vector2(0, 0),
                Utility.HalfOne,
                new Vector2(-10, -10),
                "dark_panel");

            //Setting Text
            TextMeshProUGUI settingsText = UiTmpTextFactory.Create(
                settingPanel,
                "settings_text",
                UiAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, positionY),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "textbox",
                "settings");
            settingsText.alignment = TextAlignmentOptions.Center;

            //Settingslist
            RectTransform settingsList = UICreatorImage.Create(
                innerPanel,
                "settings_list",
                UiAnchor.Create(Vector2.zero, Vector2.one),
                new Vector2(0, 0),
                Utility.HalfOne,
                new Vector2(-10, -200),
                "dark_panel");

            //ScrollRect TODO in factory
            ScrollRect ipsr = innerPanel.gameObject.AddComponent<ScrollRect>() as ScrollRect;
            ipsr.horizontal = false;
            ipsr.content = settingsList;
            ipsr.movementType = ScrollRect.MovementType.Clamped;

            //Mask  TODO in factory
            innerPanel.gameObject.AddComponent<Mask>();

            //SoundSetting
            RectTransform soundSettingPanel = UICreatorImage.Create(
                settingsList,
                "sound_settings",
                UiAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, panelPositionY),
                Utility.HalfOne,
                new Vector2(-20, 100),
                "light_panel");

            //SoundSetting text
            UiTmpTextFactory.Create(
                soundSettingPanel,
                "sound_settings_text",
                UiAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(-100, -40),
                Utility.HalfOne,
                new Vector2(-220, 52.6f),
                "textbox",
                "Sound");

            //Toggles
            soundSettingPanel.gameObject.AddComponent<ToggleGroup>();
            GameObject offToggle = (GameObject)GameObject.Instantiate(Resources.Load("prefabs/Toggle"));
            offToggle.SetParent(soundSettingPanel.gameObject);
            offToggle.transform.localScale = new Vector3(1,1,1);
            offToggle.transform.localPosition = new Vector3(0,0,0);
            offToggle.GetComponent<Toggle>().group = soundSettingPanel.GetComponent<ToggleGroup>();
            offToggle.GetComponentInChildren<Text>().text = "OFF";

            soundSettingPanel.gameObject.AddComponent<ToggleGroup>();
            GameObject onToggle = (GameObject)GameObject.Instantiate(Resources.Load("prefabs/Toggle"));
            onToggle.SetParent(soundSettingPanel.gameObject);
            onToggle.transform.localScale = new Vector3(1, 1, 1);
            onToggle.transform.localPosition = new Vector3(100, 0, 0);
            onToggle.GetComponent<Toggle>().group = soundSettingPanel.GetComponent<ToggleGroup>();
            onToggle.GetComponentInChildren<Text>().text = "ON";

            //UnityEngine.Object soundSettingPrefab = Resources.Load("prefabs/SoundSetting");
            //GameObject soundSetting = (GameObject)GameObject.Instantiate(soundSettingPrefab);
            //soundSetting.SetParent(settingsList.gameObject);
            //soundSetting.transform.localScale = new Vector3(1,1,1);
            //soundSetting.GetComponent<RectTransform>().SetLeft(5);
            //soundSetting.GetComponent<RectTransform>().SetRight(5);
            //soundSetting.GetComponent<RectTransform>().sizeDelta = new Vector2(-20, soundSetting.GetComponent<RectTransform>().sizeDelta.y);
            //soundSetting.GetComponent<RectTransform>().offsetMin = new Vector2(soundSetting.GetComponent<RectTransform>().offsetMin.x, 500);
            //soundSetting.GetComponent<RectTransform>().offsetMax = new Vector2(soundSetting.GetComponent<RectTransform>().offsetMax.x, 10);

            panelPositionY -= panelIndent;
            //LanguageSetting
            RectTransform languageSettingPanel = UICreatorImage.Create(
                settingsList,
                "language_settings",
                UiAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, panelPositionY),
                Utility.HalfOne,
                new Vector2(-20, 100),
                "light_panel");

            //LanguageSetting text
            UiTmpTextFactory.Create(
                languageSettingPanel,
                "language_settings_text",
                UiAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(-100, -40),
                Utility.HalfOne,
                new Vector2(-220, 52.6f),
                "textbox",
                "Language");

            //LanguageList
            GameObject languageDropdown = (GameObject)GameObject.Instantiate(Resources.Load("prefabs/LanguageDropdown"));
            languageDropdown.SetParent(languageSettingPanel.gameObject);
            languageDropdown.transform.localScale = new Vector3(1, 1, 1);
            languageDropdown.transform.localPosition = new Vector3(80, 0, 0);
            List<string> languageList = new List<string>(Enum.GetNames(typeof(DefaultNamespace.Language)));
            languageDropdown.GetComponent<TMP_Dropdown>().ClearOptions();
            //languageDropdown.GetComponent<TMP_Dropdown>().options = null;
            languageDropdown.GetComponent<TMP_Dropdown>().AddOptions(languageList);

            //BackButton
            RectTransform back = UICreatorImage.Create(
                settingPanel,
                "buttonBack",
                UiAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(140, -740f),
                Utility.HalfOne,
                new Vector2(-320, 52.6f),
                "buttonBackContainer");

            UiTmpButtonFactory.Create(
                back,
                "button",
                "Back",
                UiAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero,
                "buttonBack",
                () =>
                {
                    Debug.Log("buttonBack Pushed");
                    DestroyPanel(_Container);
                });
        }

        public static void DestroyPanel(RectTransform _Container)
        {
            GameObject settingsPanel = _Container.Find("settings_panel").gameObject;
            Object.Destroy(settingsPanel);
        }
    }
}