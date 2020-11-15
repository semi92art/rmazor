using Constants;
using Lean.Touch;
using UnityEngine;
using Shapes;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PointsTapper
{
    public class PointItem : MonoBehaviour
    {
        #region serialized fields

        [SerializeField] private Disc background;
        [SerializeField] private Disc border;
        [SerializeField] private CircleCollider2D coll;
        [SerializeField] private Animator animator;

        #endregion
        
        #region private members

        private static int AkActivate => AnimKeys.Anim;
        private static int AkDeactivate => AnimKeys.Stop;
        
        #endregion
        
        #region api

        public float Radius
        {
            get => background.Radius;
            set => background.Radius = border.Radius = coll.radius = value;
        }

        public void Activate()
        {
            animator.SetTrigger(AkActivate);
        }

        public void Deactivate(LeanFinger _Finger)
        {
            animator.SetTrigger(AkDeactivate);
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
                m_Item.Deactivate(null);
            GUILayout.EndHorizontal();
        }
    }
}
#endif