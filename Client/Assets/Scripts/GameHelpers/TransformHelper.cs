﻿using Extensions;
using UnityEditor;
using UnityEngine;
using Utils;
#if UNITY_EDITOR

#endif

namespace GameHelpers
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
            if (m_Transform is RectTransform)
            {
                GUILayout.Label("This helper is not intended for gameobjects with RectTransform component");
                return;
            }
        
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
                case 0:
                    SetCenter();
                    break;
                case 1:
                    SetTopLeft();
                    break;
                case 2:
                    SetTopRight();
                    break;
                case 3:
                    SetBottomLeft();
                    break;
                case 4:
                    SetBottomRight();
                    break;
            }
        }

        private void SetCenter()
        {
            Bounds bounds = GameUtils.GetVisibleBounds();
            m_Transform.SetPosXY(bounds.center.x, bounds.center.y);
        }

        private void SetTopLeft()
        {
            Bounds bounds = GameUtils.GetVisibleBounds();
            m_Transform.SetPosXY(bounds.min.x, bounds.max.y);
        }

        private void SetTopRight()
        {
            Bounds bounds = GameUtils.GetVisibleBounds();
            m_Transform.SetPosXY(bounds.max.x, bounds.max.y);
        }

        private void SetBottomLeft()
        {
            Bounds bounds = GameUtils.GetVisibleBounds();
            m_Transform.SetPosXY(bounds.min.x, bounds.min.y);
        }

        private void SetBottomRight()
        {
            Bounds bounds = GameUtils.GetVisibleBounds();
            m_Transform.SetPosXY(bounds.max.x, bounds.min.y);
        }
    }
#endif
}