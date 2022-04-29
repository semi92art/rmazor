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
    public partial class ViewMazeItemProtEditor
    {
        private static void SetBlank(IEnumerable<ViewMazeItemProt> _ProtsCollection)
        {
            var protsList = _ProtsCollection.ToList();
            bool allBlank = protsList.All(_P => _P.Props.Blank);
            bool allNotBlank = protsList.All(_P => !_P.Props.Blank);
            bool allSame = allBlank || allNotBlank;
            bool newBlankValue;
            if (allSame)
            {
                GUI.color = allBlank ? Color.green : Color.white;
                newBlankValue = !allBlank;
            }
            else
            {
                GUI.color = Color.yellow;
                newBlankValue = true;
            }
            EditorUtilsEx.GuiButtonAction("Blank", () =>
            {
                foreach (var props in protsList.Select(_P => _P.Props))
                    props.Blank = newBlankValue;
            });
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

        private static EMazeItemType MazeItemsPopup(Rect _Rect, EMazeItemType _ItemType)
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
                {EMazeItemType.Bazooka,          '\u22c8'}
            };

            var keys = itemSymbolsDict.Keys.ToList();
            int idx = keys.IndexOf(_ItemType);
            var popupContent = itemSymbolsDict
                .Select(_Kvp => $"{_Kvp.Key} {_Kvp.Value}")
                .ToArray();
            int newIdx = EditorGUI.Popup(_Rect, idx, popupContent);
            return keys[newIdx];
        }
        
        private static void DrawControlsForBlocksCollection(ViewMazeItemProt[] _Items)
        {
            switch (_Items[0].Props.Type)
            {
                case EMazeItemType.TrapMoving:   DrawControlsMovingTraps(_Items);   break;
                
                case EMazeItemType.GravityBlock: DrawControlsGravityBlocks(_Items); break;
                case EMazeItemType.Turret:       DrawControlsTurrets(_Items);       break;
                case EMazeItemType.TrapReact:    DrawControlsTrapsReact(_Items);    break;
                case EMazeItemType.Springboard:  DrawControlsSpringboards(_Items);  break;
                case EMazeItemType.Hammer:       DrawControlsHammers(_Items);       break;
                case EMazeItemType.Portal:       DrawControlsPortals(_Items);       break;
                case EMazeItemType.Bazooka:
                case EMazeItemType.Block:
                case EMazeItemType.ShredingerBlock:
                case EMazeItemType.TrapIncreasing:
                case EMazeItemType.GravityTrap:
                case EMazeItemType.GravityBlockFree:
                    // do nothing
                    break;
                default: throw new SwitchCaseNotImplementedException(_Items[0].Props.Type);
            }
        }

        private static void DrawControlsMovingTraps(IEnumerable<ViewMazeItemProt> _Items)
        {
            var propsArray = _Items.Select(_Item => _Item.Props).ToArray();
            DrawControlsMovingBlock(propsArray);
        }
        
        private static void DrawControlsGravityBlocks(IEnumerable<ViewMazeItemProt> _Items)
        {
            var propsArray = _Items.Select(_Item => _Item.Props).ToArray();
            DrawControlsMovingBlock(propsArray);
        }

        private static void DrawControlsPortals(IReadOnlyList<ViewMazeItemProt> _Items)
        {
            if (_Items.Count != 2)
                return;
            EditorUtilsEx.GuiButtonAction(LinkPortals, _Items[0].Props, _Items[1].Props);
        }

        private static void DrawControlsTrapsReact(IEnumerable<ViewMazeItemProt> _Items)
        {
            var propsArray = _Items.Select(_Item => _Item.Props).ToArray();
            DrawControlsDirectedBlock(propsArray);
        }
        
        private static void DrawControlsTurrets(IEnumerable<ViewMazeItemProt> _Items)
        {
            var propsArray = _Items.Select(_Item => _Item.Props).ToArray();
            DrawControlsDirectedBlock(propsArray);
        }
        
        private static void DrawControlsMovingBlock(IReadOnlyCollection<ViewMazeItemProps> _PropsList)
        {
            if (_PropsList.Count != 1)
                return;
            var props = _PropsList.First();
            GUILayout.Label("Path:", GetHeader1Style());
            bool pathExist = props.Path.Any();
            int lastPathIndex = props.Path.Count - 1;
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction("Add", () =>
                {
                    EditorUtilsEx.SceneDirtyAction(() =>
                    {
                        var pointToAdd = !pathExist ? props.Position : props.Path.Last();
                        props.Path.Add(pointToAdd);    
                    });
                });
                if (pathExist)
                    EditorUtilsEx.GuiButtonAction("Remove Last", () =>
                        EditorUtilsEx.SceneDirtyAction(() => props.Path.RemoveAt(lastPathIndex)));
            });
            if (!pathExist)
                return;
            V2Int GetLp()         => props.Path[lastPathIndex];
            void  SetLp(V2Int _V) => EditorUtilsEx.SceneDirtyAction(() => props.Path[lastPathIndex] = _V);
            EditorUtilsEx.GuiButtonAction("△", () => SetLp(GetLp().PlusY(1)));
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction("◁", () => SetLp(GetLp().MinusX(1)));
                EditorUtilsEx.GuiButtonAction("▷", () => SetLp(GetLp().PlusX(1)));    
            });
            EditorUtilsEx.GuiButtonAction("▽", () => SetLp(GetLp().MinusY(1)));
        }

        private static void DrawControlsDirectedBlock(IReadOnlyCollection<ViewMazeItemProps> _PropsList)
        {
            GUILayout.Label("Direction:", GetHeader1Style());
            var propsFirst = _PropsList.First();
            if (propsFirst.Type == EMazeItemType.Turret)
                DrawControlsDirectedBlockSingle(_PropsList);
            else DrawControlsDirectedBlockMultiple(_PropsList);
        }

        private static void DrawControlsSpringboards(ViewMazeItemProt[] _Items)
        {
            var protsList = _Items.ToList();
            var dirs = _Items.First().Props.Directions;
            var possibleDirs = new[]
            {
                V2Int.Up   + V2Int.Right,
                V2Int.Down + V2Int.Right,
                V2Int.Down + V2Int.Left,
                V2Int.Up   + V2Int.Left
            };
            void SetDirs(V2Int _Dir) => protsList.ForEach(_P => _P.SetDirection(_Dir));
            if (!dirs.Any() || !possibleDirs.Contains(dirs.First()))
            {
                EditorUtilsEx.SceneDirtyAction(() => SetDirs(V2Int.Up + V2Int.Left));
                return;
            }
            void RotateSpringboard()
            {
                var dir = dirs.First();
                if (dir == V2Int.Up + V2Int.Left)         dir = V2Int.Up + V2Int.Right;
                else if (dir == V2Int.Up + V2Int.Right)   dir = V2Int.Down + V2Int.Right;
                else if (dir == V2Int.Down + V2Int.Right) dir = V2Int.Down + V2Int.Left;
                else if (dir == V2Int.Down + V2Int.Left)  dir = V2Int.Up + V2Int.Left;
                SetDirs(dir);
            }
            void RotateSpringboardDirty() => EditorUtilsEx.SceneDirtyAction(RotateSpringboard);
            EditorUtilsEx.GuiButtonAction("Rotate", RotateSpringboardDirty);
        }

        private static void DrawControlsHammers(IReadOnlyCollection<ViewMazeItemProt> _ProtsCollection)
        {
            if (_ProtsCollection.Count != 1)
                return;
            var protsList = _ProtsCollection.ToList();
            var firstProps = protsList.First().Props;
            var dirs = firstProps.Directions;
            var possibleDirs = new []
            {
                V2Int.Up,
                V2Int.Right,
                V2Int.Down,
                V2Int.Left
            };
            void SetDirs(V2Int _Dir) => protsList.ForEach(_P => _P.SetDirection(_Dir));
            if (!dirs.Any() || !possibleDirs.Contains(dirs.First()))
            {
                EditorUtilsEx.SceneDirtyAction(() => SetDirs(V2Int.Up));
                return;
            }
            void RotateHammer()
            {
                var dir = dirs.First();
                if (dir == V2Int.Up)         dir = V2Int.Right;
                else if (dir == V2Int.Right) dir = V2Int.Down;
                else if (dir == V2Int.Down)  dir = V2Int.Left;
                else if (dir == V2Int.Left)  dir = V2Int.Up;
                SetDirs(dir);
            }
            void RotateHammerDirty() => EditorUtilsEx.SceneDirtyAction(RotateHammer);
            EditorUtilsEx.GuiButtonAction("Rotate", RotateHammerDirty);
            if (_ProtsCollection.First().Props.Args.Count < 2)
                foreach (var props in _ProtsCollection.Select(_P => _P.Props))
                {
                    props.Args.Clear();
                    props.Args.Add("angle:90");
                    props.Args.Add("clockwise:true");
                }
            void SetRotationAngle(int _Angle)
            {
                foreach (var props in _ProtsCollection.Select(_P => _P.Props))
                    props.Args[0] = "angle:" + _Angle.ToString(CultureInfo.InvariantCulture);
            }
            void SetRotationAngleDirty(int _Angle) => EditorUtilsEx.SceneDirtyAction(() => SetRotationAngle(_Angle));
            void SetClockwise(bool _Clockwise)
            {
                foreach (var props in _ProtsCollection.Select(_P => _P.Props))
                    props.Args[1] = "clockwise:" + (_Clockwise ? "true" : "false");
            }
            void SetClockwiseDirty(bool _Clockwise) => EditorUtilsEx.SceneDirtyAction(() => SetClockwise(_Clockwise));
            int rotAng = int.Parse(firstProps.Args[0].Split(':')[1]);
            bool clockwise = firstProps.Args[1].Split(':')[1] == "true";
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

        private static void DrawControlsDirectedBlockSingle(IReadOnlyCollection<ViewMazeItemProps> _PropsList)
        {
            var propsFirst = _PropsList.First();
            void  SetDir(IEnumerable<V2Int> _Directions)
            {
                var dirs = _Directions.ToList();
                EditorUtilsEx.SceneDirtyAction(() =>
                {
                    foreach (var props in _PropsList)
                        props.Directions = dirs;
                });
            }
            Color GetCol(V2Int _Direction)
            {
                return propsFirst.Directions.Contains(_Direction) ? Color.green : Color.white;
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

        private static void DrawControlsDirectedBlockMultiple(IReadOnlyCollection<ViewMazeItemProps> _PropsList)
        {
            var firstProps = _PropsList.First();
            void SetDir(V2Int _Direction)
            {
                EditorUtilsEx.SceneDirtyAction(() =>
                {
                    foreach (var props in _PropsList)
                    {
                        if (props.Directions.Contains(V2Int.Zero))
                            props.Directions.Remove(V2Int.Zero);
                        if (props.Directions.Contains(_Direction))
                            props.Directions.Remove(_Direction);
                        else
                            props.Directions.Add(_Direction);
                    }
                });
            }
            Color GetCol(V2Int _Direction)
            {
                return firstProps.Directions.Contains(_Direction) ? Color.green : Color.white;
            }
            EditorUtilsEx.GUIColorZone(GetCol(V2Int.Up), () => EditorUtilsEx.GuiButtonAction("△", SetDir, V2Int.Up));
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GUIColorZone(GetCol(V2Int.Left), () => EditorUtilsEx.GuiButtonAction("◁", SetDir, V2Int.Left));
                EditorUtilsEx.GUIColorZone(GetCol(V2Int.Right), () => EditorUtilsEx.GuiButtonAction("▷", SetDir, V2Int.Right));
            });
            EditorUtilsEx.GUIColorZone(GetCol(V2Int.Down), () => EditorUtilsEx.GuiButtonAction("▽", SetDir, V2Int.Down));
        }

        private static void LinkPortals(ViewMazeItemProps _Props1, ViewMazeItemProps _Props2)
        {
            EditorUtilsEx.SceneDirtyAction(() =>
            {
                _Props1.Pair = _Props2.Position;
                _Props2.Pair = _Props1.Position;    
            });
        }
    }
}