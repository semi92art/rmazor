using System.Linq;
using Constants;
using Lean.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class LoadingPanelView : SimpleUiDialogPanelView
    {
        #region serialized fields
    
        public Image outerIndicator1;
        public Image outerIndicator2;
        public Image innerIndicator;
        public Animator animator;
        public TextMeshProUGUI loading;
        public float speed = 50f;
        
        #endregion
        
        #region nonpublic members
        
        private Transform m_Indicator;
        private Transform m_Indicator2;
        private bool m_DoLoading;
        private string m_LoadingText = LeanLocalization.GetTranslationText("Loading");
        private int m_PointsState;
        private int m_TimePrev;
        
        #endregion
        
        #region api

        public bool DoLoading
        {
            get => m_DoLoading;
            set
            {
                outerIndicator1.enabled = value;
                outerIndicator2.enabled = value;
                animator.enabled = value;
                m_DoLoading = value;
            }
        }

        public void SetProgress(float _Percents, string _Text)
        {
            loading.text = _Text;
        }

        public void Break(string _Error)
        {
            loading.text = _Error;
            DoLoading = false;
        }

        #endregion
        
        #region engine methods

        protected override void OnEnable()
        {
            var outerColor = ColorUtils.GetColorFromCurrentPalette(
                CommonPaletteColors.UiLoadingPanelOuterIndicator);
            var innerColor = ColorUtils.GetColorFromCurrentPalette(
                CommonPaletteColors.UiLoadingPanelInnerIndicator);
            outerIndicator1.color = outerIndicator2.color = outerColor;
            innerIndicator.color = innerColor;
            base.OnEnable();
        }

        private void Start()
        {
            m_Indicator = outerIndicator1.transform;
            m_Indicator2 = outerIndicator2.transform;
            DoLoading = true;
        }
        
        private void Update()
        {
            IndicateLoading();
        }
        
        #endregion
        
        #region nonpublic methods
        
        private void IndicateLoading()
        {
            if (!DoLoading)
                return;
        
            m_Indicator.Rotate(Vector3.back, Time.deltaTime * speed);
            m_Indicator2.Rotate(Vector3.forward, Time.deltaTime * speed);

            int time = default; // FIXME
            // int time = Mathf.FloorToInt(UiTimeProvider.Instance.Time * 5f);
            if (time % 2 == 0 && time != m_TimePrev)
            {
                CommonUtils.IncWithOverflow(ref m_PointsState, 4);
                // m_LoadingText = LeanLocalization.GetTranslationText("Loading");
                loading.text = m_LoadingText + string.Concat(Enumerable.Repeat(".", m_PointsState));
            }

            m_TimePrev = time;
        }
    
        #endregion
    }
}