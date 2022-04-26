using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common.Entities;
using Common.Exceptions;
using Common.Utils;
using RMAZOR;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.MazeItems;
using RMAZOR.Views.MazeItems.Props;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Editor
{
    [CustomEditor(typeof(ViewMazeItemProt)), CanEditMultipleObjects]
    public class ViewMazeItemProtEditor : UnityEditor.Editor
    {
        private ViewMazeItemProt[] m_TargetsCopy;
        private EMazeItemType m_Type;
        
        private void OnEnable()
        {
            m_TargetsCopy = targets.Cast<ViewMazeItemProt>().ToArray();
        }
        
        private void OnSceneGUI()
        {
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(20, 20, 200, 500));
            
            EditorGUILayout.BeginVertical();
            EditorUtilsEx.HorizontalZone(() =>
            {
                GUI.color = m_TargetsCopy.All(_Item => _Item.Props.IsNode) ? Color.white : Color.green;
                EditorUtilsEx.GuiButtonAction("Block", SetAsBlock, m_TargetsCopy);
                GUI.color = m_TargetsCopy.All(_Item => _Item.Props.IsNode) ? Color.green : Color.white;
                EditorUtilsEx.GuiButtonAction("Path", SetAsPathItem, m_TargetsCopy);
                if (m_TargetsCopy.Length != 1)
                    return;
                GUI.color = m_TargetsCopy[0].Props.Blank ? Color.green : Color.white;
                EditorUtilsEx.GuiButtonAction("Blank", SetAsBlankPathItem, m_TargetsCopy[0]);
                if (!m_TargetsCopy[0].Props.IsNode)
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
            switch (m_TargetsCopy.Length)
            {
                case 1:
                    DrawControlsForSingleBlock(m_TargetsCopy[0]);
                    break;
                case 2 when m_TargetsCopy
                    .All(_T => _T.Props.Type == EMazeItemType.Portal):
                    EditorUtilsEx.GuiButtonAction(LinkPortals, m_TargetsCopy[0].Props, m_TargetsCopy[1].Props);
                    break;
            }
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
            var prevStartNodes =
                LevelDesigner.Instance.maze
                    .Where(_Node => _Node.Props.IsNode && _Node.Props.IsStartNode);
            foreach (var node in prevStartNodes)
            {
                if (node != _Item)
                    node.SetType(node.Props.Type, true, false);
            }
            _Item.SetType(_Item.Props.Type, true, !_Item.Props.IsStartNode);
        }

        private static void SetAsBlankPathItem(ViewMazeItemProt _Item)
        {
            _Item.Props.Blank = !_Item.Props.Blank;
        }

        private static void DrawControlsForSingleBlock(ViewMazeItemProt _Item)
        {
            var props = _Item.Props;
            switch (props.Type)
            {
                case EMazeItemType.TrapMoving:
                case EMazeItemType.GravityBlock: DrawControlsMovingBlock(props);      break;
                case EMazeItemType.Turret:
                case EMazeItemType.TrapReact:    DrawControlsDirectedBlock(props);    break;
                case EMazeItemType.Springboard:  DrawControlsSpringboardBlock(_Item); break;
                case EMazeItemType.Hammer:       DrawControlsHammer(_Item);           break;
                case EMazeItemType.Bazooka:
                case EMazeItemType.Portal:
                case EMazeItemType.Block:
                case EMazeItemType.ShredingerBlock:
                case EMazeItemType.TrapIncreasing:
                case EMazeItemType.GravityTrap:
                case EMazeItemType.GravityBlockFree:
                    // do nothing
                    break;
                default: throw new SwitchCaseNotImplementedException(props.Type);
            }
        }
        
        private static void DrawControlsMovingBlock(ViewMazeItemProps _Props)
        {
            EditorUtilsEx.GUIColorZone(Color.black,() => GUILayout.Label("Path:"));
            bool pathExist = _Props.Path.Any();
            int lastPathIndex = _Props.Path.Count - 1;
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction("Add", () =>
                {
                    EditorUtilsEx.SceneDirtyAction(() =>
                    {
                        var pointToAdd = !pathExist ? _Props.Position : _Props.Path.Last();
                        _Props.Path.Add(pointToAdd);    
                    });
                });
                if (pathExist)
                    EditorUtilsEx.GuiButtonAction("Remove Last", () =>
                        EditorUtilsEx.SceneDirtyAction(() => _Props.Path.RemoveAt(lastPathIndex)));
            });
            if (!pathExist)
                return;
            V2Int GetLp()         => _Props.Path[lastPathIndex];
            void SetLp(V2Int _V) => EditorUtilsEx.SceneDirtyAction(() => _Props.Path[lastPathIndex] = _V);
            EditorUtilsEx.GuiButtonAction("△", () => SetLp(GetLp().PlusY(1)));
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction("◁", () => SetLp(GetLp().MinusX(1)));
                EditorUtilsEx.GuiButtonAction("▷", () => SetLp(GetLp().PlusX(1)));    
            });
            EditorUtilsEx.GuiButtonAction("▽", () => SetLp(GetLp().MinusY(1)));
        }

        private static void DrawControlsDirectedBlock(ViewMazeItemProps _Props)
        {
            EditorUtilsEx.GUIColorZone(Color.black,() => GUILayout.Label("Direction:"));
            if (_Props.Type == EMazeItemType.Turret)
                DrawControlsDirectedBlockSingle(_Props);
            else DrawControlsDirectedBlockMultiple(_Props);
        }

        private static void DrawControlsSpringboardBlock(ViewMazeItemProt _Item)
        {
            var props = _Item.Props;
            var dirs = props.Directions;
            var possibleDirs = new[]
            {
                V2Int.Up   + V2Int.Right,
                V2Int.Down + V2Int.Right,
                V2Int.Down + V2Int.Left,
                V2Int.Up   + V2Int.Left
            };
            if (!dirs.Any() || !possibleDirs.Contains(dirs.First()))
            {
                EditorUtilsEx.SceneDirtyAction(() => { _Item.SetDirection(V2Int.Up + V2Int.Left);});
                return;
            }
            void RotateSpringboard()
            {
                var dir = dirs.First();
                if (dir == V2Int.Up + V2Int.Left)         dir = V2Int.Up + V2Int.Right;
                else if (dir == V2Int.Up + V2Int.Right)   dir = V2Int.Down + V2Int.Right;
                else if (dir == V2Int.Down + V2Int.Right) dir = V2Int.Down + V2Int.Left;
                else if (dir == V2Int.Down + V2Int.Left)  dir = V2Int.Up + V2Int.Left;
                _Item.SetDirection(dir);
            }
            void RotateSpringboardDirty() => EditorUtilsEx.SceneDirtyAction(RotateSpringboard);
            EditorUtilsEx.GuiButtonAction("Rotate", RotateSpringboardDirty);
        }

        private static void DrawControlsHammer(ViewMazeItemProt _Item)
        {
            var props = _Item.Props;
            var dirs = props.Directions;
            var possibleDirs = new []
            {
                V2Int.Up,
                V2Int.Right,
                V2Int.Down,
                V2Int.Left
            };
            if (!dirs.Any() || !possibleDirs.Contains(dirs.First()))
            {
                EditorUtilsEx.SceneDirtyAction(() => { _Item.SetDirection(V2Int.Up);});
                return;
            }
            void RotateHammer()
            {
                var dir = dirs.First();
                if (dir == V2Int.Up)         dir = V2Int.Right;
                else if (dir == V2Int.Right) dir = V2Int.Down;
                else if (dir == V2Int.Down)  dir = V2Int.Left;
                else if (dir == V2Int.Left)  dir = V2Int.Up;
                _Item.SetDirection(dir);
            }
            void RotateHammerDirty() => EditorUtilsEx.SceneDirtyAction(RotateHammer);
            EditorUtilsEx.GuiButtonAction("Rotate", RotateHammerDirty);
            if (_Item.Props.Args.Count < 2)
            {
                _Item.Props.Args.Clear();
                _Item.Props.Args.Add("angle:90");
                _Item.Props.Args.Add("clockwise:true");
            }
            void SetRotationAngle(int _Angle)
            {
                _Item.Props.Args[0] = "angle:" + _Angle.ToString(CultureInfo.InvariantCulture);
            }
            void SetRotationAngleDirty(int _Angle) => EditorUtilsEx.SceneDirtyAction(() => SetRotationAngle(_Angle));
            void SetClockwise(bool _Clockwise)
            {
                _Item.Props.Args[1] = "clockwise:" + (_Clockwise ? "true" : "false");
            }
            void SetClockwiseDirty(bool _Clockwise) => EditorUtilsEx.SceneDirtyAction(() => SetClockwise(_Clockwise));
            int rotAng = int.Parse(_Item.Props.Args[0].Split(':')[1]);
            bool clockwise = _Item.Props.Args[1].Split(':')[1] == "true";
            EditorUtilsEx.HorizontalZone(() =>
            {
                Color GetCol(int _Angle) => rotAng == _Angle ? Color.green : Color.white;
                var angles = new [] {90, 180, 270};
                foreach (int ang in angles)
                {
                    EditorUtilsEx.GUIColorZone(GetCol(ang), () =>
                    {
                        EditorUtilsEx.GuiButtonAction(ang + "°", () => SetRotationAngleDirty(ang));
                    });
                }
            });
            EditorUtilsEx.HorizontalZone(() =>
            {
                static Color GetCol(bool _Predicate) => _Predicate ? Color.green : Color.white;
                EditorUtilsEx.GUIColorZone(GetCol(clockwise), () =>
                {
                    EditorUtilsEx.GuiButtonAction("clockwise", () => SetClockwiseDirty(true));
                });
                EditorUtilsEx.GUIColorZone(GetCol(!clockwise), () =>
                {
                    EditorUtilsEx.GuiButtonAction("counter", () => SetClockwiseDirty(false));
                });
            });
        }

        private static void DrawControlsDirectedBlockSingle(ViewMazeItemProps _Props)
        {
            void  SetDir(IEnumerable<V2Int> _Directions)
            {
                EditorUtilsEx.SceneDirtyAction(() => _Props.Directions = _Directions.ToList());
            }
            Color GetCol(V2Int              _Direction)
            {
                return _Props.Directions.Contains(_Direction) ? Color.green : Color.white;
            }
            EditorUtilsEx.GUIColorZone(
                GetCol(V2Int.Up), 
                () => EditorUtilsEx.GuiButtonAction(
                    "△", (UnityAction<IEnumerable<V2Int>>) SetDir, new []{V2Int.Up}));
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GUIColorZone(
                    GetCol(V2Int.Left), 
                    () => EditorUtilsEx.GuiButtonAction(
                        "◁", (UnityAction<IEnumerable<V2Int>>) SetDir, new []{V2Int.Left}));
                EditorUtilsEx.GUIColorZone(
                    GetCol(V2Int.Right), 
                    () => EditorUtilsEx.GuiButtonAction(
                        "▷", (UnityAction<IEnumerable<V2Int>>) SetDir, new []{V2Int.Right}));
            });
            EditorUtilsEx.GUIColorZone(
                GetCol(V2Int.Down), 
                () => EditorUtilsEx.GuiButtonAction(
                    "▽", (UnityAction<IEnumerable<V2Int>>) SetDir, new []{V2Int.Down}));
        }

        private static void DrawControlsDirectedBlockMultiple(ViewMazeItemProps _Props)
        {
            UnityAction<V2Int> setDir = _Direction =>
            {
                EditorUtilsEx.SceneDirtyAction(() =>
                {
                    if (_Props.Directions.Contains(V2Int.Zero))
                        _Props.Directions.Remove(V2Int.Zero);
                    if (_Props.Directions.Contains(_Direction))
                        _Props.Directions.Remove(_Direction);
                    else _Props.Directions.Add(_Direction);
                });
            };
            Func<V2Int, Color> getCol = _Direction => _Props.Directions.Contains(_Direction) ? Color.green : Color.white;
            EditorUtilsEx.GUIColorZone(getCol(V2Int.Up), () => EditorUtilsEx.GuiButtonAction("△", setDir, V2Int.Up));
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GUIColorZone(getCol(V2Int.Left), () => EditorUtilsEx.GuiButtonAction("◁", setDir, V2Int.Left));
                EditorUtilsEx.GUIColorZone(getCol(V2Int.Right), () => EditorUtilsEx.GuiButtonAction("▷", setDir, V2Int.Right));
            });
            EditorUtilsEx.GUIColorZone(getCol(V2Int.Down), () => EditorUtilsEx.GuiButtonAction("▽", setDir, V2Int.Down));
        }

        private static void LinkPortals(ViewMazeItemProps _Props1, ViewMazeItemProps _Props2)
        {
            EditorUtilsEx.SceneDirtyAction(() =>
            {
                _Props1.Pair = _Props2.Position;
                _Props2.Pair = _Props1.Position;    
            });
        }

        private EMazeItemType MazeItemsPopup(Rect _Rect, EMazeItemType _ItemType)
        {
            var itemSymbolsDict = new Dictionary<EMazeItemType, char>
            {
                {EMazeItemType.Block,            '\u20DE'},
                {EMazeItemType.GravityBlock,     '\u23FA'},
                {EMazeItemType.GravityBlockFree, '\u2601'},
                {EMazeItemType.ShredingerBlock,  '\u25A8'},
                {EMazeItemType.Springboard,      '\u22CC'},
                {EMazeItemType.Portal,           '\u058D'},
                {EMazeItemType.TrapMoving,       '\u2618'},
                {EMazeItemType.TrapReact,        '\u234B'},
                {EMazeItemType.TrapIncreasing,   '\u2602'},
                {EMazeItemType.Turret,           '\u260F'},
                {EMazeItemType.GravityTrap,      '\u2622'},
                {EMazeItemType.Hammer,           '\u2692'},
                {EMazeItemType.Bazooka,         '\u22c8'}
            };

            var keys = itemSymbolsDict.Keys.ToList();
            int idx = keys.IndexOf(_ItemType);
            var popupContent = itemSymbolsDict
                .Select(_Kvp => $"{_Kvp.Key} {_Kvp.Value}")
                .ToArray();
            int newIdx = EditorGUI.Popup(_Rect, idx, popupContent);
            return keys[newIdx];
        }
    }
}