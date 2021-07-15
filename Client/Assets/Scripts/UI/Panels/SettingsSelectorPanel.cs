﻿using System.Collections.Generic;
using DialogViewers;
using Entities;
using Extensions;
using GameHelpers;
using UI.Entities;
using UI.Factories;
using UI.Managers;
using UI.PanelItems;
using UnityEngine;
using UnityEngine.UI;
using Constants;

namespace UI.Panels
{
    public class SettingsSelectorPanel : DialogPanelBase, IMenuUiCategory
    {
        #region private members
        
        private readonly IMenuDialogViewer m_DialogViewer;
        private readonly List<string> m_Items;
        private readonly System.Action<string> m_Select;
        private readonly string m_DefaultValue;
        private RectTransform m_Content;
        private ToggleGroup m_ToggleGroup;
        
        #endregion

        #region api

        public MenuUiCategory Category => MenuUiCategory.Settings;

        public SettingsSelectorPanel(
            IMenuDialogViewer _DialogViewer,
            string _Value,
            List<string> _Items,
            System.Action<string> _Select)
        {
            m_DialogViewer = _DialogViewer;
            m_DefaultValue = _Value;
            m_Items = _Items;
            m_Select = _Select;
        }
        
        public override void Init()
        {
            var sp = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.Container,
                    RtrLites.FullFill),
                CommonPrefabSetNames.MainMenuDialogPanels, "settings_selector_panel");

            m_ToggleGroup = sp.AddComponent<ToggleGroup>();
            m_Content = sp.GetCompItem<RectTransform>("content");
            InitItems();
            Panel = sp.RTransform();
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
            
            GameObject sspi = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Content,
                    sspiRect),
                "setting_items", "settings_selector_item");

            var selectorItems = new List<SettingSelectorItem>();
            foreach (var item in m_Items)
            {
                var sspiClone = sspi.Clone();
                SettingSelectorItem si = sspiClone.GetComponent<SettingSelectorItem>();
                si.Init(item, m_Select, item == m_DefaultValue, GetObservers());
                selectorItems.Add(si);
            }

            foreach (var selItem in selectorItems)
                selItem.SetItems(selectorItems);
            
            Object.Destroy(sspi);
        }
        
        #endregion
    }
}