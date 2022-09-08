using Common.Extensions;
using Common.Utils;
using UnityEditor;
using UnityEngine;

namespace Common.Helpers
{
    public class TransformHelper : MonoBehaviour
    {
        public int ScreenPosIndex { get; set; }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(TransformHelper))]
    public class TransformHelperEditor : Editor
    {
        private Transform m_Transform;
        private TransformHelper m_TransformHelper;
        private readonly string[] m_ScreenPosOptions =
        {
            "Center", 
            "Top Left", 
            "Top Right",
            "Bottom Left", 
            "Bottom Right"
        };

        private void OnEnable()
        {
            m_TransformHelper = (TransformHelper)target;
            m_Transform = m_TransformHelper.transform;
        }

        public override void OnInspectorGUI()
        {
            if (m_TransformHelper.IsNull())
                return;
            if (GUILayout.Button("Clone"))
                m_TransformHelper.gameObject.Clone();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Set Position"))
                SetPosition();
            GUILayout.Label("Position:");
            m_TransformHelper.ScreenPosIndex = EditorGUILayout.Popup(
                m_TransformHelper.ScreenPosIndex, m_ScreenPosOptions);
            GUILayout.EndHorizontal();
        }

        private void SetPosition()
        {
            switch (m_TransformHelper.ScreenPosIndex)
            {
                case 0: SetCenter();      break;
                case 1: SetTopLeft();     break;
                case 2: SetTopRight();    break;
                case 3: SetBottomLeft();  break;
                case 4: SetBottomRight(); break;
            }
        }

        private void SetCenter()
        {
            var bounds = GraphicUtils.GetVisibleBounds();
            m_Transform.SetPosXY(bounds.center.x, bounds.center.y);
        }

        private void SetTopLeft()
        {
            var bounds = GraphicUtils.GetVisibleBounds();
            m_Transform.SetPosXY(bounds.min.x, bounds.max.y);
        }

        private void SetTopRight()
        {
            var bounds = GraphicUtils.GetVisibleBounds();
            m_Transform.SetPosXY(bounds.max.x, bounds.max.y);
        }

        private void SetBottomLeft()
        {
            var bounds = GraphicUtils.GetVisibleBounds();
            m_Transform.SetPosXY(bounds.min.x, bounds.min.y);
        }

        private void SetBottomRight()
        {
            var bounds = GraphicUtils.GetVisibleBounds();
            m_Transform.SetPosXY(bounds.max.x, bounds.min.y);
        }
    }
#endif
}