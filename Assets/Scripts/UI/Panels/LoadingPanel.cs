using DialogViewers;
using Extensions;
using Helpers;
using UI.Entities;
using UI.Factories;
using UI.Managers;
using UnityEngine;

namespace UI.Panels
{
    public interface ILoadingPanel : IMenuDialogPanel
    {
        bool DoLoading { get; set; }
        void Hide();
    }
    
    public class LoadingPanel : ILoadingPanel
    {
        #region nonpublic members

        private readonly IMenuDialogViewer m_DialogViewer;
        private LoadingPanelView m_View;
        
        #endregion
        
        #region public api

        public MenuUiCategory Category => MenuUiCategory.Loading;
        public RectTransform Panel { get; private set; }
    
        public bool DoLoading
        {
            get => m_View.DoLoading;
            set => m_View.DoLoading = value;
        }

        public LoadingPanel(IMenuDialogViewer _DialogViewer)
        {
            m_DialogViewer = _DialogViewer;
        }

        public void Show()
        {
            Panel = Create(m_DialogViewer);
            m_DialogViewer.Show(this);
        }

        public void OnEnable() { }

        public void Hide()
        {
            m_DialogViewer.Back();
        }
    
        #endregion
        
        #region nonpublic methods
        
        private RectTransform Create(
            IMenuDialogViewer _MenuDialogViewer)
        {
            GameObject prefab = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    _MenuDialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "loading_panel", "loading_panel");
            m_View = prefab.GetComponent<LoadingPanelView>();
            return prefab.RTransform();
        }
    
        #endregion
    }
}