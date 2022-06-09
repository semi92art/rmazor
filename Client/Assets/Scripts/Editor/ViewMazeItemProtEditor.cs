using System.Linq;
using Common.Utils;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.MazeItems;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(ViewMazeItemProt)), CanEditMultipleObjects]
    public partial class ViewMazeItemProtEditor : UnityEditor.Editor
    {
        private const float AreaWidth  = 400f;
        private const float AreaHeight = 500f;
        
        private ViewMazeItemProt[] m_TargetsCopy;
        private EMazeItemType m_Type;

        private static GUIStyle GetHeader1Style()
        {
            var style = new GUIStyle("label")
            {
                fontStyle = FontStyle.Bold,
                fontSize = 14
            };
            return style;
        } 
        
        private void OnEnable()
        {
            m_TargetsCopy = targets.Cast<ViewMazeItemProt>().ToArray();
        }

        private void OnSceneGUI()
        {
            Handles.BeginGUI();
            EditorGUI.DrawRect(
                new Rect(20f, 20f, AreaWidth * 0.5f, 
                    EditorGUIUtility.singleLineHeight * 8f),
                new Color(0.18f, 0.17f, 0.18f));
            EditorUtilsEx.AreaZone(() => EditorUtilsEx.VerticalZone(DrawElementsControls),
                new Rect(20f, 20f, AreaWidth * 0.5f, AreaHeight));
            if (m_TargetsCopy.Length == 1)
            {
                EditorGUI.DrawRect(
                    new Rect(20f + AreaWidth * 0.5f, 20f, AreaWidth * 0.5f + 4f, 
                        EditorGUIUtility.singleLineHeight * 6f),
                    new Color(0.18f, 0.17f, 0.18f));
            }
            EditorUtilsEx.AreaZone(() => EditorUtilsEx.VerticalZone(DrawMazeControls),
            new Rect(20f + AreaWidth * 0.5f, 20f, AreaWidth * 0.5f, AreaHeight));
            Handles.EndGUI();
        }

        private void DrawElementsControls()
        {
            EditorUtilsEx.HorizontalZone(() =>
            {
                GUI.color = m_TargetsCopy.All(_Item => _Item.Props.IsNode) ? Color.white : Color.green;
                EditorUtilsEx.GuiButtonAction("Block", SetAsBlock, m_TargetsCopy);
                GUI.color = m_TargetsCopy.All(_Item => _Item.Props.IsNode) ? Color.green : Color.white;
                EditorUtilsEx.GuiButtonAction("Path", SetAsPathItem, m_TargetsCopy);
                SetBlank(m_TargetsCopy);
                if (m_TargetsCopy.Length != 1 || !m_TargetsCopy[0].Props.IsNode)
                    return;
                GUI.color = m_TargetsCopy[0].Props.IsStartNode ? Color.green : Color.white;
                EditorUtilsEx.GuiButtonAction("Start", SetAsPathItemStart, m_TargetsCopy[0]);
            });
            GUI.color = Color.white;
            var props = m_TargetsCopy[0].Props;
            var popupRect = new Rect(0, 20, 200, 20);

            if (!props.IsNode)
            {
                if (m_TargetsCopy.Length == 1)
                {
                    var type = MazeItemsPopup(popupRect, props.Type);
                    if (props.Type != type)
                        m_TargetsCopy[0].SetType(type, false, false);
                }
                else
                {
                    m_Type = MazeItemsPopup(popupRect, m_Type);
                    GUILayout.Space(20);
                    void SetType()
                    {
                        foreach (var t in m_TargetsCopy)
                            t.SetType(m_Type, false, false);
                    }
                    EditorUtilsEx.GuiButtonAction("Set type", SetType);
                }
            }
            GUILayout.Space(20);
            var firstType = m_TargetsCopy.First().Props.Type;
            if (m_TargetsCopy.All(_I => _I.Props.Type == firstType))
                DrawControlsForBlocksCollection(m_TargetsCopy);
        }
        
        private void DrawMazeControls()
        {
            if (m_TargetsCopy.Length != 1)
                return;
            var protItem = m_TargetsCopy[0];
            GUILayout.Label("Row:", GetHeader1Style());
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction("Remove", () => RemoveRow(protItem, false));
                GUILayout.Label("Add:");
                EditorUtilsEx.GuiButtonAction("Down", () => AddRow(protItem, false));
                EditorUtilsEx.GuiButtonAction("Up", () => AddRow(protItem, true));
            });
            GUILayout.Label("Column:", GetHeader1Style());
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction("Remove", () => RemoveColumn(protItem, false));
                GUILayout.Label("Add:");
                EditorUtilsEx.GuiButtonAction("Left", () => AddColumn(protItem, false));
                EditorUtilsEx.GuiButtonAction("Right", () => AddColumn(protItem, true));
            });
        }
    }
}