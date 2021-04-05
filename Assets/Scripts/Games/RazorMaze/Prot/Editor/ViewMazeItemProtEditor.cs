using System.Collections.Generic;
using System.Linq;
using Entities;
using Exceptions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.MazeItems;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Utils.Editor;

namespace Games.RazorMaze.Prot.Editor
{
    [CustomEditor(typeof(ViewMazeItemProt)), CanEditMultipleObjects]
    public class ViewMazeItemProtEditor : UnityEditor.Editor
    {
        private ViewMazeItemProt[] targetsCopy;
        private EMazeItemType m_Type;
        
        private void OnEnable()
        {
            targetsCopy = targets.Cast<ViewMazeItemProt>().ToArray();
        }
        
        private void OnSceneGUI()
        {
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(20, 20, 200, 500));
            
            EditorGUILayout.BeginVertical();
            EditorUtilsEx.HorizontalZone(() =>
            {
                GUI.color = targetsCopy.All(_Item => _Item.Props.IsNode) ? Color.white : Color.green;
                EditorUtilsEx.GuiButtonAction("Block", SetAsBlock, targetsCopy);
                GUI.color = targetsCopy.All(_Item => _Item.Props.IsNode) ? Color.green : Color.white;
                EditorUtilsEx.GuiButtonAction("Path", SetAsPathItem, targetsCopy);
                if (targetsCopy.Length == 1)
                {
                    GUI.color = targetsCopy[0].Props.IsStartNode ? Color.green : Color.white;
                    EditorUtilsEx.GuiButtonAction("Start", SetAsPathItemStart, targetsCopy[0]);    
                }
            });
            
            GUI.color = Color.white;
            var props = targetsCopy[0].Props;
            var popupRect = new Rect(0, 20, 200, 20);
            
            if (!props.IsNode)
            {
                if (targetsCopy.Length == 1)
                {
                    props.Type = (EMazeItemType)EditorGUI.EnumPopup(popupRect, props.Type);
                    if (targetsCopy[0].typeCheck != props.Type)
                        targetsCopy[0].SetType(props.Type, false, false);
                }
                else
                {
                    m_Type = (EMazeItemType)EditorGUI.EnumPopup(popupRect, m_Type);
                    GUILayout.Space(20);
                    EditorUtilsEx.GuiButtonAction("Set type", () =>
                    {
                        foreach (var t in targetsCopy)
                        {
                            t.SetType(m_Type, false, false);
                        }
                    });
                }
            }
            
            GUILayout.Space(20);
            
            if (targetsCopy.Length == 1)
                DrawControlsForSingleBlock(props);
            else if (targetsCopy.Length == 2 && targetsCopy
                .All(_T => _T.Props.Type == EMazeItemType.Portal))
                EditorUtilsEx.GuiButtonAction(LinkPortals, targetsCopy[0].Props, targetsCopy[1].Props);

            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
            Handles.EndGUI();
        }

        private static void SetAsBlock(IEnumerable<ViewMazeItemProt> _Items)
        {
            foreach (var item in _Items)
                item.SetType(item.Props.Type, false, false);
        }

        private static void SetAsPathItem(IEnumerable<ViewMazeItemProt> _Items)
        {
            foreach (var item in _Items)
                item.SetType(item.Props.Type, true, false);
        }

        private static void SetAsPathItemStart(ViewMazeItemProt _Item)
        {
            _Item.SetType(_Item.Props.Type, true, !_Item.Props.IsStartNode);
        }

        private void DrawControlsForSingleBlock(ViewMazeItemProps _Props)
        {
            switch (_Props.Type)
            {
                case EMazeItemType.TrapMoving:
                case EMazeItemType.GravityBlock:
                case EMazeItemType.GravityTrap:
                    DrawControlsMovingBlock(_Props);
                    break;
                case EMazeItemType.Turret:
                case EMazeItemType.TrapReact:
                    DrawControlsDirectedBlock(_Props);
                    break;
                case EMazeItemType.Portal:
                case EMazeItemType.Block:
                case EMazeItemType.ShredingerBlock:
                case EMazeItemType.TrapIncreasing:
                case EMazeItemType.TurretRotating:
                    // do nothing
                    break;
                default: throw new SwitchCaseNotImplementedException(_Props.Type);
            }
        }
        
        private void DrawControlsMovingBlock(ViewMazeItemProps _Props)
        {
            EditorUtilsEx.GUIColorZone(Color.black,() => GUILayout.Label("Path:"));
            bool pathExist = _Props.Path.Any();
            int lastPathIndex = _Props.Path.Count - 1;
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction("Add", () =>
                {
                    var pointToAdd = !pathExist ? _Props.Position : _Props.Path.Last();
                    _Props.Path?.Add(pointToAdd);
                });
                if (pathExist)
                    EditorUtilsEx.GuiButtonAction("Remove Last", () => _Props.Path.RemoveAt(lastPathIndex));
            });
            if (!pathExist)
                return;
            EditorUtilsEx.GuiButtonAction("△", () => 
                    _Props.Path[lastPathIndex] = _Props.Path[lastPathIndex].PlusY(1));
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction("◁", () => 
                    _Props.Path[lastPathIndex] = _Props.Path[lastPathIndex].MinusX(1));
                EditorUtilsEx.GuiButtonAction("▷", () => 
                    _Props.Path[lastPathIndex] = _Props.Path[lastPathIndex].PlusX(1));    
            });
            EditorUtilsEx.GuiButtonAction("▽", () => 
                _Props.Path[lastPathIndex] = _Props.Path[lastPathIndex].MinusY(1));
        }

        private void DrawControlsDirectedBlock(ViewMazeItemProps _Props)
        {
            EditorUtilsEx.GUIColorZone(Color.black,() => GUILayout.Label("Direction:"));
            if (_Props.Type == EMazeItemType.Turret)
                DrawControlsDirectedBlockSingle(_Props);
            else DrawControlsDirectedBlockMultiple(_Props);
        }

        private void DrawControlsDirectedBlockSingle(ViewMazeItemProps _Props)
        {
            UnityAction<IEnumerable<V2Int>> setDir = _Directions => _Props.Directions = _Directions.ToList();
            System.Func<V2Int, Color> getCol = _Direction => _Props.Directions.Contains(_Direction) ? Color.green : Color.white;
            EditorUtilsEx.GUIColorZone(getCol(V2Int.up), () => EditorUtilsEx.GuiButtonAction("△", setDir, new []{V2Int.up}));
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GUIColorZone(getCol(V2Int.left), () => EditorUtilsEx.GuiButtonAction("◁", setDir, new []{V2Int.left}));
                EditorUtilsEx.GUIColorZone(getCol(V2Int.right), () => EditorUtilsEx.GuiButtonAction("▷", setDir, new []{V2Int.right}));
            });
            EditorUtilsEx.GUIColorZone(getCol(V2Int.down), () => EditorUtilsEx.GuiButtonAction("▽", setDir, new []{V2Int.down}));
        }

        private void DrawControlsDirectedBlockMultiple(ViewMazeItemProps _Props)
        {
            UnityAction<V2Int> setDir = _Direction =>
            {
                if (_Props.Directions.Contains(V2Int.zero))
                    _Props.Directions.Remove(V2Int.zero);
                if (_Props.Directions.Contains(_Direction))
                    _Props.Directions.Remove(_Direction);
                else _Props.Directions.Add(_Direction);
            };
            System.Func<V2Int, Color> getCol = _Direction => _Props.Directions.Contains(_Direction) ? Color.green : Color.white;
            EditorUtilsEx.GUIColorZone(getCol(V2Int.up), () => EditorUtilsEx.GuiButtonAction("△", setDir, V2Int.up));
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GUIColorZone(getCol(V2Int.left), () => EditorUtilsEx.GuiButtonAction("◁", setDir, V2Int.left));
                EditorUtilsEx.GUIColorZone(getCol(V2Int.right), () => EditorUtilsEx.GuiButtonAction("▷", setDir, V2Int.right));
            });
            EditorUtilsEx.GUIColorZone(getCol(V2Int.down), () => EditorUtilsEx.GuiButtonAction("▽", setDir, V2Int.down));
        }

        private void LinkPortals(ViewMazeItemProps _Props1, ViewMazeItemProps _Props2)
        {
            _Props1.Pair = _Props2.Position;
            _Props2.Pair = _Props1.Position;
        }
    }
}