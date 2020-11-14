﻿#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Utils
{
    public static class EditorUtils
    {
        public static void DrawUiLine(Color _C, int _Thickness = 2, int _Padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(_Padding+_Thickness));
            r.height = _Thickness;
            r.y += _Padding * 0.5f;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, _C);
        }
    }
}

#endif