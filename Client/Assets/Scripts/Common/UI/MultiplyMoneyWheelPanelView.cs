using System.Collections.Generic;
using System.Linq;
using Common.Constants;
using Common.Extensions;
using Common.Managers;
using Common.Ticker;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Common.UI
{
    public class MultiplyMoneyWheelPanelView : SimpleUiItemBase, IFixedUpdateTick
    {
        #region serialized fields

        [SerializeField] private List<Image>   coeffBacks;
        [SerializeField] private Image         arrow;
        [SerializeField] private RectTransform min, max;
        [SerializeField] private Animator      animator;

        #endregion
        
        #region nonpublic members

        private List<float> m_Thresholds;
        private List<Color> m_BackColors;

        private bool m_DoMoveArrow;
        private int  m_MultiplyCoefficient;
        
        #endregion
        
        #region api

        public event UnityAction<int> MultiplyCoefficientChanged;

        public override void Init(
            IUITicker            _UITicker,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager)
        {
            InitCoefficientBackgroundsColors();
            base.Init(
                _UITicker,
                _AudioManager, 
                _LocalizationManager);
        }


        public int GetMultiplyCoefficient()
        {
            return m_MultiplyCoefficient;
        }

        public void StartWheel()
        {
            animator.enabled = true;
            UpdateThresholds();
            animator.SetTrigger(AnimKeys.Anim);
            m_DoMoveArrow = true;
        }

        public void ResetWheel()
        {
            animator.enabled = true;
            animator.SetTrigger(AnimKeys.Stop);
        }

        public void StopWheel()
        {
            animator.enabled = false;
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
        
        public void HighlightCurrentCoefficient()
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
            coeffBacks[coeffPos].color = Color.HSVToRGB(h , s + 0.3f, v + 0.2f).SetA(1f);
        }

        public void FixedUpdateTick()
        {
            if (Ticker.Pause)
                return;
            if (!m_DoMoveArrow)
                return;
            UpdateMultiplyCoefficient();
        }
        
        #endregion
        
        #region nonpublic methods

        private void InitCoefficientBackgroundsColors()
        {
            m_BackColors = coeffBacks.Select(_Cb => _Cb.color.SetA(0.6f)).ToList();
            coeffBacks.ForEach(_Cb => _Cb.color = _Cb.color.SetA(0.6f));
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

        private void UpdateMultiplyCoefficient()
        {
            int multCoeff = GetMultiplyCoefficientCore();
            if (multCoeff == m_MultiplyCoefficient) 
                return;
            m_MultiplyCoefficient = multCoeff;
            MultiplyCoefficientChanged?.Invoke(multCoeff);
            HighlightCurrentCoefficient();
        }

        #endregion
    }
}