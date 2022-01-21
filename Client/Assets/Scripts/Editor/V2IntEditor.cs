using Common.Entities;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(V2Int))]
public class V2IntEditor : PropertyDrawer
{
    public override void OnGUI(Rect _Position, SerializedProperty _Property, GUIContent _Label)
    {
        var x = _Property.FindPropertyRelative("x");
        var y = _Property.FindPropertyRelative("y");
        
        EditorGUI.BeginProperty(_Position, _Label, _Property);
        {
            EditorGUI.Vector2IntField(_Position, _Label, new Vector2Int(x.intValue, y.intValue));
        }
        EditorGUI.EndProperty();
    }
}