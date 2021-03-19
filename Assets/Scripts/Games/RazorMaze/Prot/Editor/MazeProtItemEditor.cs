using System;
using System.Linq;
using Entities;
using Exceptions;
using Extensions;
using UnityEditor;
using UnityEngine;
using Utils.Editor;

namespace Games.RazorMaze.Prot.Editor
{
    [CustomEditor(typeof(MazeProtItem)), CanEditMultipleObjects]
    public class MazeProtItemEditor : UnityEditor.Editor
    {
        private readonly Array m_Types = Enum.GetValues(typeof(MazeItemType));
        private MazeProtItem[] targetsCopy;
        private void OnEnable()
        {
            targetsCopy = targets.Cast<MazeProtItem>().ToArray();
        }
        
        private void OnSceneGUI()
        {
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(20, 20, 150, 500));
        
            EditorGUILayout.BeginVertical();
            var targ = targetsCopy[0];
            if (targetsCopy.Length == 1)
            {
                foreach (var t in m_Types.Cast<MazeItemType>())
                {
                    SetGUIColors(targ.Type, t);
                    if (SelectTypeButtonPressed(t))
                        targ.Type = t;
                }
            }
            else
            {
                bool allEquals = targetsCopy.All(_T => _T.Type == targ.Type);
                if (allEquals)
                {
                    foreach (var t in m_Types.Cast<MazeItemType>())
                    {
                        SetGUIColors(targ.Type, t);
                        if (!SelectTypeButtonPressed(t)) 
                            continue;
                        foreach (var tg in targetsCopy)
                            tg.Type = t;
                    }   
                }
                else
                {
                    SetGUIColorsIdle();
                    foreach (var t in m_Types.Cast<MazeItemType>())
                    {
                        if (!SelectTypeButtonPressed(t)) 
                            continue;
                        foreach (var tg in targetsCopy)
                            tg.Type = t;
                    }   
                }
            }

            GUILayout.Space(10);
            if (targetsCopy.Length == 1)
            {
                switch (targ.Type)
                {
                    case MazeItemType.ObstacleMoving:
                        DrawControlsObstacle(targ);
                        break;
                    case MazeItemType.ObstacleTrap:
                        DrawControlsTrap(targ);
                        break;
                    case MazeItemType.Node:
                    case MazeItemType.NodeStart:
                    case MazeItemType.Obstacle:
                    case MazeItemType.ObstacleTrapMoving:
                        // do nothing
                        break;
                    default: throw new SwitchCaseNotImplementedException(targ.Type);
                }
            }
            
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
            Handles.EndGUI();
        }
        
        private void DrawControlsObstacle(MazeProtItem _Item)
        {
            bool pathExist = _Item.path.Any();
            int lastPathIndex = _Item.path.Count - 1;
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction("Add", () =>
                {
                    var pointToAdd = !pathExist ? _Item.start : _Item.path.Last();
                    _Item.path?.Add(pointToAdd);
                });
                if (pathExist)
                    EditorUtilsEx.GuiButtonAction("Remove Last", () => _Item.path.RemoveAt(lastPathIndex));
            });
            if (!pathExist)
                return;
            EditorUtilsEx.GuiButtonAction("△", () => 
                    _Item.path[lastPathIndex] = _Item.path[lastPathIndex].PlusY(1));
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction("◁", () => 
                    _Item.path[lastPathIndex] = _Item.path[lastPathIndex].MinusX(1));
                EditorUtilsEx.GuiButtonAction("▷", () => 
                    _Item.path[lastPathIndex] = _Item.path[lastPathIndex].PlusX(1));    
            });
            EditorUtilsEx.GuiButtonAction("▽", () => 
                _Item.path[lastPathIndex] = _Item.path[lastPathIndex].MinusY(1));
        }

        private void DrawControlsTrap(MazeProtItem _Item)
        {
            
        }
        
        private static void SetGUIColors(MazeItemType _A, MazeItemType _B)
        {
            if (_A == _B)
                SetGUIColorsIdle();
            else
                SetGUIColorsSelected();
        }
        
        private static void SetGUIColorsIdle()
        {
            GUI.color = Color.white;
            GUI.backgroundColor = Color.gray;
        }

        private static void SetGUIColorsSelected()
        {
            GUI.color = Color.white;
            GUI.backgroundColor = Color.gray;  
        }

        private static bool SelectTypeButtonPressed(MazeItemType _Type)
        {
            return GUILayout.Button(_Type.ToString().WithSpaces());
        }
    }
}