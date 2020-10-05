using System.Text;
using UICreationSystem;
using UnityEngine;
using Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RectTranshormHelper : MonoBehaviour
{ }

#if UNITY_EDITOR
[CustomEditor(typeof(RectTranshormHelper))]
public class RectTransformHelperEditor : Editor
{
    private RectTranshormHelper m_RectTranshormHelper;

    private void OnEnable()
    {
        m_RectTranshormHelper = target as RectTranshormHelper;
    }

    public override void OnInspectorGUI()
    {
        RectTransform rTr = m_RectTranshormHelper.RTransform();
        rTr.anchorMin = EditorGUILayout.Vector2Field("Anchor Min", rTr.anchorMin);
        rTr.anchorMax = EditorGUILayout.Vector2Field("Anchor Max", rTr.anchorMax);
        rTr.anchoredPosition =
            EditorGUILayout.Vector2Field("Anchored Position", rTr.anchoredPosition);
        rTr.pivot = EditorGUILayout.Vector2Field("Pivot", rTr.pivot);
        rTr.sizeDelta = EditorGUILayout.Vector2Field("Size Delta", rTr.sizeDelta);

        if (GUILayout.Button("Copy to clipboard"))
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Name: {m_RectTranshormHelper.gameObject.name},\n");
            sb.Append($"Anchor Min: {rTr.anchorMin},\n");
            sb.Append($"Anchor Max: {rTr.anchorMax},\n");
            sb.Append($"AnchoredPosition: {rTr.anchoredPosition},\n");
            sb.Append($"Pivot: {rTr.pivot},\n");
            sb.Append($"SizeDelta: {rTr.sizeDelta})");
            
            sb.ToString().CopyToClipboard();
        }
    }
}
#endif
