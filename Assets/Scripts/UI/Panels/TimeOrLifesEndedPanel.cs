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
    public class TimeOrLifesEndedPanel : IGameDialogPanel
    {
        #region nonpublic members

        public static int TimesPanelCalled;
        public const int TimesCanTimeOrLifeBeAdded = 2;
        private readonly IGameDialogViewer m_DialogViewer;
        private readonly bool m_IsSecs;
        private readonly UnityAction m_Continue;
        private readonly UnityAction<float> m_SetAdditionalTime;
        private readonly UnityAction<long> m_SetAdditionalLife;
        private readonly float[] m_SecsArr = {10f, 7f};
        private TextMeshProUGUI m_AddSecsOrLifeMoreText;
        private TextMeshProUGUI m_TimeOrLifesEndedText;
        private Button m_FinishButton;
        
        
        #endregion
        
        #region api
        
        public GameUiCategory Category => GameUiCategory.TimeEnded;
        public RectTransform Panel { get; private set; }

        public TimeOrLifesEndedPanel(
            IGameDialogViewer _DialogViewer,
            bool _IsSecs,
            UnityAction _Continue = null,
            UnityAction<float> _SetAdditionalTime = null,
            UnityAction<long> _SetAdditionalLife = null)
        {
            m_DialogViewer = _DialogViewer;
            m_IsSecs = _IsSecs;
            m_Continue = _Continue;
            m_SetAdditionalTime = _SetAdditionalTime;
            m_SetAdditionalLife = _SetAdditionalLife;
        }
        
        public void Show()
        {
            TimesPanelCalled++;
            Panel = Create();
            m_DialogViewer.Show(this);
        }
        #endregion
        
        #region private methods

        private RectTransform Create()
        {
            GameObject go = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "game_menu", "time_or_lifes_ended_panel");

            m_TimeOrLifesEndedText = go.GetCompItem<TextMeshProUGUI>("time_or_lifes_ended_text");
            m_AddSecsOrLifeMoreText = go.GetCompItem<TextMeshProUGUI>("add_secs_or_life_more_text");

            Button addSecsOrLifeMoreButton = go.GetCompItem<Button>("add_secs_or_life_more_button");
            m_FinishButton = go.GetCompItem<Button>("finish_button");

            addSecsOrLifeMoreButton.SetOnClick(OnAddSecsOrLifeMoreButtonClick);
            m_FinishButton.SetOnClick(OnFinishButtonClick);

            SetTexts();
            return go.RTransform();
        }

        private void SetTexts()
        {
            m_TimeOrLifesEndedText.text = m_IsSecs ? "Time ended" : "Lifes ended";
            m_AddSecsOrLifeMoreText.text = m_IsSecs ? 
                $"Get {m_SecsArr[TimesPanelCalled - 1]} seconds more for watching ad:" :
                "Get 1 life more for watching ad:";
        }

        private void Continue()
        {
            IGameDialogPanel countdownPanel = new CountdownPanel(m_DialogViewer, () =>
            {
                m_DialogViewer.CloseAll();
                m_Continue?.Invoke();
            });
            countdownPanel.Show();
        }

        private void OnFinishButtonClick()
        {
            SceneManager.LoadScene(SceneNames.Main);
        }

        private void OnAddSecsOrLifeMoreButtonClick()
        {
            if (m_IsSecs)
                AddSecs();
            else
                AddLife();
            Continue();
        }

        private void AddSecs()
        {
            m_SetAdditionalTime?.Invoke(m_SecsArr[TimesPanelCalled - 1]);
        }

        private void AddLife()
        {
            m_SetAdditionalLife?.Invoke(1);
        }

        #endregion
        
    }
}