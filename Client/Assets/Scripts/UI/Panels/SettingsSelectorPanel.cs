using System;
using System.Collections.Generic;
using Constants;
using DI.Extensions;
using DialogViewers;
using Entities;
using GameHelpers;
using LeTai.Asset.TranslucentImage;
using Ticker;
using UI.Entities;
using UI.Factories;
using UI.PanelItems.Setting_Panel_Items;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UI.Panels
{
    public interface ISettingSelectorDialogPanel : IDialogPanel
    {
        void PreInit(string _DefaultValue, List<string> _Items, Action<string> _OnSelect);
    }
    
    public class SettingsSelectorPanel : DialogPanelBase, ISettingSelectorDialogPanel
    {
        #region private members

        private List<string> m_Items;
        private Action<string> m_OnSelect;
        private string m_DefaultValue;
        private RectTransform m_Content;
        private ToggleGroup m_ToggleGroup;
        
        #endregion

        #region inject
        
        private ICameraProvider CameraProvider { get; }

        public SettingsSelectorPanel(
            IDialogViewer _DialogViewer,
            IManagersGetter _Managers,
            IUITicker _UITicker,
            ICameraProvider _CameraProvider) 
            : base(_Managers, _UITicker, _DialogViewer)
        {
            CameraProvider = _CameraProvider;
        }


        #endregion

        #region api

        public override EUiCategory Category => EUiCategory.Settings;


        public void PreInit(string _DefaultValue, List<string> _Items, Action<string> _OnSelect)
        {
            m_DefaultValue = _DefaultValue;
            m_Items = _Items;
            m_OnSelect = _OnSelect;
        }
        
        public override void Init()
        {
            base.Init();
            var sp = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    DialogViewer.Container,
                    RtrLites.FullFill),
                CommonPrefabSetNames.DialogPanels, "settings_selector_panel");
            var translBack = sp.GetCompItem<TranslucentImage>("translucent_background");
            translBack.source = CameraProvider.MainCamera.GetComponent<TranslucentImageSource>();
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
            
            var sspi = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Content,
                    sspiRect),
                "setting_items", "settings_selector_item");

            var selectorItems = new List<SettingSelectorItem>();
            foreach (var item in m_Items)
            {
                var sspiClone = sspi.Clone();
                SettingSelectorItem si = sspiClone.GetComponent<SettingSelectorItem>();
                si.Init(Managers, Ticker, item, m_OnSelect, item == m_DefaultValue);
                selectorItems.Add(si);
            }

            foreach (var selItem in selectorItems)
                selItem.SetItems(selectorItems);
            
            Object.Destroy(sspi);
        }
        
        #endregion
    }
}