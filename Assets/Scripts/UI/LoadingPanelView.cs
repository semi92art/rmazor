using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class LoadingPanelView : MonoBehaviour
    {
        #region serialized fields
    
        public Image indicator;
        public Image indicator2;
        public Animator animator;
        public TextMeshProUGUI loading;
        public float speed = 50f;
        
        #endregion
        
        #region private fields
        
        private Transform m_Indicator;
        private Transform m_Indicator2;
        private bool m_DoLoading;
        private string m_LoadingText = "Loading";
        private int m_PointsState;
        private int m_TimePrev;
        
        #endregion
        
        #region public properties

        public bool DoLoading
        {
            get => m_DoLoading;
            set
            {
                indicator.enabled = value;
                indicator2.enabled = value;
                animator.enabled = value;
                m_DoLoading = value;
            }
        }

        #endregion
        
        #region engine methods
        
        private void Start()
        {
            m_Indicator = indicator.transform;
            m_Indicator2 = indicator2.transform;
            DoLoading = true;
        }
        
        private void Update()
        {
            IndicateLoading();
        }
        
        #endregion
        
        #region private methods
        
        private void IndicateLoading()
        {
            if (!DoLoading)
                return;
        
            m_Indicator.Rotate(Vector3.back, Time.deltaTime * speed);
            m_Indicator2.Rotate(Vector3.forward, Time.deltaTime * speed);

            int time = Mathf.FloorToInt(Time.time * 5f);
            if (time % 2 == 0 && time != m_TimePrev)
            {
                Utility.IncWithOverflow(ref m_PointsState, 4);
                loading.text = m_LoadingText + string.Concat(Enumerable.Repeat(".", m_PointsState));
            }

            m_TimePrev = time;
        }
    
        #endregion
    }
}