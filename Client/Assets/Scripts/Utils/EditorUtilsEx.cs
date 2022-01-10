#if UNITY_EDITOR
using System;
using System.Reflection;
using DI.Extensions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Utils
{
    public static class EditorUtilsEx
    {
        public static void GuiButtonAction(string _Name, UnityAction _Action, params GUILayoutOption[] _Options)
        {
            if (GUILayout.Button(_Name, _Options))
                _Action?.Invoke();
        }
        
        public static void GuiButtonAction(UnityAction _Action, params GUILayoutOption[] _Options)
        {
            GuiButtonAction(_Action.Method.Name.WithSpaces(), _Action, _Options);
        }
    
        public static void GuiButtonAction<T>(
            string _Name,
            UnityAction<T> _Action, 
            T _Arg, 
            params GUILayoutOption[] _Options)
        {
            if (GUILayout.Button(_Name, _Options))
                _Action?.Invoke(_Arg);
        }

        public static void GuiButtonAction<T>(
            UnityAction<T> _Action,
            T _Arg,
            params GUILayoutOption[] _Options)
        {
            GuiButtonAction(_Action.Method.Name.WithSpaces(), _Action, _Arg, _Options);
        }
        
        public static void GuiButtonAction<T1, T2>(
            string _Name, 
            UnityAction<T1, T2> _Action, 
            T1 _Arg1,
            T2 _Arg2,
            params GUILayoutOption[] _Options)
        {
            if (GUILayout.Button(_Name, _Options))
                _Action?.Invoke(_Arg1, _Arg2);
        }

        public static void GuiButtonAction<T1, T2>(
            UnityAction<T1, T2> _Action,
            T1 _Arg1,
            T2 _Arg2,
            params GUILayoutOption[] _Options)
        {
            GuiButtonAction(_Action.Method.Name.WithSpaces(), _Action, _Arg1, _Arg2, _Options);
        }
        
        public static void GuiButtonAction<T1, T2, T3>(
            string _Name,
            UnityAction<T1, T2, T3> _Action,
            T1 _Arg1,
            T2 _Arg2,
            T3 _Arg3,
            params GUILayoutOption[] _Options)
        {
            if (GUILayout.Button(_Name, _Options))
                _Action?.Invoke(_Arg1, _Arg2, _Arg3);
        }

        public static void GuiButtonAction<T1, T2, T3>(
            UnityAction<T1, T2, T3> _Action,
            T1 _Arg1, 
            T2 _Arg2,
            T3 _Arg3,
            params GUILayoutOption[] _Options)
        {
            GuiButtonAction(_Action.Method.Name.WithSpaces(), _Action, _Arg1, _Arg2, _Arg3, _Options);
        }
        
        public static void GuiButtonAction<T1, T2, T3, T4>(
            string _Name,
            UnityAction<T1, T2, T3, T4> _Action,
            T1 _Arg1,
            T2 _Arg2,
            T3 _Arg3, 
            T4 _Arg4,
            params GUILayoutOption[] _Options)
        {
            if (GUILayout.Button(_Name, _Options))
                _Action?.Invoke(_Arg1, _Arg2, _Arg3, _Arg4);
        }

        public static void GuiButtonAction<T1, T2, T3, T4>(
            UnityAction<T1, T2, T3, T4> _Action,
            T1 _Arg1,
            T2 _Arg2,
            T3 _Arg3, 
            T4 _Arg4,
            params GUILayoutOption[] _Options)
        {
            GuiButtonAction(_Action.Method.Name.WithSpaces(), _Action, _Arg1, _Arg2, _Arg3, _Arg4, _Options);
        }
        
        public static void ScrollViewZone(
            ref Vector2 _ScrollPos,
            UnityAction _Action,
            bool _AlwaysShowHorizontal = false,
            bool _AlwaysShowVertical = false)
        {
            _ScrollPos = EditorGUILayout.BeginScrollView(_ScrollPos, _AlwaysShowHorizontal, _AlwaysShowVertical);
            _Action?.Invoke();
            EditorGUILayout.EndScrollView();
        }

        public static void HorizontalLine(Color? _C = null, int _Thickness = 2, int _Padding = 10)
        {
            if (_C == null)
                _C = Color.gray;
            var r = EditorGUILayout.GetControlRect(GUILayout.Height(_Padding+_Thickness));
            r.height = _Thickness;
            r.y += _Padding * 0.5f;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, _C.Value);
        }

        public static void HorizontalZone(UnityAction _Action)
        {
            GUILayout.BeginHorizontal();
            _Action?.Invoke();
            GUILayout.EndHorizontal();
        }
        
        public static void VerticalZone(UnityAction _Action)
        {
            GUILayout.BeginVertical();
            _Action?.Invoke();
            GUILayout.EndVertical();
        }

        public static void GUIColorZone(Color _Color, UnityAction _Action)
        {
            var defCol = GUI.color;
            GUI.color = _Color;
            _Action?.Invoke();
            GUI.color = defCol;
        }

        public static void FocusSceneCamera(Bounds _Bounds, bool _Instant = false)
        {
            SceneView.lastActiveSceneView.Frame(_Bounds, _Instant);
        }

        public static void ClearConsole()
        {
            var logEntries = Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            var clearMethod = logEntries?.GetMethod(
                "Clear", 
                BindingFlags.Static | BindingFlags.Public);
            clearMethod?.Invoke(null, null);
        }

        public static void GUIEnabledZone(bool _Enabled, UnityAction _Action)
        {
            bool prevEnabled = GUI.enabled;
            GUI.enabled = _Enabled;
            _Action?.Invoke();
            GUI.enabled = prevEnabled;
        }

        public static void SceneDirtyAction(UnityAction _Action)
        {
            _Action?.Invoke();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}
#endif