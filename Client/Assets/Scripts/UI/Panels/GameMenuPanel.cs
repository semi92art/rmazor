using System;
using Constants;
using DI.Extensions;
using DialogViewers;
using Entities;
using GameHelpers;
using Ticker;
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
    
    public class GameMenuPanel : DialogPanelBase, IGameUiCategory, IUpdateTick
    {
        #region nonpublic members

        private readonly IGameDialogViewer m_DialogViewer;
        private readonly UnityAction m_Continue;
        private bool m_Initialized;

        #endregion
        
        #region api
        
        public static PanelState PanelState { get; set; }
        public GameUiCategory Category => GameUiCategory.Settings;

        public GameMenuPanel(
            IGameDialogViewer _DialogViewer, 
            UnityAction _Continue,
            IManagersGetter _Managers,
            IUITicker _UITicker) 
            : base(_Managers, _UITicker)
        {
            m_DialogViewer = _DialogViewer;
            m_Continue = _Continue;
        }
        
        public override void OnDialogShow()
        {
            var lastCategory = (m_DialogViewer.Last as IGameUiCategory)?.Category;
            bool hidePrev = m_DialogViewer.Last == null || lastCategory != GameUiCategory.Countdown;
            m_DialogViewer.Show(this, hidePrev);
            PanelState = PanelState.Showing;
            m_Initialized = true;
        }

        public override void Init()
        {
            base.Init();
            GameObject go = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.Container,
                    RtrLites.FullFill),
                "game_menu", "game_menu_panel");

            go.GetCompItem<Button>("exit_yes_button").SetOnClick(OnExitYesButtonClick);
            go.GetCompItem<Button>("exit_no_button").SetOnClick(OnExitNoButtonClick);
            Panel = go.RTransform();
        }


        #endregion
        
        #region nonpublic methods

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
        
        public void UpdateTick()
        {
            if (!m_Initialized)
                return;
            if (PanelState.HasFlag(PanelState.NeedToClose))
            {
                OnExitNoButtonClick();
                PanelState &= ~PanelState.NeedToClose;
            }
            if (!PanelState.HasFlag(PanelState.Showing))
                Ticker.Unregister(this);
        }
        
        #endregion
    }
}