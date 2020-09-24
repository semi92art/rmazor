using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using Utils;


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

        if (GUILayout.Button("Copy constructor to clipboard"))
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("UIFactory.UIRectTransform(\n");
            sb.Append("[GROUP],\n");
            sb.Append($"\"{m_RectTranshormHelper.gameObject.name}\",\n");
            sb.Append($"new Vector2{rTr.anchorMin.ToStringAlt()},\n");
            sb.Append($"new Vector2{rTr.anchorMax.ToStringAlt()},\n");
            sb.Append($"new Vector2{rTr.anchoredPosition.ToStringAlt()},\n");
            sb.Append($"new Vector2{rTr.pivot.ToStringAlt()},\n");
            sb.Append($"new Vector2{rTr.sizeDelta.ToStringAlt()})");
            
            sb.ToString().CopyToClipboard();
        }
    }
}
