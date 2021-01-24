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
    
        public static void GuiButtonAction<T>(string _Name, UnityAction<T> _Action, T _Arg)
        {
            if (GUILayout.Button(_Name))
                _Action?.Invoke(_Arg);
        }
    }
}