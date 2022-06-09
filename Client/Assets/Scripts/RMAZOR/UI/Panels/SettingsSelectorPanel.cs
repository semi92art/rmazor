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
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.UI.PanelItems.Setting_Panel_Items;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.Panels
{
    public interface ISettingSelectorDialogPanel : IDialogPanel
    {
        void PreInit(string _DefaultValue, List<object> _Items, UnityAction<object> _OnSelect);
    }
    
    public class SettingsSelectorPanel : DialogPanelBase, ISettingSelectorDialogPanel
    {
        #region private members

        private List<object>        m_Items;
        private UnityAction<object> m_OnSelect;
        private string              m_DefaultValue;
        private RectTransform       m_Content;
        private ToggleGroup         m_ToggleGroup;
        
        #endregion

        #region inject
        
        private SettingsSelectorPanel(
            IBigDialogViewer _DialogViewer,
            IManagersGetter  _Managers,
            IUITicker        _UITicker,
            ICameraProvider  _CameraProvider,
            IColorProvider   _ColorProvider) 
            : base(
                _Managers, 
                _UITicker,
                _DialogViewer,
                _CameraProvider,
                _ColorProvider) { }
        
        #endregion

        #region api

        public override EUiCategory Category      => EUiCategory.Settings;
        public override bool        AllowMultiple => true;


        public void PreInit(string _DefaultValue, List<object> _Items, UnityAction<object> _OnSelect)
        {
            m_DefaultValue = _DefaultValue;
            m_Items = _Items;
            m_OnSelect = _OnSelect;
        }
        
        public override void LoadPanel()
        {
            base.LoadPanel();
            var sp = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    DialogViewer.Container,
                    RectTransformLite.FullFill),
                CommonPrefabSetNames.DialogPanels, "settings_selector_panel");
            m_ToggleGroup = sp.AddComponent<ToggleGroup>();
            m_Content = sp.GetCompItem<RectTransform>("content");
            InitItems();
            PanelObject = sp.RTransform();
        }
        
        #endregion

        #region nonpublic methods
 
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

            var selectorItems = new List<SettingSelectorItem>();
            foreach (var item in m_Items)
            {
                var sspiClone = sspi.Clone();
                SettingSelectorItem si = sspiClone.GetComponent<SettingSelectorItem>();
                si.Init(
                    Ticker,
                    ColorProvider,
                    Managers.AudioManager,
                    Managers.LocalizationManager,
                    Managers.PrefabSetManager,
                    item is ELanguage lang ? LocalizationUtils.GetLanguageTitle(lang) : item.ToString(),
                    item,
                    m_OnSelect,
                    item.ToString() == m_DefaultValue,
                    () => item is ELanguage lang ?
                        Managers.LocalizationManager.GetFont(ETextType.MenuUI, lang) : null);
                selectorItems.Add(si);
            }

            foreach (var selItem in selectorItems)
                selItem.SetItems(selectorItems);
            
            Object.Destroy(sspi);
        }
        
        #endregion
    }
}