using System;
using Constants;
using DialogViewers;
using Extensions;
using Helpers;
using TMPro;
using UI.Factories;
using UI.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.Panels
{
    [Flags]
    public enum PanelState
    {
        Showing = 1,
        NeedToClose = 2
    }
    
    public class GameMenuPanel : DI.DiObject, IGameDialogPanel
    {
        #region nonpublic members

        private readonly IGameDialogViewer m_DialogViewer;
        private readonly UnityAction m_Continue;
        private bool m_Initialized;

        #endregion
        
        #region api
        
        public static PanelState PanelState { get; set; }
        public GameUiCategory Category => GameUiCategory.Settings;
        public RectTransform Panel { get; private set; }

        public GameMenuPanel(IGameDialogViewer _DialogViewer, UnityAction _Continue)
        {
            m_DialogViewer = _DialogViewer;
            m_Continue = _Continue;
        }
        
        public void Show()
        {
            Panel = Create();
            bool hidePrev = m_DialogViewer.Last == null || m_DialogViewer.Last.Category != GameUiCategory.Countdown;
            m_DialogViewer.Show(this, hidePrev);
            PanelState = PanelState.Showing;
            m_Initialized = true;
        }
        
        #endregion
        
        #region nonpublic methods

        private RectTransform Create()
        {
            GameObject go = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "game_menu", "game_menu_panel");

            go.GetCompItem<Button>("exit_yes_button").SetOnClick(OnExitYesButtonClick);
            go.GetCompItem<Button>("exit_no_button").SetOnClick(OnExitNoButtonClick);
            return go.RTransform();
        }
        
        private void OnExitYesButtonClick()
        {
            SceneManager.LoadScene(SceneNames.Main);
        }

        private void OnExitNoButtonClick()
        {
            m_Continue?.Invoke();
            m_DialogViewer.Back();
            PanelState &= ~PanelState.Showing;
        }

        [DI.Update]
        private void OnUpdate()
        {
            if (!m_Initialized)
                return;
            if (PanelState.HasFlag(PanelState.NeedToClose))
            {
                OnExitNoButtonClick();
                PanelState &= ~PanelState.NeedToClose;
            }
            if (!PanelState.HasFlag(PanelState.Showing))
                Unregister();
        }
        
        #endregion
    }
}