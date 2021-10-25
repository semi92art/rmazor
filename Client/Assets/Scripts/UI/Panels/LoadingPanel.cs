using System.Collections.Generic;
using System.Linq;
using Constants;
using DI.Extensions;
using DialogViewers;
using Entities;
using GameHelpers;
using Ticker;
using UI.Factories;
using UnityEngine;

namespace UI.Panels
{

    
    public interface ILoadingDialogPanel : IDialogPanel
    {
        void SetProgress(float _Percents, IEnumerable<string> _Infos);
        void Break(string _Error);
    }
    
    public class LoadingPanel : DialogPanelBase, ILoadingDialogPanel
    {
        #region nonpublic members

        private LoadingPanelView m_View;

        #endregion

        #region inject

        public LoadingPanel(
            IDialogViewer _DialogViewer,
            IManagersGetter _Managers,
            IUITicker _UITicker) 
            : base(_Managers, _UITicker, _DialogViewer) { }

        #endregion
        
        #region api

        public override EUiCategory Category => EUiCategory.Loading;

        public override void Init()
        {
            base.Init();
            GameObject prefab = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    DialogViewer.Container,
                    RtrLites.FullFill),
                CommonPrefabSetNames.DialogPanels, "loading_panel");
            m_View = prefab.GetComponent<LoadingPanelView>();
            m_View.Init(Ticker);
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