using DialogViewers;
using Extensions;
using GameHelpers;
using UI.Entities;
using UI.Factories;
using UI.Managers;
using UnityEngine;

namespace UI.Panels
{
    public class LoadingPanel : DialogPanelBase, IMenuUiCategory
    {
        #region nonpublic members

        private readonly IMenuDialogViewer m_DialogViewer;
        private LoadingPanelView m_View;
        
        #endregion
        
        #region public api

        public MenuUiCategory Category => MenuUiCategory.Loading;

        public bool DoLoading
        {
            get => m_View.DoLoading;
            set => m_View.DoLoading = value;
        }

        public LoadingPanel(IMenuDialogViewer _DialogViewer)
        {
            m_DialogViewer = _DialogViewer;
        }

        public override void Init()
        {
            GameObject prefab = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.Container,
                    RtrLites.FullFill),
                Constants.CommonStyleNames.MainMenuDialogPanels, "loading_panel");
            m_View = prefab.GetComponent<LoadingPanelView>();
            Panel = prefab.RTransform();
        }

    
        #endregion
    }
}