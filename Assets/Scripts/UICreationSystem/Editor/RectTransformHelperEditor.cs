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
        RectTransform rectTransform = m_RectTranshormHelper.GetComponent<RectTransform>();
        rectTransform.anchorMin = EditorGUILayout.Vector2Field("Anchor Min", rectTransform.anchorMin);
        rectTransform.anchorMax = EditorGUILayout.Vector2Field("Anchor Max", rectTransform.anchorMax);
        rectTransform.anchoredPosition =
            EditorGUILayout.Vector2Field("Anchored Position", rectTransform.anchoredPosition);
        rectTransform.pivot = EditorGUILayout.Vector2Field("Pivot", rectTransform.pivot);
        rectTransform.sizeDelta = EditorGUILayout.Vector2Field("Size Delta", rectTransform.sizeDelta);

        if (GUILayout.Button("Copy constructor to clipboard"))
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("UIFactory.UIRectTransform(\n");
            sb.Append("[GROUP],\n");
            sb.Append($"\"{m_RectTranshormHelper.gameObject.name}\",\n");
            sb.Append($"new Vector2{rectTransform.anchorMin.ToStringAlt()},\n");
            sb.Append($"new Vector2{rectTransform.anchorMax.ToStringAlt()},\n");
            sb.Append($"new Vector2{rectTransform.anchoredPosition.ToStringAlt()},\n");
            sb.Append($"new Vector2{rectTransform.pivot.ToStringAlt()},\n");
            sb.Append($"new Vector2{rectTransform.sizeDelta.ToStringAlt()})");
            
            sb.ToString().CopyToClipboard();
        }
    }
}
