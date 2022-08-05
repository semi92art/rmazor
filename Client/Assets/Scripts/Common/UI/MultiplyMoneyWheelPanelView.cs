using System.Collections.Generic;
using System.Linq;
using Common.Extensions;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Common.UI
{
    public class MultiplyMoneyWheelPanelView : SimpleUiItemBase, IFixedUpdateTick
    {
        #region constants

        private const float ArrowSpeed = 5f;

        #endregion
        
        #region serialized fields

        [SerializeField] private List<Image>   coeffBacks;
        [SerializeField] private Image         arrow;
        [SerializeField] private RectTransform min, max;

        #endregion
        
        #region nonpublic members

        private List<float> m_Thresholds;
        private List<Color> m_BackColors;
        private bool        m_DoMoveArrow;
        private bool        m_ArrowDirectionRight = true;
        private int         m_MultiplyCoefficient;
        
        #endregion
        
        #region api

        public event UnityAction<int> MultiplyCoefficientChanged;

        public override void Init(
            IUITicker            _UITicker,
            IColorProvider       _ColorProvider,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            IPrefabSetManager    _PrefabSetManager,
            bool                 _AutoFont = true)
        {
            InitCoefficientBackgroundsColors();
            base.Init(
                _UITicker,
                _ColorProvider,
                _AudioManager, 
                _LocalizationManager, 
                _PrefabSetManager, 
                _AutoFont);
        }


        public int GetMultiplyCoefficient()
        {
            return m_MultiplyCoefficient;
        }

        public void StartWheel()
        {
            UpdateThresholds();
            m_DoMoveArrow = true;
        }

        public void StopWheel()
        {
            m_DoMoveArrow = false;
        }

        public void SetArrowOnCurrentCoefficientPosition()
        {
            float minX = m_Thresholds.First();
            float maxX = m_Thresholds.Last();
            float centerX = (minX + maxX) * .5f;
            Vector2 GetArrowPos()          => arrow.rectTransform.localPosition;
            float   GetCoeffPosX(int _Idx) => coeffBacks[_Idx].rectTransform.localPosition.x;
            if (m_MultiplyCoefficient == 5)
            {
                arrow.rectTransform.localPosition = GetArrowPos().SetX(GetCoeffPosX(2));
                return;
            }
            if (GetArrowPos().x < centerX)
            {
                arrow.rectTransform.localPosition = m_MultiplyCoefficient switch
                {
                    2 => GetArrowPos().SetX(GetCoeffPosX(0)),
                    3 => GetArrowPos().SetX(GetCoeffPosX(1)),
                    _ => GetArrowPos()
                };
            }
            else
            {
                arrow.rectTransform.localPosition = m_MultiplyCoefficient switch
                {
                    2 => GetArrowPos().SetX(GetCoeffPosX(4)),
                    3 => GetArrowPos().SetX(GetCoeffPosX(3)),
                    _ => GetArrowPos()
                };
            }

        }

        public void FixedUpdateTick()
        {
            if (Ticker.Pause)
                return;
            if (!m_DoMoveArrow)
                return;
            MoveArrow();
        }
        
        #endregion
        
        #region nonpublic methods

        private void InitCoefficientBackgroundsColors()
        {
            m_BackColors = coeffBacks.Select(_Cb => _Cb.color).ToList();
        }
        
        private int GetMultiplyCoefficientCore()
        {
            if (!Initialized)
                return 2;
            float arrowXpos = arrow.rectTransform.anchoredPosition.x;
            float GetThresholdXPos(int _Index) => m_Thresholds[_Index];
            if (arrowXpos < GetThresholdXPos(1)) return 2;
            if (arrowXpos < GetThresholdXPos(2)) return 3;
            if (arrowXpos < GetThresholdXPos(3)) return 5;
            if (arrowXpos < GetThresholdXPos(4)) return 3;
            return 2;
        }

        private void UpdateThresholds()
        {
            const int count = 5;
            float step = (max.localPosition.x - min.localPosition.x) / count;
            m_Thresholds = Enumerable
                .Range(0, count + 1)
                .Select(_I => min.localPosition.x + step * _I)
                .ToList();
        }

        private void MoveArrow()
        {
            int multCoeff = GetMultiplyCoefficientCore();
            if (multCoeff != m_MultiplyCoefficient)
            {
                m_MultiplyCoefficient = multCoeff;
                MultiplyCoefficientChanged?.Invoke(multCoeff);
                HighlightCoefficientBackground();
            }
            float minX = m_Thresholds.First();
            float maxX = m_Thresholds.Last();
            float centerX = (minX + maxX) * .5f;
            Vector2 GetArrowPos () => arrow.rectTransform.localPosition;
            float coeff = ((maxX - minX) * .5f - Mathf.Abs(GetArrowPos().x - centerX)) / ((maxX - minX) * .5f);
            if (coeff < MathUtils.Epsilon) coeff = MathUtils.Epsilon;
            float speedAddict = coeff;
            float arrowSpeedCorrected = 100f + speedAddict * ArrowSpeed * 100f;
            var delta = Vector2.right * Ticker.FixedDeltaTime * arrowSpeedCorrected;
            arrow.rectTransform.localPosition = GetArrowPos() + (m_ArrowDirectionRight ? 1f : -1f) * delta;
            if (m_ArrowDirectionRight && GetArrowPos().x > maxX)
                m_ArrowDirectionRight = false;
            else if (!m_ArrowDirectionRight && GetArrowPos().x < minX)
                m_ArrowDirectionRight = true;
        }

        private void HighlightCoefficientBackground()
        {
            float arrowXpos = arrow.rectTransform.anchoredPosition.x;
            float GetThresholdXPos(int _Index) => m_Thresholds[_Index];
            int coeffPos;
            if (arrowXpos < GetThresholdXPos(1))      coeffPos = 0;
            else if (arrowXpos < GetThresholdXPos(2)) coeffPos = 1;
            else if (arrowXpos < GetThresholdXPos(3)) coeffPos = 2;
            else if (arrowXpos < GetThresholdXPos(4)) coeffPos = 3;
            else                                             coeffPos = 4;
            for (int i = 0; i < coeffBacks.Count; i++)
                coeffBacks[i].color = m_BackColors[i];
            Color.RGBToHSV(m_BackColors[coeffPos], out float h, out float s, out float v);
            coeffBacks[coeffPos].color = Color.HSVToRGB(h , s + 0.3f, v + 0.2f);
        }

        #endregion
    }
}