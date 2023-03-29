using System;
using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.Helpers
{
    public interface ISpecialOfferTimerController : IInit
    {
        event UnityAction<TimeSpan> TimerValueChanged;
        event UnityAction           TimeIsGone;
        bool                        ShownThisSession { get;  set; }
        bool                        IsTimeGone       { get; }
    }
    
    public class SpecialOfferTimerController : InitBase, ISpecialOfferTimerController, IUpdateTick
    {
        #region nonpublic members

        private float m_SecondsThisSession;
        private int   m_TimeInSecondsThisSessionRounded;
        private float m_SpecialOfferTimeInMinutesThisSession;
        private float m_TimeAtStartInMinutes;
        private bool  m_DoProceedTimer;
        private bool  m_SpecialOfferShownThisSession;
        
        #endregion
        
        #region inject

        private ViewSettings  ViewSettings { get; }
        private ICommonTicker CommonTicker { get; }

        public SpecialOfferTimerController(
            ViewSettings _ViewSettings, 
            ICommonTicker _CommonTicker)
        {
            ViewSettings = _ViewSettings;
            CommonTicker = _CommonTicker;
        }

        #endregion
        
        public event UnityAction<TimeSpan> TimerValueChanged;
        public event UnityAction           TimeIsGone;

        public bool ShownThisSession
        {
            get => m_SpecialOfferShownThisSession;
            set
            {
                if (!m_SpecialOfferShownThisSession)
                    m_DoProceedTimer = true;
                m_SpecialOfferShownThisSession = value;
            }
        }
        
        public override void Init()
        {
            m_TimeAtStartInMinutes = (float)SaveUtils.GetValue(
                SaveKeysRmazor.SpecialOfferTimePassedInMinutes).TotalMinutes; 
            CommonTicker.Register(this);
            base.Init();
        }


        public bool IsTimeGone => m_TimeAtStartInMinutes * 60f + m_SecondsThisSession >
                                  ViewSettings.specialOfferDurationInMinutes * 60f;

        public void UpdateTick()
        {
            if (!m_DoProceedTimer)
                return;
            m_SecondsThisSession += CommonTicker.DeltaTime;
            InvokeActionIfTimerValueChanged();
            SaveNewTotalMinutesIfChanged();
            if (!IsTimeGone)
                return;
            m_DoProceedTimer = false;
            TimeIsGone?.Invoke();
        }

        private void InvokeActionIfTimerValueChanged()
        {
            int newSecondsThisSessionRounded = Mathf.RoundToInt(m_SecondsThisSession);
            if (m_TimeInSecondsThisSessionRounded == newSecondsThisSessionRounded)
                return;
            var span = TimeSpan.FromSeconds(ViewSettings.specialOfferDurationInMinutes * 60f -
                                            m_TimeAtStartInMinutes * 60f - newSecondsThisSessionRounded);
            m_TimeInSecondsThisSessionRounded = newSecondsThisSessionRounded;
            TimerValueChanged?.Invoke(span);
        }

        private void SaveNewTotalMinutesIfChanged()
        {
            float newMinutesThisSession = Mathf.FloorToInt(m_SecondsThisSession / 60f);
            if (MathUtils.Equals(m_SpecialOfferTimeInMinutesThisSession, newMinutesThisSession))
                return;
            var span = TimeSpan.FromMinutes(m_TimeAtStartInMinutes + newMinutesThisSession);
            SaveUtils.PutValue(SaveKeysRmazor.SpecialOfferTimePassedInMinutes, span);
            m_SpecialOfferTimeInMinutesThisSession = newMinutesThisSession;
        }
    }
}