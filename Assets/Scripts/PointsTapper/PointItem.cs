using Constants;
using Lean.Touch;
using UnityEngine;
using Shapes;
using Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PointsTapper
{
    public class PointItem : MonoBehaviour, IActivated
    {
        #region serialized fields

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
        private System.Action<bool, PointItem> m_OnActivated;
        
        #endregion
        
        #region api
        public bool Activated { get; set; }

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
        
        public void SetOnActivated(System.Action<bool, PointItem> _OnActivated)
        {
            m_OnActivated = _OnActivated;
        }
        
        public void Activate()
        {
            timer.AngRadiansStart = 90f * Mathf.Deg2Rad;
            animator.SetTrigger(AkActivate);
        }

        public void Deactivate()
        {
            Deactivate(null);
        }

        #endregion
        
        #region animator

        public void ActivationFinished()
        {
            Activated = true;
            m_OnActivated?.Invoke(true, this);

            Coroutines.Run(Coroutines.Lerp(
                90f, 
                -270f, 
                ExistenceTime,
                _Value => border.AngRadiansStart = _Value * Mathf.Deg2Rad, 
                () => Deactivate(null),
                () => !Activated));
        }

        public void DeactivationFinished()
        {
            Activated = false;
            m_OnActivated?.Invoke(false, this);
            transform.position = DefaultPosition;
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

        #endregion
        
        #region private methods
        
        protected void Deactivate(LeanFinger _Finger)
        {
            animator.SetTrigger(AkDeactivate);
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
                m_Item.Activate();
            if (GUILayout.Button("Deactivate"))
                m_Item.Deactivate();
            GUILayout.EndHorizontal();
        }
    }
}
#endif