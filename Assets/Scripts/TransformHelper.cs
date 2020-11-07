using Extentions;
using Utils;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TransformHelper : MonoBehaviour
{ }

#if UNITY_EDITOR
[CustomEditor(typeof(TransformHelper))]
public class TransformHelperEditor : Editor
{
    private Transform m_Transform;
    private int m_ScreenPosIndex;
    private readonly string[] m_ScreenPosOptions = { "Center", "Top Left", "Top Right", "Bottom Left", "Bottom Right"};

    private void OnEnable()
    {
        m_Transform = (target as TransformHelper)?.transform;
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
        m_ScreenPosIndex = EditorGUILayout.Popup(m_ScreenPosIndex, m_ScreenPosOptions);
        GUILayout.EndHorizontal();
            
    }

    private void SetPosition()
    {
        switch (m_ScreenPosIndex)
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
        m_Transform.SetPosXY(bounds.center.x - bounds.size.x, bounds.center.y + bounds.size.y);
    }

    private void SetTopRight()
    {
        Bounds bounds = GameUtils.GetVisibleBounds();
        m_Transform.SetPosXY(bounds.center.x + bounds.size.x, bounds.center.y + bounds.size.y);
    }

    private void SetBottomLeft()
    {
        Bounds bounds = GameUtils.GetVisibleBounds();
        m_Transform.SetPosXY(bounds.center.x - bounds.size.x, bounds.center.y - bounds.size.y);
    }

    private void SetBottomRight()
    {
        Bounds bounds = GameUtils.GetVisibleBounds();
        m_Transform.SetPosXY(bounds.center.x + bounds.size.x, bounds.center.y - bounds.size.y);
    }
}
#endif
