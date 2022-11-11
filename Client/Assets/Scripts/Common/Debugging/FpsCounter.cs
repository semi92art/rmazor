using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Common.Entities;
using Common.Extensions;
using Common.Helpers;
using Common.Ticker;
using Common.Utils;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Common.Debugging
{
    public readonly struct FpsCounterRecording
    {
        public FpsCounterRecording(
            float _Fps,
            float _FpsMin, 
            float _FpsMax, List<float> _FpsValues)
        {
            Fps        = _Fps;
            FpsMin     = _FpsMin;
            FpsMax     = _FpsMax;
            FpsValues  = _FpsValues;
        }

        public float       Fps       { get; }
        public float       FpsMin    { get; }
        public float       FpsMax    { get; }
        public List<float> FpsValues { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"Average Fps: {Fps}, min FPS: {FpsMin}, max FPS: {FpsMax}");
            sb.AppendLine(", values:");
            foreach (float fpsValue in FpsValues)
                sb.AppendLine(fpsValue.ToString(CultureInfo.InvariantCulture));
            return sb.ToString();
        }
    }

    public interface IFpsCounter : IInit
    {
        Entity<bool>        IsLowPerformance { get; }
        FpsCounterRecording GetRecording();
        void                Record(float _Duration);
    }

    public class FpsCounterFake : InitBase, IFpsCounter
    {
        public Entity<bool>        IsLowPerformance        => new Entity<bool>
        {
            Value = false,
            Result = EEntityResult.Success
        };
        public FpsCounterRecording GetRecording()          => default;
        public void                Record(float _Duration) { }
    }
    
    public class FpsCounter : InitBase, IFpsCounter, IUpdateTick
    {
        #region constants

        private const float UpdateRate = 4f;

        #endregion
        
        #region nonpublic members

        private TextMeshPro m_FpsText;
        
        private float m_FpsCurrent;
        private int   m_FramesCount;
        private float m_Dt;
        private bool  m_DoRecord;
        
        private readonly List<float> m_FpsValues = new List<float>();

        #endregion

        #region inject

        private IRemotePropertiesCommon RemotePropertiesCommon { get; }
        private ICommonTicker           CommonTicker           { get; }

        private FpsCounter(
            IRemotePropertiesCommon _RemotePropertiesCommon,
            ICommonTicker           _CommonTicker)
        {
            RemotePropertiesCommon = _RemotePropertiesCommon;
            CommonTicker           = _CommonTicker;
        }
        
        #endregion

        #region api

        public override void Init()
        {
            if (RemotePropertiesCommon.DebugEnabled)
                InitFpsText();
            MeasurePerformanceOnInit();
            CommonTicker.Register(this);
            base.Init();
        }

        public Entity<bool> IsLowPerformance { get; } = new Entity<bool>
        {
            Result = EEntityResult.Pending
        };

        public FpsCounterRecording GetRecording()
        {
            float fpsAverage = m_FpsValues.Count > 0f ? m_FpsValues.Average() : 0f;
            float fpsMin = m_FpsValues.Count > 0f ? m_FpsValues.Min() : 0f;
            float fpsMax = m_FpsValues.Count > 0f ? m_FpsValues.Max() : 0f;
            return new FpsCounterRecording(fpsAverage, fpsMin, fpsMax, m_FpsValues);
        }

        public void Record(float _Duration)
        {
            Clear();
            m_DoRecord = true;
            m_FpsValues.Clear();
            Cor.Run(Cor.Delay(
                _Duration,
                CommonTicker, 
                _OnStart: () => m_DoRecord = true,
                _OnDelay: () => m_DoRecord = false));
        }

        public void UpdateTick()
        {
            if (!Initialized)
                return;
            m_FramesCount++;
            m_Dt += CommonTicker.DeltaTime;
            if (!(m_Dt > 1f / UpdateRate)) 
                return;
            m_FpsCurrent = m_FramesCount / m_Dt ;
            m_FramesCount = 0;
            m_Dt -= 1f/UpdateRate;
            if (RemotePropertiesCommon.DebugEnabled)
                m_FpsText.text = "FPS: " + Convert.ToInt32(m_FpsCurrent);
            if (m_DoRecord)
                m_FpsValues.Add(m_FpsCurrent);
        }

        #endregion

        #region nonpublic methods

        private void MeasurePerformanceOnInit()
        {
            Record(3f);
            Cor.Run(Cor.WaitWhile(() => m_DoRecord,
                () =>
                {
                    float fpsAverage = m_FpsValues.Count > 0f ? m_FpsValues.Average() : 0f;
                    IsLowPerformance.Value = fpsAverage < CommonUtils.FpsThresholdLowPerformance;
                    IsLowPerformance.Result = EEntityResult.Success;
                    if (IsLowPerformance.Value)
                        SaveUtils.PutValue(SaveKeysCommon.LowPerformanceDevice, true);
                }));
        }
        
        private void InitFpsText()
        {
            var textGo = new GameObject("Fps Counter Text");
            Object.DontDestroyOnLoad(textGo);
            var screendBounds = GraphicUtils.GetVisibleBounds();
            textGo.transform.SetPosXY(screendBounds.max.x - 1, screendBounds.max.y - 1);
            m_FpsText = textGo.AddComponent<TextMeshPro>();
            m_FpsText.rectTransform.pivot = Vector2.one;
            m_FpsText.alignment = TextAlignmentOptions.TopRight;
            m_FpsText.fontSize = 15f;
            m_FpsText.color = Color.yellow;
            m_FpsText.outlineColor = Color.black;
        }
        
        private void Clear()
        {
            m_FramesCount = 0;
            m_FpsCurrent  = 0f;
            m_Dt          = 0f;
        }

        #endregion
    }
}