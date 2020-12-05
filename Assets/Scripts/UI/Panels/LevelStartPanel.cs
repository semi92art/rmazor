using DialogViewers;
using Extensions;
using Helpers;
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
    public class LevelStartPanel : IGameDialogPanel
    {
        #region nonpublic members
        
        private readonly IGameDialogViewer m_DialogViewer;
        private readonly int m_Level;
        private readonly UnityAction<long> m_GetLifes;
        private readonly UnityAction m_StartLevel;
        private TextMeshProUGUI m_LifesAvailableText;
        private TextMeshProUGUI m_StartWithLifesText;
        private TextMeshProUGUI m_TakeOneMoreText;
        private Button m_TakeOneMoreButton;
        private long? m_AvailableLifes;
        private long m_StartLifes;

        #endregion
        
        #region api

        public GameUiCategory Category => GameUiCategory.LevelStart;
        public RectTransform Panel { get; private set; }

        public LevelStartPanel(
            IGameDialogViewer _DialogViewer,
            int _Level,
            UnityAction<long> _GetLifes, 
            UnityAction _StartLevel)
        {
            m_DialogViewer = _DialogViewer;
            m_Level = _Level;
            m_GetLifes = _GetLifes;
            m_StartLevel = _StartLevel;
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
            GameObject go = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "game_menu", "level_start_panel");

            TextMeshProUGUI levelText = go.GetCompItem<TextMeshProUGUI>("level_text");
            m_LifesAvailableText = go.GetCompItem<TextMeshProUGUI>("lifes_available_text");
            m_StartWithLifesText = go.GetCompItem<TextMeshProUGUI>("start_with_lifes_text");
            m_TakeOneMoreText = go.GetCompItem<TextMeshProUGUI>("take_one_more_text");

            m_TakeOneMoreButton = go.GetCompItem<Button>("take_one_more_button");
            Button startButton = go.GetCompItem<Button>("start_button");

            levelText.text = $"Level {m_Level}";
            SetStartLifes();
            m_TakeOneMoreButton.SetOnClick(OnTakeOneMoreLifeButtonClick);
            startButton.SetOnClick(OnStartButtonClick);
            return go.RTransform();
        }
        
        #endregion
        
        #region private methods

        private void SetStartLifes()
        {
            var bank = MoneyManager.Instance.GetBank();
            Coroutines.Run(Coroutines.WaitWhile(() =>
                {
                    if (!m_AvailableLifes.HasValue)
                        m_AvailableLifes = bank.Money[MoneyType.Lifes];
                    m_StartLifes = System.Math.Min(m_AvailableLifes.Value, 3);
                    m_AvailableLifes -= m_StartLifes;
                    CheckForAvailableLifesAndSetTexts();
                },
                () => !bank.Loaded));
        }

        private void OnTakeOneMoreLifeButtonClick()
        {
            bool shown = GoogleAdsManager.Instance.ShowRewardedAd(() =>
            {
                var bank = MoneyManager.Instance.GetBank();
                Coroutines.Run(Coroutines.WaitWhile(() =>
                    {
                        if (!m_AvailableLifes.HasValue)
                            m_AvailableLifes = bank.Money[MoneyType.Lifes];
                        if (m_AvailableLifes <= 0)
                            return;
                        m_StartLifes++;
                        m_AvailableLifes--;
                        CheckForAvailableLifesAndSetTexts();
                    },
                    () => !bank.Loaded));
            });
            if (!shown)
                Debug.LogError("Rewarded ad was not loaded!");
        }
        
        private void OnStartButtonClick()
        {
            IGameDialogPanel countdownPanel = new CountdownPanel(m_DialogViewer, () =>
            {
                m_DialogViewer.CloseAll();
                m_StartLevel?.Invoke();
            });
            countdownPanel.Show();
        }

        private void CheckForAvailableLifesAndSetTexts()
        {
            if (!m_AvailableLifes.HasValue)
                return;
            long avLifes = m_AvailableLifes.Value;
            SetLivesCountTexts(avLifes, m_StartLifes);
            if (avLifes == 0 || m_StartLifes == 5)
                CommonUtils.SetGoActive(false, (Component)m_TakeOneMoreButton, m_TakeOneMoreText);
            
            m_GetLifes?.Invoke(m_StartLifes);
        }
        
        private void SetLivesCountTexts(long _Available, long _OnStart)
        {
            m_LifesAvailableText.text = $"Lifes available: {_Available}";
            m_StartWithLifesText.text = $"Start with lifes: {_OnStart}";
        }

        #endregion
    }
}