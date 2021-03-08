using Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Utils.Editor
{
    public static class EditorUtilsEx
    {
        public static void GuiButtonAction(string _Name, UnityAction _Action)
        {
            if (GUILayout.Button(_Name))
                _Action?.Invoke();
        }
        
        public static void GuiButtonAction(UnityAction _Action)
        {
            if (GUILayout.Button(_Action.Method.Name.WithSpaces()))
                _Action?.Invoke();
        }
    
        public static void GuiButtonAction<T>(string _Name, UnityAction<T> _Action, T _Arg)
        {
            if (GUILayout.Button(_Name))
                _Action?.Invoke(_Arg);
        }
        
        public static void GuiButtonAction<T>(UnityAction<T> _Action, T _Arg)
        {
            if (GUILayout.Button(_Action.Method.Name.WithSpaces()))
                _Action?.Invoke(_Arg);
        }

        public static void DrawUiLine(Color _C, int _Thickness = 2, int _Padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(_Padding+_Thickness));
            r.height = _Thickness;
            r.y += _Padding * 0.5f;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, _C);
        }

        public static void HorizontalZone(UnityAction _Action)
        {
            GUILayout.BeginHorizontal();
            _Action?.Invoke();
            GUILayout.EndHorizontal();
        }

        public static void GUIColorZone(Color _Color, UnityAction _Action)
        {
            var defCol = GUI.color;
            GUI.color = _Color;
            _Action?.Invoke();
            GUI.color = defCol;
        }
    }
}