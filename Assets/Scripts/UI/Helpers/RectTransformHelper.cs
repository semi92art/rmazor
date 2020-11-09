using System.Text;
using Extensions;
using UnityEditor;
using UnityEngine;
using Utils;

#if UNITY_EDITOR

#endif

namespace UI.Helpers
{
    public class RectTransformHelper : MonoBehaviour
    {
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(RectTransformHelper))]
    public class RectTransformHelperEditor : Editor
    {
        private RectTransformHelper m_RectTransformHelper;

        private GUIStyle m_White;
        private GUIStyle m_Black;
        
        private void OnEnable()
        {
            m_RectTransformHelper = target as RectTransformHelper;
            m_White = new GUIStyle(EditorStyles.label)
            {
                normal = {textColor = Color.white},
                fontStyle = FontStyle.Bold,
                fontSize = 13
            };
            m_Black = new GUIStyle(EditorStyles.label)
            {
                normal = {textColor = Color.black},
                fontStyle = FontStyle.Bold,
                fontSize = 13
            };
        }

        public override void OnInspectorGUI()
        {
            RectTransform rTr = m_RectTransformHelper.RTransform();
            var anchMin = rTr.anchorMin;
            var anchMax = rTr.anchorMax;
            var anchPos = rTr.anchoredPosition;
            var pivot = rTr.pivot;
            var sizeDelta = rTr.sizeDelta;


            GUIStyle gs = EditorGUIUtility.isProSkin ? m_White : m_Black;

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
                sb.Append($"Name: {m_RectTransformHelper.gameObject.name},\n");
                sb.Append($"Anchor Min: {rTr.anchorMin},\n");
                sb.Append($"Anchor Max: {rTr.anchorMax},\n");
                sb.Append($"AnchoredPosition: {rTr.anchoredPosition},\n");
                sb.Append($"Pivot: {rTr.pivot},\n");
                sb.Append($"SizeDelta: {rTr.sizeDelta})");
            
                sb.ToString().CopyToClipboard();
            }

            if (GUILayout.Button("Clone"))
                m_RectTransformHelper.gameObject.Clone();
            
            GUILayout.Space(5);
            GUILayout.Label("Additional info:", gs);
            GUILayout.Space(5);
            GUILayout.Label("Position:", gs);
            GUILayout.Label(rTr.position.ToString(), gs);
        }
    }
#endif
}