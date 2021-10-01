using System.Collections.Generic;
using System.Linq;
using Constants;
using DI.Extensions;
using DialogViewers;
using Entities;
using GameHelpers;
using Ticker;
using UI.Factories;
using UI.Managers;
using UnityEngine;

namespace UI.Panels
{
    public interface ILoadingHandler
    {
        void SetProgress(float _Percents, IEnumerable<string> _Infos);
        void Break(string _Error);
    }
    
    public class LoadingPanel : DialogPanelBase, IMenuUiCategory, ILoadingHandler
    {
        #region nonpublic members

        private readonly IMenuDialogViewer m_DialogViewer;
        private LoadingPanelView m_View;

        #endregion
        
        #region api

        public MenuUiCategory Category => MenuUiCategory.Loading;

        public LoadingPanel(
            IMenuDialogViewer _DialogViewer,
            IManagersGetter _Managers,
            IUITicker _UITicker) : base(_Managers, _UITicker)
        {
            m_DialogViewer = _DialogViewer;
        }

        public override void Init()
        {
            base.Init();
            GameObject prefab = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.Container,
                    RtrLites.FullFill),
                CommonPrefabSetNames.MainMenuDialogPanels, "loading_panel");
            m_View = prefab.GetComponent<LoadingPanelView>();
            Panel = prefab.RTransform();
        }

        public void SetProgress(float _Percents, IEnumerable<string> _Infos)
        {
            m_View.SetProgress(_Percents, _Infos.Last());
        }
        
        public void Break(string _Error)
        {
            m_View.Break(_Error);
        }

        #endregion
    }
}