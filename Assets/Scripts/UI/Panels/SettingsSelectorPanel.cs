using System.Collections.Generic;
using Extensions;
using Helpers;
using UI.Entities;
using UI.Factories;
using UI.Managers;
using UI.PanelItems;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels
{
    public class SettingsSelectorPanel : IDialogPanel
    {
        #region private members
        
        private readonly IDialogViewer m_DialogViewer;
        private readonly List<string> m_Items;
        private readonly System.Action<string> m_Select;
        private readonly string m_DefaultValue;
        private RectTransform m_Content;
        private ToggleGroup m_ToggleGroup;
        
        #endregion

        #region api

        public UiCategory Category => UiCategory.Settings;
        public RectTransform Panel { get; private set; }
        
        public SettingsSelectorPanel(
            IDialogViewer _DialogViewer,
            string _Value,
            List<string> _Items,
            System.Action<string> _Select)
        {
            m_DialogViewer = _DialogViewer;
            m_DefaultValue = _Value;
            m_Items = _Items;
            m_Select = _Select;
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
            var sp = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "main_menu", "settings_selector_panel");

            m_ToggleGroup = sp.AddComponent<ToggleGroup>();
            m_Content = sp.GetCompItem<RectTransform>("content");
            InitItems();
            return sp.RTransform();
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
            
            GameObject sspi = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Content,
                    sspiRect),
                "setting_items", "settings_selector_item");
            
            foreach (var item in m_Items)
            {
                var sspiClone = sspi.Clone();
                SettingSelectorItem si = sspiClone.GetComponent<SettingSelectorItem>();
                bool selected = item == m_DefaultValue;
                si.Init(selected, item, m_Select, m_ToggleGroup);
            }
            
            Object.Destroy(sspi);
        }
        
        #endregion
    }
}