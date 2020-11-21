using System.Collections.Generic;
using DialogViewers;
using Extensions;
using Helpers;
using UI.Entities;
using UI.Factories;
using UI.Managers;
using UI.PanelItems;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Panels
{
    public class CountriesPanel : MonoBehaviour, IMenuDialogPanel
    {
        #region private members
        
        private IMenuDialogViewer m_MenuDialogViewer;
        private System.Action<string> m_Select;
        private string m_DefaultValue;
        private RectTransform m_Content;
        private RectTransform m_Viewport;
        private ToggleGroup m_ToggleGroup;
        private Dictionary<CountrySelectItem, RectTransform> m_ItemsDict = 
            new Dictionary<CountrySelectItem, RectTransform>();
        
        #endregion
        
        #region engine methods

        private void Update()
        {
            CheckItemsVisibility();
        }
        
        #endregion
        
        #region api
        
        public MenuUiCategory Category => MenuUiCategory.Countries;
        public RectTransform Panel { get; private set; }

        public void Init(
            IMenuDialogViewer _MenuDialogViewer,
            string _Value,
            System.Action<string> _Select)
        {
            m_MenuDialogViewer = _MenuDialogViewer;
            m_DefaultValue = _Value;
            m_Select = _Select;
        }
        
        public void Show()
        {
            Panel = Create();
            m_MenuDialogViewer.Show(this);
        }
        
        private RectTransform Create()
        {
            var sp = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_MenuDialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "main_menu", "settings_selector_panel");
            
            m_ToggleGroup = sp.AddComponent<ToggleGroup>();
            m_Content = sp.GetCompItem<RectTransform>("content");
            m_Viewport = sp.GetCompItem<RectTransform>("viewport");
            InitItems();
            m_Content.anchoredPosition = m_Content.anchoredPosition.SetY(0);
            return sp.RTransform();
        }

        private void InitItems()
        {
            RectTransformLite sspiRect = new RectTransformLite
            {
                Anchor = UiAnchor.Create(0, 1, 0, 1),
                AnchoredPosition = new Vector2(213, -60),
                Pivot = Vector2.one * 0.5f,
                SizeDelta = new Vector2(406, 70)
            };
            
            GameObject sspi = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Content,
                    sspiRect),
                "main_menu", "country_select_item");
            
            foreach (var item in Countries.Keys)
            {
                var sspiClone = sspi.Clone();
                CountrySelectItem si = sspiClone.GetComponent<CountrySelectItem>();
                bool selected = item == m_DefaultValue;
                si.Init(selected, item, m_Select, m_ToggleGroup);
                m_ItemsDict.Add(si, si.gameObject.RTransform());
            }
            
            Destroy(sspi);
        }

        private void CheckItemsVisibility()
        {
            foreach (var item in m_ItemsDict)
            {
                bool visible = item.Value.IsVisibleFrom(m_Viewport);
                item.Key.SetVisible(visible);
            }
        }
        
        #endregion
    }
}