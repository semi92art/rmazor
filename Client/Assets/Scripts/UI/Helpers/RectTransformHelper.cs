using System.Text;
using DI.Extensions;
using UnityEditor;
using UnityEngine;
using Utils;

namespace UI.Helpers
{
    public class RectTransformHelper : TransformHelper { }

#if UNITY_EDITOR
    [CustomEditor(typeof(RectTransformHelper))]
    public class RectTransformHelperEditor : TransformHelperEditor
    {
        private RectTransformHelper m_RectTransformHelper;
        private GUIStyle m_Bold;

        private void OnEnable()
        {
            m_RectTransformHelper = target as RectTransformHelper;
            m_Bold = new GUIStyle(EditorStyles.label)
                      {
                          fontStyle = FontStyle.Bold,
                          fontSize = 13
                      };
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var rTr = m_RectTransformHelper.RTransform();
            var anchMin = rTr.anchorMin;
            var anchMax = rTr.anchorMax;
            var anchPos = rTr.anchoredPosition;
            var pivot = rTr.pivot;
            var sizeDelta = rTr.sizeDelta;

            
            GUILayout.Space(5);
            GUILayout.Label($"Anchor:\t\t {anchMin.x}  {anchMin.y}  {anchMax.x}  {anchMax.y}", m_Bold);
            GUILayout.Space(5);

            GUILayout.Label("\t\tX:\tY:", m_Bold);
            GUILayout.Space(5);
            
            string anchPosXstr = Mathf.FloorToInt(anchPos.x * 10f) % 10 == 0 ? $"{anchPos.x:F0}" : $"{anchPos.x:F1}";
            string anchPosYstr = Mathf.FloorToInt(anchPos.y * 10f) % 10 == 0 ? $"{anchPos.y:F0}" : $"{anchPos.y:F1}";
            GUILayout.Label($"Anch.position:\t {anchPosXstr}\t{anchPosYstr}", m_Bold);
            GUILayout.Space(5);
            string pivotXstr = Mathf.FloorToInt(pivot.x * 10f) % 10 == 0 ? $"{pivot.x:F0}" : $"{pivot.x:F1}";
            string pivotYstr = Mathf.FloorToInt(pivot.y * 10f) % 10 == 0 ? $"{pivot.y:F0}" : $"{pivot.y:F1}";
            GUILayout.Label($"Pivot:\t\t {pivotXstr}\t{pivotYstr}", m_Bold);
            GUILayout.Space(5);
            string sizeDeltaXstr = Mathf.FloorToInt(sizeDelta.x * 10f) % 10 == 0 ? $"{sizeDelta.x:F0}" : $"{sizeDelta.x:F1}";
            string sizeDeltaYstr = Mathf.FloorToInt(sizeDelta.y * 10f) % 10 == 0 ? $"{sizeDelta.y:F0}" : $"{sizeDelta.y:F1}";
            GUILayout.Label($"Size Delta:\t {sizeDeltaXstr}\t{sizeDeltaYstr}", m_Bold);
            GUILayout.Space(5);
 

            if (GUILayout.Button("Copy to clipboard"))
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Name: {m_RectTransformHelper.gameObject.name},");
                sb.AppendLine($"Anchor Min: {rTr.anchorMin},");
                sb.AppendLine($"Anchor Max: {rTr.anchorMax},");
                sb.AppendLine($"AnchoredPosition: {rTr.anchoredPosition},");
                sb.AppendLine($"Pivot: {rTr.pivot},");
                sb.AppendLine($"SizeDelta: {rTr.sizeDelta})");
            
                sb.ToString().CopyToClipboard();
            }

            GUILayout.Space(5);
            GUILayout.Label("Additional info:", m_Bold);
            GUILayout.Space(5);
            GUILayout.Label("Position:", m_Bold);
            GUILayout.Label(rTr.position.ToString(), m_Bold);
        }
    }
#endif
}