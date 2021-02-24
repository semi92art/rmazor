using DialogViewers;
using Extensions;
using GameHelpers;
using UI.Entities;
using UI.Factories;
using UI.Managers;
using UnityEngine;

namespace UI.Panels
{
    public interface IProgress
    {
        void SetProgress(float _Percents, string _Info);
        void Break(string _Error);
    }
    
    public class LoadingPanel : DialogPanelBase, IMenuUiCategory, IProgress
    {
        #region nonpublic members

        private readonly IMenuDialogViewer m_DialogViewer;
        private LoadingPanelView m_View;

        #endregion
        
        #region public api

        public MenuUiCategory Category => MenuUiCategory.Loading;

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
                Constants.CommonPrefabSetNames.MainMenuDialogPanels, "loading_panel");
            m_View = prefab.GetComponent<LoadingPanelView>();
            Panel = prefab.RTransform();
        }

        public void SetProgress(float _Percents, string _Info)
        {
            m_View.SetProgress(_Percents, _Info);
        }
        
        public void Break(string _Error)
        {
            m_View.Break(_Error);
        }

        #endregion
    }
}