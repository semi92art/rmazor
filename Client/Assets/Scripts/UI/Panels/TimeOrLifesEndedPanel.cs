using Constants;
using DI.Extensions;
using DialogViewers;
using GameHelpers;
using Ticker;
using TMPro;
using UI.Factories;
using UI.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.Panels
{
    public class TimeOrLifesEndedPanel : DialogPanelBase, IGameUiCategory
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

        public TimeOrLifesEndedPanel(
            IGameDialogViewer _DialogViewer,
            IUITicker _UITicker,
            bool _IsSecs,
            UnityAction _Continue = null,
            UnityAction<float> _SetAdditionalTime = null,
            UnityAction<long> _SetAdditionalLife = null) : base(_UITicker)
        {
            m_DialogViewer = _DialogViewer;
            m_IsSecs = _IsSecs;
            m_Continue = _Continue;
            m_SetAdditionalTime = _SetAdditionalTime;
            m_SetAdditionalLife = _SetAdditionalLife;
        }
        
        public override void Init()
        {
            GameObject go = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.Container,
                    RtrLites.FullFill),
                "game_menu", "time_or_lifes_ended_panel");

            m_TimeOrLifesEndedText = go.GetCompItem<TextMeshProUGUI>("time_or_lifes_ended_text");
            m_AddSecsOrLifeMoreText = go.GetCompItem<TextMeshProUGUI>("add_secs_or_life_more_text");

            Button addSecsOrLifeMoreButton = go.GetCompItem<Button>("add_secs_or_life_more_button");
            m_FinishButton = go.GetCompItem<Button>("finish_button");

            addSecsOrLifeMoreButton.SetOnClick(OnAddSecsOrLifeMoreButtonClick);
            m_FinishButton.SetOnClick(OnFinishButtonClick);

            SetTexts();
            Panel = go.RTransform();
        }
        
        public override void OnDialogShow()
        {
            TimesPanelCalled++;
        }

        #endregion
        
        #region nonpublic methods

        private void SetTexts()
        {
            m_TimeOrLifesEndedText.text = m_IsSecs ? "Time ended" : "Lifes ended";
            m_AddSecsOrLifeMoreText.text = m_IsSecs ? 
                $"Get {m_SecsArr[TimesPanelCalled - 1]} seconds more for watching ad:" :
                "Get 1 life more for watching ad:";
        }

        private void Continue()
        {
            var countdownPanel = new CountdownPanel(
                m_DialogViewer.Container, () =>
            {
                m_DialogViewer.CloseAll();
                m_Continue?.Invoke();
            }, UITicker);
            countdownPanel.Init();
            m_DialogViewer.Show(countdownPanel);
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