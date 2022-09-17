using System.Collections.Generic;
using Common.CameraProviders;
using Common.Constants;
using Common.Entities.UI;
using Common.Enums;
using Common.Extensions;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.UI;
using Common.UI.DialogViewers;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.UI.PanelItems.Setting_Panel_Items;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RMAZOR.UI.Panels
{
    public interface ISettingLanguageDialogPanel : IDialogPanel
    {
        void PreInit(
            List<ELanguage>                _Languages,
            UnityAction<ELanguage>         _OnSelect,
            System.Func<ELanguage, Sprite> _GetIconFunc);
    }
    
    public class SettingsLanguagePanel : DialogPanelBase, ISettingLanguageDialogPanel
    {
        #region private members

        private List<ELanguage>                m_Languages;
        private UnityAction<ELanguage>         m_OnSelect;
        private RectTransform                  m_Content;
        private System.Func<ELanguage, Sprite> m_GetIconFunc;
        private TextMeshProUGUI                m_Title;
        private RectTransform                  m_Blackout;
        
        private readonly Dictionary<ELanguage, SettingLanguageItem> m_Items 
            = new Dictionary<ELanguage, SettingLanguageItem>();

        #endregion

        #region inject
        
        private SettingsLanguagePanel(
            IManagersGetter          _Managers,
            IUITicker                _UITicker,
            ICameraProvider          _CameraProvider,
            IColorProvider           _ColorProvider) 
            : base(
                _Managers, 
                _UITicker,
                _CameraProvider,
                _ColorProvider) { }
        
        #endregion

        #region api

        public override EDialogViewerType DialogViewerType => EDialogViewerType.Medium1;
        public override EUiCategory       Category         => EUiCategory.Settings;
        
        public void PreInit(
            List<ELanguage>                _Languages,
            UnityAction<ELanguage>         _OnSelect,
            System.Func<ELanguage, Sprite> _GetIconFunc)
        {
            m_Languages   = _Languages;
            m_OnSelect    = _OnSelect;
            m_GetIconFunc = _GetIconFunc;
        }
        
        public override void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanel(_Container, _OnClose);
            var sp = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    _Container,
                    RectTransformLite.FullFill),
                CommonPrefabSetNames.DialogPanels, "settings_language_panel");
            m_Content   = sp.GetCompItem<RectTransform>("content");
            m_Title     = sp.GetCompItem<TextMeshProUGUI>("title");
            m_Blackout  = sp.GetCompItem<RectTransform>("blackout");
            var locInfo = new LocalizableTextObjectInfo(m_Title, ETextType.MenuUI, "Language");
            Managers.LocalizationManager.AddTextObject(locInfo);
            m_Content.gameObject.DestroyChildrenSafe();
            var closeButton = sp.GetCompItem<Button>("close_button");
            closeButton.onClick.AddListener(() => base.OnClose(null));
            InitItems();
            PanelRectTransform = sp.RTransform();
            PanelRectTransform.SetGoActive(false);
        }
        
        #endregion

        #region nonpublic methods

        public override void OnDialogStartAppearing()
        {
            base.OnDialogStartAppearing();
            EnableCurrentLanguageGraphic();
            m_Blackout.SetParent(Container.parent);
            m_Blackout.anchoredPosition = Vector2.zero;
            m_Blackout.pivot = Vector2.one * 0.5f;
            m_Blackout.sizeDelta = Vector2.zero;
            m_Blackout.SetParent(PanelRectTransform);
            m_Blackout.SetAsFirstSibling();
        }

        private void InitItems()
        {
            RectTransformLite sspiRect = new RectTransformLite
            {
                Anchor = UiAnchor.Create(0, 1, 0, 1),
                AnchoredPosition = new Vector2(213, -60),
                Pivot = Vector2.one * 0.5f,
                SizeDelta = new Vector2(406, 100)
            };
            var sspi = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    m_Content,
                    sspiRect),
                "setting_items", "settings_selector_item");
            var selectorItems = new List<SettingLanguageItem>();
            foreach (var language in m_Languages)
            {
                var sspiClone = sspi.Clone();
                SettingLanguageItem si = sspiClone.GetComponent<SettingLanguageItem>();
                si.Init(
                    Ticker,
                    Managers.AudioManager,
                    Managers.LocalizationManager,
                    language,
                    m_OnSelect,
                    m_GetIconFunc,
                    () => AppearingState == EAppearingState.Appeared);
                selectorItems.Add(si);
                m_Items.Add(language, si);
            }
            foreach (var selItem in selectorItems)
                selItem.SetItems(selectorItems);
            Object.Destroy(sspi);
        }

        private void EnableCurrentLanguageGraphic()
        {
            var currentLanguage = Managers.LocalizationManager.GetCurrentLanguage();
            foreach (var (key, value) in m_Items)
            {
                if (key == currentLanguage)
                    value.Select();
                else value.DeSelect();
            }
        }

        #endregion
    }
}