using System;
using System.Collections.Generic;
using System.Globalization;
using Common.Entities;
using Common.Extensions;
using Common.Managers;
using Common.UI;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using RMAZOR.UI.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Setting_Panel_Items
{
    public class SettingLanguageSelectorItem : SimpleUiItemBase
    {
        public                   Image           languageIcon;
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Button          button;

        private Func<string> m_Font;
        
        public void Init(
            IUITicker                   _UITicker,
            IAudioManager               _AudioManager,
            ILocalizationManager        _LocalizationManager,
            IDialogViewer               _DialogViewer,
            UnityAction<ELanguage>      _OnSelect,
            Func<List<ELanguage>>       _Languages,
            ISettingLanguageDialogPanel _LanguagePanel,
            Func<ELanguage, Sprite>     _GetIconFunc)
        {
            base.Init(_UITicker, _AudioManager, _LocalizationManager);
            var currentLang = _LocalizationManager.GetCurrentLanguage();
            languageIcon.sprite = _GetIconFunc(currentLang);
            var locInfo = new LocalizableTextObjectInfo(title, ETextType.MenuUI, "Language",
                _T => _T.FirstCharToUpper(CultureInfo.CurrentCulture));
            bool langPanelLoaded = false;
            _LocalizationManager.AddTextObject(locInfo);
            void OnClick()
            {
                SoundOnClick();
                var items = _Languages?.Invoke();
                if (!langPanelLoaded)
                {
                    _LanguagePanel.PreInit(
                        items, 
                        _OnSelect,
                        _GetIconFunc);
                    _LanguagePanel.LoadPanel(_DialogViewer.Container, _DialogViewer.Back);
                    langPanelLoaded = true;
                }
                _DialogViewer.Show(_LanguagePanel);
            }
            button.SetOnClick(OnClick);
        }
    }
}