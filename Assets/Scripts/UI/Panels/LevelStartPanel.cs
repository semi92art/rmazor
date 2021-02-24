using Constants;
using DialogViewers;
using Entities;
using Extensions;
using GameHelpers;
using Managers;
using TMPro;
using UI.Factories;
using UI.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;

namespace UI.Panels
{
    public class LevelStartPanel : DialogPanelBase, IGameUiCategory
    {
        #region nonpublic members
        
        private readonly IGameDialogViewer m_DialogViewer;
        private readonly int m_Level;
        private readonly UnityAction m_StartLevel;

        private Button m_TakeOneMoreButton;

        #endregion
        
        #region api

        public GameUiCategory Category => GameUiCategory.LevelStart;

        public LevelStartPanel(
            IGameDialogViewer _DialogViewer,
            int _Level,
            UnityAction _StartLevel)
        {
            m_DialogViewer = _DialogViewer;
            m_Level = _Level;
            m_StartLevel = _StartLevel;
        }
        
        public override void Init()
        {
            GameObject go = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.Container,
                    RtrLites.FullFill),
                "game_menu", "level_start_panel");

            TextMeshProUGUI levelText = go.GetCompItem<TextMeshProUGUI>("level_text");

            m_TakeOneMoreButton = go.GetCompItem<Button>("take_one_more_button");
            Button startButton = go.GetCompItem<Button>("start_button");

            levelText.text = $"Level {m_Level}";
            startButton.SetOnClick(OnStartButtonClick);
            Panel = go.RTransform();
        }

        #endregion
        
        #region nonpublic methods
        
        
        private void OnStartButtonClick()
        {
            var countdownPanel = new CountdownPanel(
                m_DialogViewer.Container, () =>
            {
                m_DialogViewer.CloseAll();
                m_StartLevel?.Invoke();
            });
            countdownPanel.Init();
            m_DialogViewer.Show(countdownPanel);
        }
        
        
        #endregion
    }
}