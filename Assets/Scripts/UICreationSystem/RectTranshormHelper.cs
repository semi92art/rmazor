using System.Text;
using Extentions;
using UnityEngine;
using Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UICreationSystem
{
    public class RectTranshormHelper : MonoBehaviour
    {
    }

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
            var anchMin = rTr.anchorMin;
            var anchMax = rTr.anchorMax;
            var anchPos = rTr.anchoredPosition;
            var pivot = rTr.pivot;
            var sizeDelta = rTr.sizeDelta;
            
            GUIStyle gs = new GUIStyle();
            gs.fontStyle = FontStyle.Bold;
            gs.fontSize = 13;
            
            GUILayout.Space(5);
            GUILayout.Label($"Anchor:\t\t {anchMin.x}  {anchMin.y}  {anchMax.x}  {anchMax.y}", gs);
            GUILayout.Space(5);

            GUILayout.Label("\t\tX:\tY:", gs);
            GUILayout.Space(5);
            
            string anchPosXstr = Mathf.FloorToInt(anchPos.x * 10f) % 10 == 0 ? $"{anchPos.x:F0}" : $"{anchPos.x:F1}";
            string anchPosYstr = Mathf.FloorToInt(anchPos.y * 10f) % 10 == 0 ? $"{anchPos.y:F0}" : $"{anchPos.y:F1}";
            GUILayout.Label($"Anch.position:\t {anchPosXstr}\t{anchPosYstr}", gs);
            GUILayout.Space(5);
            string pivotXstr = Mathf.FloorToInt(pivot.x * 10f) % 10 == 0 ? $"{pivot.x:F0}" : $"{pivot.x:F1}";
            string pivotYstr = Mathf.FloorToInt(pivot.y * 10f) % 10 == 0 ? $"{pivot.y:F0}" : $"{pivot.y:F1}";
            GUILayout.Label($"Pivot:\t\t {pivotXstr}\t{pivotYstr}", gs);
            GUILayout.Space(5);
            string sizeDeltaXstr = Mathf.FloorToInt(sizeDelta.x * 10f) % 10 == 0 ? $"{sizeDelta.x:F0}" : $"{sizeDelta.x:F1}";
            string sizeDeltaYstr = Mathf.FloorToInt(sizeDelta.y * 10f) % 10 == 0 ? $"{sizeDelta.y:F0}" : $"{sizeDelta.y:F1}";
            GUILayout.Label($"Size Delta:\t {sizeDeltaXstr}\t{sizeDeltaYstr}", gs);
            GUILayout.Space(5);
 

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

            if (GUILayout.Button("Clone"))
                m_RectTranshormHelper.gameObject.Clone();
        }
    }
#endif
}