using System.Collections.Generic;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Entities.UI;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.Utils;
using RMAZOR.Managers;
using RMAZOR.UI.PanelItems.Setting_Panel_Items;
using RMAZOR.Views.Common;
using RMAZOR.Views.InputConfigurators;
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

        private RectTransform                  m_Content, m_Blackout;
        private Button                         m_ButtonClose;
        private TextMeshProUGUI                m_Title;
        private List<ELanguage>                m_Languages;
        private UnityAction<ELanguage>         m_OnSelect;
        private System.Func<ELanguage, Sprite> m_GetIconFunc;
        
        private readonly Dictionary<ELanguage, SettingLanguageItem> m_Items 
            = new Dictionary<ELanguage, SettingLanguageItem>();
        
        protected override string PrefabName => "settings_language_panel";

        #endregion

        #region inject
        
        private SettingsLanguagePanel(
            IManagersGetter             _Managers,
            IUITicker                   _UITicker,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IViewTimePauser             _TimePauser,
            IViewInputCommandsProceeder _CommandsProceeder) 
            : base(
                _Managers, 
                _UITicker,
                _CameraProvider,
                _ColorProvider,
                _TimePauser,
                _CommandsProceeder) { }
        
        #endregion

        #region api

        public override int DialogViewerId => DialogViewerIdsCommon.FullscreenCommon;
        
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
            m_Content.gameObject.DestroyChildrenSafe();
            InitItems();
        }
        
        #endregion

        #region nonpublic methods

        protected override void OnDialogStartAppearing()
        {
            base.OnDialogStartAppearing();
            EnableCurrentLanguageGraphic();
            m_Blackout.SetParent(Container.parent);
            m_Blackout.anchoredPosition = Vector2.zero;
            m_Blackout.pivot            = Vector2.one * 0.5f;
            m_Blackout.sizeDelta        = Vector2.zero;
            m_Blackout.SetParent(PanelRectTransform);
            m_Blackout.SetAsFirstSibling();
        }

        protected override void GetPrefabContentObjects(GameObject _Go)
        {
            m_Content     = _Go.GetCompItem<RectTransform>("content");
            m_Title       = _Go.GetCompItem<TextMeshProUGUI>("title");
            m_Blackout    = _Go.GetCompItem<RectTransform>("blackout");
            m_ButtonClose = _Go.GetCompItem<Button>("close_button");
        }

        protected override void LocalizeTextObjectsOnLoad()
        {
            var locInfo = new LocTextInfo(m_Title, ETextType.MenuUI_H1, "Language");
            Managers.LocalizationManager.AddLocalization(locInfo);
        }

        protected override void SubscribeButtonEvents()
        {
            m_ButtonClose.onClick.AddListener(() => OnClose());
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