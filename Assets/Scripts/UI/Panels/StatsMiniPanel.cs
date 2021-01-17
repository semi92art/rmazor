using Constants;
using Extensions;
using GameHelpers;
using TMPro;
using UI.Entities;
using UI.Factories;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Panels
{
    public interface IStatsMiniPanel : IMiniPanel
    {
        void SetLifes(long _Count, bool _AnimateIfDecrease = true);
        void SetTime(float _Time);
        void SetNecessaryScore(int? _Score = null);
        void SetScore(int _Score);
    }

    public enum StatsPanelPosition
    {
        Top,
        TopLeft,
        TopRight
    }

    public class StatsMiniPanel : IStatsMiniPanel
    {
        #region nonpublic members

        private const int MaxLifesCount = 5;
        private readonly RectTransform m_Panel;
        private readonly Image[] m_LifeIcons = new Image[MaxLifesCount];
        private readonly Animator[] m_LifeBrokenAnimators = new Animator[MaxLifesCount];
        private readonly Sprite m_LifeEnabled;
        private readonly Sprite m_LifeDisabled;
        private readonly TextMeshProUGUI m_CountdownText;
        private readonly TextMeshProUGUI m_ScoreText;
        private int m_Score;
        private int? m_NecessaryScore;
        private long m_LifesCount;
        private bool m_AnimateIfDecrease;
        private static int AkBrokenLife => AnimKeys.Anim;
        private long LifesCount
        {
            get => m_LifesCount;
            set
            {
                bool isLess = value - m_LifesCount < 0; 
                m_LifesCount = value;
                SetIcons();
                if (isLess && m_AnimateIfDecrease)
                    AnimateBrokenLife();
            }
        }
        
        #endregion

        #region constructor

        private StatsMiniPanel(RectTransform _Parent, StatsPanelPosition _Position)
        {
            string prefabName = "stats_panel_top";
            if (_Position == StatsPanelPosition.TopLeft)
                prefabName = "stats_panel_top_left";
            else if (_Position == StatsPanelPosition.TopRight)
                prefabName = "stats_panel_top_right";
            
            var go = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    _Parent,
                    UiAnchor.Create(0, 1, 1, 1),
                    Vector2.zero, 
                    Vector2.one,
                    new Vector2(0f, 59.2f)),
                "game_menu", prefabName);
            m_Panel = go.RTransform();
            for (int i = 0; i < MaxLifesCount; i++)
            {
                m_LifeIcons[i] = go.GetCompItem<Image>($"life_icon_{i + 1}");
                m_LifeBrokenAnimators[i] = go.GetCompItem<Animator>($"life_broken_animator_{i + 1}");    
            }
            m_LifeEnabled = PrefabInitializer.GetObject<Sprite>("icons", "icon_life_enabled");
            m_LifeDisabled = PrefabInitializer.GetObject<Sprite>("icons", "icon_life_disabled");
            m_CountdownText = go.GetCompItem<TextMeshProUGUI>("countdown_text");
            m_ScoreText = go.GetCompItem<TextMeshProUGUI>("score_text");
        }
        
        #endregion

        #region api

        public static IStatsMiniPanel Create(RectTransform _Parent, StatsPanelPosition _Position)
        {
            return new StatsMiniPanel(_Parent, _Position);
        }
        
        public void Show()
        {
            m_Panel.SetGoActive(true);
        }

        public void Hide()
        {
            m_Panel.SetGoActive(false);
        }

        public void Init()
        {
            
        }

        public void SetLifes(long _Count, bool _AnimateIfDecrease = true)
        {
            m_AnimateIfDecrease = _AnimateIfDecrease;
            LifesCount = _Count;
        }
        
        public void SetTime(float _Time)
        {
            m_CountdownText.text = _Time.ToTimer();
        }
        
        public void SetNecessaryScore(int? _Score = null)
        {
            m_NecessaryScore = _Score;
            SetScoreText();
        }

        public void SetScore(int _Score)
        {
            m_Score = _Score;
            SetScoreText();
        }

        private void SetScoreText()
        {
            m_ScoreText.text = $"{m_Score}" + (m_NecessaryScore.HasValue ? 
                                   $"/{m_NecessaryScore.Value}" : string.Empty);
        }
        
        #endregion

        #region nonpublic methods
        
        private void SetIcons()
        {
            for (int i = 0; i < MaxLifesCount; i++)
                m_LifeIcons[i].sprite = i < LifesCount ? m_LifeEnabled : m_LifeDisabled;
        }

        private void AnimateBrokenLife()
        {
            m_LifeBrokenAnimators[LifesCount].SetTrigger(AkBrokenLife);
        }

        #endregion
    }
}