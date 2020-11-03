using Extentions;
using UICreationSystem.Factories;
using UnityEngine;

namespace UICreationSystem.Panels
{
    public interface ILoadingPanel : IDialogPanel
    {
        bool DoLoading { get; set; }
        void Hide();
    }
    
    public class LoadingPanel : ILoadingPanel
    {
        #region private members

        private LoadingPanelView m_View;
        private IDialogViewer m_DialogViewer;
        
        #endregion
        
        #region public api

        public UiCategory Category => UiCategory.Loading;
        public RectTransform Panel { get; private set; }
    
        public bool DoLoading
        {
            get => m_View.DoLoading;
            set => m_View.DoLoading = value;
        }

        public LoadingPanel(IDialogViewer _DialogViewer)
        {
            m_DialogViewer = _DialogViewer;
        }

        public void Show()
        {
            Panel = Create(m_DialogViewer);
            m_DialogViewer.Show(this);
        }
        
        public void Hide()
        {
            m_DialogViewer.Show(null, true);
        }
    
        #endregion
        
        #region private methods
        
        private RectTransform Create(
            IDialogViewer _DialogViewer)
        {
            GameObject prefab = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(_DialogViewer.DialogContainer, RtrLites.FullFill),
                "loading_panel", "loading_panel");
            m_View = prefab.GetComponent<LoadingPanelView>();
            //prefab.SetActive(false);
            return prefab.RTransform();
        }
    
        #endregion
    }
}