using System.Collections.Generic;
using Extentions;
using UICreationSystem.Factories;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;

namespace UICreationSystem.Panels
{
    public class SettingsSelectorPanel : IDialogPanel
    {
        #region private members
        
        private readonly IDialogViewer m_DialogViewer;
        private RectTransform m_Content;
        private List<string> m_Items;
        private UnityAction<string> m_Select;
        private ToggleGroup m_ToggleGroup;
        private string m_DefaultValue;
        
        #endregion

        #region api

        public UiCategory Category => UiCategory.Settings;
        public RectTransform Panel { get; private set; }
        
        public SettingsSelectorPanel(
            IDialogViewer _DialogViewer,
            string _Value,
            List<string> _Items,
            UnityAction<string> _Select)
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
            m_Content = sp.GetComponentItem<RectTransform>("content");
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