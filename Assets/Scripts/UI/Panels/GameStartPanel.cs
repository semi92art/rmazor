using Constants;
using DialogViewers;
using Extensions;
using Helpers;
using UI.Factories;
using UI.Managers;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Panels
{
    public class GameStartPanel : IGameDialogPanel
    {
        #region nonpublic members
        
        private readonly IGameDialogViewer m_DialogViewer;
        private readonly UnityAction m_StartGame;

        #endregion

        #region api

        public GameUiCategory Category => GameUiCategory.GameStart;
        public RectTransform Panel { get; private set; }

        public GameStartPanel(IGameDialogViewer _DialogViewer, UnityAction _StartGame)
        {
            m_DialogViewer = _DialogViewer;
            m_StartGame = _StartGame;
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
            GameObject gsp = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "game_menu", "start_game_panel");

            GameStartPanelView view = gsp.GetCompItem<GameStartPanelView>("view");
            view.StartCountdown(m_StartGame);
            return gsp.RTransform();
        }
        
        #endregion
    }
}