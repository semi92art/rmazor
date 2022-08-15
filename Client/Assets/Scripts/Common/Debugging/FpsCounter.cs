using System.Collections.Generic;
using Common.Helpers;
using Common.Ticker;
using Common.Utils;
using UnityEngine;

namespace Common.Debugging
{
    public readonly struct FpsCounterRecording
    {
        public FpsCounterRecording(
            float _FpsMin, 
            float _FpsMax, List<float> _FpsValues)
        {
            FpsMin     = _FpsMin;
            FpsMax     = _FpsMax;
            FpsValues  = _FpsValues;
        }

        public float       FpsMin     { get; }
        public float       FpsMax     { get; }
        public List<float> FpsValues  { get; }
    }

    public interface IFpsCounter : IInit
    {
        FpsCounterRecording GetRecording();
        void                SetUpdateRate(float _Rate);
        void                Record(float _Duration);
    }
    
    public class FpsCounter : InitBase, IFpsCounter, IUpdateTick
    {
        #region nonpublic members

        private float m_FpsCurrent;
        private float m_FpsMin;
        private float m_FpsMax;
        private int   m_FramesCount;
        private float m_Dt;
        private float m_UpdateRate = 4f;

        private          bool        m_DoRecord;
        private readonly List<float> m_FpsValues = new List<float>();

        #endregion

        #region inject
        
        private ICommonTicker CommonTicker { get; }

        public FpsCounter(ICommonTicker _CommonTicker)
        {
            CommonTicker = _CommonTicker;
        }
        
        #endregion

        #region api
        
        public FpsCounterRecording GetRecording()
        {
            return new FpsCounterRecording(m_FpsMin, m_FpsMax, m_FpsValues);
        }

        public void SetUpdateRate(float _Rate)
        {
            m_UpdateRate = _Rate;
        }
        
        public void Record(float _Duration)
        {
            Clear();
            m_DoRecord = true;
            Cor.Run(Cor.Delay(
                _Duration,
                CommonTicker, 
                () => m_DoRecord = false));
        }

        public void UpdateTick()
        {
            if (!m_DoRecord)
                return;
            m_FramesCount++;
            m_Dt += CommonTicker.DeltaTime;
            if (!(m_Dt > 1f / m_UpdateRate)) 
                return;
            m_FpsCurrent = m_FramesCount / m_Dt ;
            m_FpsMin = Mathf.Min(m_FpsCurrent, m_FpsMin);
            m_FpsMax = Mathf.Max(m_FpsCurrent, m_FpsMax);
            m_FramesCount = 0;
            m_Dt -= 1f/m_UpdateRate;
        }

        #endregion

        #region nonpublic methods
        
        private void Clear()
        {
            m_FpsMin = float.PositiveInfinity;
            m_FpsMax = float.NegativeInfinity;
            m_FpsCurrent = 0f;
            m_FramesCount = 0;
            m_Dt = 0f;
        }

        #endregion
    }
}