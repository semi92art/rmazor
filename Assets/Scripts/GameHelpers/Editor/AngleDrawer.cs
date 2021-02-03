using UnityEditor;
using UnityEngine;

namespace GameHelpers.Editor
{
    public class AngleAttribute : PropertyAttribute
    {
        public readonly float Snap;
        public readonly float Min;
        public readonly float Max;
 
        public AngleAttribute()
        {
            Snap = 1;
            Min = -360;
            Max = 360;
        }
 
        public AngleAttribute(float _Snap)
        {
            Snap = _Snap;
            Min = -360;
            Max = 360;
        }
 
        public AngleAttribute(float _Snap, float _Min, float _Max)
        {
            Snap = _Snap;
            Min = _Min;
            Max = _Max;
        }
    }

    [CustomPropertyDrawer(typeof(AngleAttribute))]
    public class AngleDrawer : PropertyDrawer
    {
        private static Vector2 _mousePosition;
        private static readonly Texture2D KnobBack = Resources.Load("Editor Icons/Dial") as Texture2D;
        private static readonly Texture2D Knob = Resources.Load("Editor Icons/DialButton") as Texture2D;

        public static float FloatAngle(Rect _Rect, float _Value, float _Snap = -1, float _Min = -1, float _Max = -1)
        {
            return FloatAngle(_Rect, _Value, _Snap, _Min, _Max, Vector2.up);
        }

        private static float FloatAngle(Rect _Rect, float _Value, float _Snap, float _Min, float _Max, Vector2 _ZeroVector)
        {
            int id = GUIUtility.GetControlID(FocusType.Passive, _Rect);
            float originalValue = _Value;
            Rect knobRect = new Rect(_Rect.x, _Rect.y, _Rect.height, _Rect.height);

            float delta;
            if (Mathf.Abs(_Min - _Max) > float.Epsilon)
                delta = (_Max - _Min) / 360;
            else
                delta = 1;

            if (Event.current != null)
            {
                if (Event.current.type == EventType.MouseDown && knobRect.Contains(Event.current.mousePosition))
                {
                    GUIUtility.hotControl = id;
                    _mousePosition = Event.current.mousePosition;
                }
                else if (Event.current.type == EventType.MouseUp && GUIUtility.hotControl == id)
                {
                    GUIUtility.hotControl = 0;
                }
                else if (Event.current.type == EventType.MouseDrag && GUIUtility.hotControl == id)
                {
                    Vector2 mouseStartDirection = (_mousePosition - knobRect.center).normalized;
                    float startAngle = CalculateAngle(Vector2.up, mouseStartDirection);

                    Vector2 mouseNewDirection = (Event.current.mousePosition - knobRect.center).normalized;
                    float newAngle = CalculateAngle(Vector2.up, mouseNewDirection);
                    
                    float sign = Mathf.Sign(newAngle - startAngle);
                    float delta2 = Mathf.Min(Mathf.Abs(newAngle - startAngle), Mathf.Abs(newAngle - startAngle + 360f),
                        Mathf.Abs(newAngle - startAngle - 360f));
                    _Value -= delta2 * sign;
                    
                    if (_Snap > 0)
                    {
                        float mod = _Value % _Snap;

                        if (mod < (delta * 3) || Mathf.Abs(mod - _Snap) < (delta * 3))
                            _Value = Mathf.Round(_Value / _Snap) * _Snap;
                    }

                    if (Mathf.Abs(_Value - originalValue) > float.Epsilon)
                    {
                        _mousePosition = Event.current.mousePosition;
                        GUI.changed = true;
                    }
                }
            }

            float angleOffset = (CalculateAngle(Vector2.up, _ZeroVector) + 360f) % 360f;

            GUI.DrawTexture(knobRect, KnobBack);
            Matrix4x4 matrix = GUI.matrix;

            if (Mathf.Abs(_Min - _Max) > float.Epsilon)
                GUIUtility.RotateAroundPivot((angleOffset + _Value) * (360 / (_Max - _Min)), knobRect.center);
            else
                GUIUtility.RotateAroundPivot((angleOffset + _Value), knobRect.center);

            GUI.DrawTexture(knobRect, Knob);
            GUI.matrix = matrix;

            Rect label = new Rect(_Rect.x + _Rect.height, _Rect.y + (_Rect.height / 2) - 9, _Rect.height, 18);
            _Value = Mathf.Round(_Value);
            _Value = EditorGUI.FloatField(label, _Value);

            if (Mathf.Abs(_Min - _Max) > float.Epsilon)
                _Value = Mathf.Clamp(_Value, _Min, _Max);

            return _Value;
        }

        private static float CalculateAngle(Vector3 _From, Vector3 _To)
        {
            float angle = Vector3.Angle(_From, _To);
            return Vector3.Angle(Vector3.right, _To) > 90f ? 360f - angle : angle;
    
        }
    }
}