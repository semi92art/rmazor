using System;
using System.Collections.Generic;
using System.Linq;
using mazing.common.Runtime;
using mazing.common.Runtime.Debugging;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

    namespace RMAZOR
    {
        public class FpsCounterRmazor : InitBase, IFpsCounter, IUpdateTick
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

            private FpsCounterRmazor(
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
                if (RemotePropertiesCommon.DebugEnabled || Application.isEditor)
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

            public void OnActiveCameraChanged(Camera _Camera)
            {
                Cor.Run(Cor.WaitWhile(
                    () => m_FpsText.IsNull(), 
                    () =>
                    {
                        var bounds = GraphicUtils.GetVisibleBounds(_Camera);
                        m_FpsText.gameObject.SetParent(_Camera.transform);
                        m_FpsText.transform.SetLocalPosXY(bounds.min.x + 1, bounds.max.y - 1);
                    }));
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
                if (RemotePropertiesCommon.DebugEnabled || Application.isEditor)
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
                m_FpsText = textGo.AddComponent<TextMeshPro>();
                m_FpsText.rectTransform.pivot = Vector2.up;
                m_FpsText.alignment = TextAlignmentOptions.TopLeft;
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