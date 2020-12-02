using System;
using System.Collections.Generic;
using System.Linq;
using DialogViewers;
using Extensions;
using Helpers;
using Managers;
using TMPro;
using UI.Entities;
using UI.Factories;
using UI.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;

namespace UI.Panels
{
    public class LevelFinishPanel : IGameDialogPanel
    {
        #region nonpublic members

        private readonly IGameDialogViewer m_DialogViewer;
        private readonly int m_Level;
        private readonly Dictionary<MoneyType, long> m_Revenue;
        private readonly UnityAction<Dictionary<MoneyType, long>> m_SetNewRevenue;
        private readonly UnityAction m_FinishLevel;
        private readonly bool m_IsPersonalBest;

        private float m_MultiplyCoefficient = 2f;
        private TextMeshProUGUI m_GoldCountRevenueText;
        private TextMeshProUGUI m_DiamondsCountRevenueText;
        private TextMeshProUGUI m_LifesCountRevenueText;
        private Button m_X2Button;

        #endregion
        
        #region api
        
        public GameUiCategory Category => GameUiCategory.LevelFinish;
        public RectTransform Panel { get; private set; }

        public LevelFinishPanel(
            IGameDialogViewer _DialogViewer,
            int _Level,
            Dictionary<MoneyType, long> _Revenue,
            UnityAction<Dictionary<MoneyType, long>> _SetNewRevenue,
            UnityAction _FinishLevel,
            bool _IsPersonalBest)
        {
            m_DialogViewer = _DialogViewer;
            m_Level = _Level;
            m_Revenue = _Revenue;
            m_SetNewRevenue = _SetNewRevenue;
            m_FinishLevel = _FinishLevel;
            m_IsPersonalBest = _IsPersonalBest;
        }
        
        public void Show()
        {
            Panel = Create();
            m_DialogViewer.Show(this);
        }
        
        #endregion

        #region nonpublic methods

        private RectTransform Create()
        {
            GameObject go = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "game_menu", "level_finish_panel");

            var levelFinishedText = go.GetCompItem<TextMeshProUGUI>("level_finished_text");
            var personalBestText = go.GetCompItem<TextMeshProUGUI>("personal_best_text");
            var revenueText = go.GetCompItem<TextMeshProUGUI>("revenue_text");
            m_GoldCountRevenueText = go.GetCompItem<TextMeshProUGUI>("gold_count_revenue_text"); 
            m_DiamondsCountRevenueText = go.GetCompItem<TextMeshProUGUI>("diamonds_count_revenue_text"); 
            m_LifesCountRevenueText = go.GetCompItem<TextMeshProUGUI>("lifes_count_revenue_text");
            m_X2Button = go.GetCompItem<Button>("x2_button");
            Button continueButton = go.GetCompItem<Button>("continue_button");
            levelFinishedText.text = $"Level {m_Level} finished!";
            revenueText.text = "Revenue:";
            SetRevenueCountsText();

            if (m_Revenue == null 
                || !m_Revenue.Any() 
                || m_Revenue.All(_Revenue => _Revenue.Value == 0))
                m_X2Button.gameObject.SetActive(false);
            
            personalBestText.enabled = m_IsPersonalBest;
            m_X2Button.SetOnClick(OnX2ButtonClick);
            continueButton.SetOnClick(OnContinueButtonClick);
            
            return go.RTransform();
        }

        private void OnX2ButtonClick()
        {
            foreach (var kvp in m_Revenue.ToArray())
                m_Revenue[kvp.Key] = (long)(m_Revenue[kvp.Key] * m_MultiplyCoefficient);
            m_SetNewRevenue?.Invoke(m_Revenue);
            SetRevenueCountsText();
            m_X2Button.gameObject.SetActive(false);
        }

        private void OnContinueButtonClick()
        {
            m_FinishLevel?.Invoke();
        }

        private void SetRevenueCountsText()
        {
            m_GoldCountRevenueText.text = "-";
            if (m_Revenue.ContainsKey(MoneyType.Gold) && m_Revenue[MoneyType.Gold] > 0)
                m_GoldCountRevenueText.text = m_Revenue[MoneyType.Gold].ToNumeric();
            m_DiamondsCountRevenueText.text = "-";
            if (m_Revenue.ContainsKey(MoneyType.Diamonds) && m_Revenue[MoneyType.Diamonds] > 0)
                m_DiamondsCountRevenueText.text = m_Revenue[MoneyType.Diamonds].ToNumeric();
            m_LifesCountRevenueText.text = "-";
            if (m_Revenue.ContainsKey(MoneyType.Lifes) && m_Revenue[MoneyType.Lifes] > 0)
                m_LifesCountRevenueText.text = "-";
        }
        
        #endregion
        
    }
}