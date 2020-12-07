using Constants;
using UnityEngine;
using Shapes;
using UnityEngine.Events;
using Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PointsTapper
{
    public enum PointType
    {
        Default,
        Bad,
        BonusGold,
        BonusDiamonds,
        Unknown
    }
    
    public class PointItem : MonoBehaviour, IActivated
    {
        #region serialized fields

        [SerializeField] private PointType pointType;
        [SerializeField] private Disc timer;
        [SerializeField] private Disc background;
        [SerializeField] private Disc border;
        [SerializeField] private CircleCollider2D coll;
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer icon;

        #endregion
        
        #region private members

        private static Vector3 DefaultPosition => new Vector3(-1e5f, 0, 0); 
        private static int AkActivate => AnimKeys.Anim;
        private static int AkDeactivate => AnimKeys.Stop;
        private UnityAction m_TapAction;
        private UnityAction m_NotTappedAction;
        private Coroutine m_ExistenceCoroutine;
        private bool m_IsTapped;
        private bool m_Activated;
        
        #endregion
        
        #region api

        public bool Activated
        {
            get => m_Activated;
            set
            {
                if (m_Activated == value)
                    return;
                ForceActivate(value);
            }
        }

        public void ForceActivate(bool _Active)
        {
            if (_Active)
                Activate();
            else
                DeactivateWithoutNotification();
        }

        public float Radius
        {
            get => timer.Radius;
            set
            {
                timer.Radius = background.Radius = border.Radius = coll.radius = value;
                if (icon != null)
                    icon.transform.localScale = new Vector2(value * 1f, value * 1f);
            }
        }

        public float ExistenceTime { get; set; } = 4f;
        
        public void SetActions(UnityAction _TapAction, UnityAction _NotTappedAction)
        {
            m_TapAction = _TapAction;
            m_NotTappedAction = _NotTappedAction;
        }
        
        public virtual void Tap()
        {
            m_IsTapped = true;
            m_TapAction?.Invoke();
            Deactivate();
        }
        
        protected virtual void Activate()
        {
            m_IsTapped = false;
            border.AngRadiansStart = 90f * Mathf.Deg2Rad;
            animator.SetTrigger(AkActivate);
        }
        
        protected virtual void Deactivate()
        {
            if (!m_IsTapped)
                m_NotTappedAction?.Invoke();
            animator.SetTrigger(AkDeactivate);
        }

        protected virtual void DeactivateWithoutNotification()
        {
            animator.SetTrigger(AkDeactivate);
        }

        #endregion
        
        #region animator

        public void ActivationFinished()
        {
            m_Activated = true;

            m_ExistenceCoroutine = Coroutines.Run(Coroutines.Delay(() =>
            {
                Coroutines.Run(Coroutines.Lerp(
                    90f,
                    -270f,
                    ExistenceTime,
                    _Value => border.AngRadiansStart = _Value * Mathf.Deg2Rad,
                    GameTimeProvider.Instance,
                    Deactivate,
                    () => !m_Activated));
            }, 0.3f));
        }

        public void DeactivationFinished()
        {
            m_Activated = false;
            transform.position = DefaultPosition;
            enabled = false;
            gameObject.SetActive(false);
        }
        
        #endregion
        
        #region engine methods

        private void Start()
        {
            background.Type = DiscType.Pie;
            background.DashSpace = DashSpace.Meters;
            border.DashSpace = DashSpace.Meters;
            background.AngRadiansStart = Mathf.Deg2Rad * 90f;
            background.AngRadiansEnd = Mathf.Deg2Rad * -270f;
            transform.position = DefaultPosition;
        }

        private void OnDestroy()
        {
            Coroutines.Stop(m_ExistenceCoroutine);
            Activated = false;
        }

        #endregion
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(PointItem))]
    public class PointItemEditor : Editor
    {
        private PointItem m_Item;

        private void OnEnable()
        {
            m_Item = (PointItem) target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUI.enabled = Application.isPlaying;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Activate"))
                m_Item.Activated = true;
            if (GUILayout.Button("Deactivate"))
                m_Item.Activated = false;
            GUILayout.EndHorizontal();
        }
    }
    
#endif
}
