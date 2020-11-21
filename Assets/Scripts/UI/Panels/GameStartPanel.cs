using DialogViewers;
using Extensions;
using Helpers;
using UI.Factories;
using UI.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Panels
{
    public class GameStartPanel : IGameDialogPanel
    {
        #region nonpublic members
        
        private readonly IGameDialogViewer m_GameDialogViewer;
        private readonly UnityAction m_StartGame;
        
        #endregion

        #region api

        public GameUiCategory Category => GameUiCategory.GameStart;
        public RectTransform Panel { get; private set; }

        public GameStartPanel(IGameDialogViewer _GameDialogViewer, UnityAction _StartGame)
        {
            m_GameDialogViewer = _GameDialogViewer;
            m_StartGame = _StartGame;
        }

        public void Show()
        {
            Panel = Create();
        }

        #endregion
        
        #region private methods
        
        private RectTransform Create()
        {
            GameObject gsp = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_GameDialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "game_menu", "start_game_panel");

            Button startGameButton = gsp.GetCompItem<Button>("start_game_button");
            startGameButton.SetOnClick(m_StartGame);

            return gsp.RTransform();
        }
        
        #endregion
    }
}