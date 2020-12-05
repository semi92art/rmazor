using DialogViewers;
using Extensions;
using Helpers;
using UI.Factories;
using UI.Managers;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Panels
{
    public class CountdownPanel : IGameDialogPanel
    {
        #region nonpublic members
        
        private readonly IGameDialogViewer m_DialogViewer;
        private readonly UnityAction m_OnCountdownFinish;

        #endregion

        #region api

        public GameUiCategory Category => GameUiCategory.Countdown;
        public RectTransform Panel { get; private set; }

        public CountdownPanel(IGameDialogViewer _DialogViewer, UnityAction _OnCountdownFinish)
        {
            m_DialogViewer = _DialogViewer;
            m_OnCountdownFinish = _OnCountdownFinish;
        }

        public void Show()
        {
            Panel = Create();
            m_DialogViewer.Show(this);
        }

        public void OnEnable() { }

        #endregion
        
        #region private methods
        
        private RectTransform Create()
        {
            GameObject cp = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "game_menu", "countdown_panel");

            CountdownPanelView view = cp.GetCompItem<CountdownPanelView>("view");

            view.StartCountdown(() => m_OnCountdownFinish?.Invoke());
            return cp.RTransform();
        }
        
        #endregion
    }
}