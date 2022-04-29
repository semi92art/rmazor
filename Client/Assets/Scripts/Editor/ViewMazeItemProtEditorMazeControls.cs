using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Extensions;
using RMAZOR;
using RMAZOR.Editor;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.MazeItems;

namespace Editor
{
    public partial class ViewMazeItemProtEditor
    {
        private static void AddRow(ViewMazeItemProt _ProtItem, bool _Up)
        {
            AddOrRemoveRowOrColumn(_ProtItem, _Up, false, true);
        }

        private static void RemoveRow(ViewMazeItemProt _ProtItem, bool _Up)
        {
            AddOrRemoveRowOrColumn(_ProtItem, _Up, true, true);
        }

        private static void AddColumn(ViewMazeItemProt _ProtItem, bool _Right)
        {
            AddOrRemoveRowOrColumn(_ProtItem, _Right, false, false);
        }
        
        private static void RemoveColumn(ViewMazeItemProt _ProtItem, bool _Right)
        {
            AddOrRemoveRowOrColumn(_ProtItem, _Right, true, false);
        }

        private static void AddOrRemoveRowOrColumn(
            ViewMazeItemProt _ProtItem,
            bool             _RightOrUp,
            bool             _Remove,
            bool             _Row)
        {
            var protItems = LevelDesigner.Instance.maze;
            var addict = _Row switch
            {
                true  => _RightOrUp ^ _Remove ? V2Int.Up : V2Int.Down,
                false => _RightOrUp ^ _Remove ? V2Int.Right : V2Int.Left
            };
            var pos = _ProtItem.Props.Position;
            bool MustBeShifted(V2Int _Pos)
            {
                return _Row switch
                {
                    true  => _RightOrUp ? _Pos.Y > pos.Y : _Pos.Y < pos.Y,
                    false => _RightOrUp ? _Pos.X > pos.X : _Pos.X < pos.X
                };
            }
            var mazeInfo = LevelDesigner.Instance.GetLevelInfoFromScene();
            var protItemsToRemove = new List<ViewMazeItemProt>();
            var mazeItemsToAdd = new List<MazeItem>();
            if (_Remove)
            {
                if (_Row)
                    for (int i = 0; i < mazeInfo.Size.X; i++)
                    {
                        var pos1 = new V2Int(i, pos.Y);
                        protItemsToRemove.AddRange(protItems.Where(_I => _I.Props.Position == pos1));
                    }
                else
                    for (int i = 0; i < LevelDesigner.Instance.size.Y; i++)
                    {
                        var pos1 = new V2Int(pos.X, i);
                        protItemsToRemove.AddRange(protItems.Where(_I => _I.Props.Position == pos1));
                    }
            }
            else
            {
                if (_Row)
                    for (int i = 0; i < mazeInfo.Size.X; i++)
                    {
                        var mazeItem = new MazeItem
                        {
                            Position = new V2Int(i, pos.Y + (_RightOrUp ? 1 : -1)),
                            Type = EMazeItemType.Block
                        };
                        mazeItemsToAdd.Add(mazeItem);
                    }
                else
                    for (int i = 0; i < mazeInfo.Size.Y; i++)
                    {
                        var mazeItem = new MazeItem
                        {
                            Position = new V2Int(pos.X + (_RightOrUp ? 1 : -1), i),
                            Type = EMazeItemType.Block
                        };
                        mazeItemsToAdd.Add(mazeItem);
                    }
            }

            if (protItemsToRemove.Any())
            {
                protItems.RemoveRange(protItemsToRemove);
                foreach (var protItem in protItemsToRemove)
                    DestroyImmediate(protItem.gameObject);
            }

            protItems
                .Where(_P => MustBeShifted(_P.Props.Position))
                .ToList()
                .ForEach(_P =>
                {
                    _P.Props.Position += addict;
                    var path = _P.Props.Path;
                    for (int i = 0; i < path.Count; i++)
                    {
                        if (MustBeShifted(path[i]))
                            path[i] += addict;
                    }
                    if (MustBeShifted(_P.Props.Pair))
                        _P.Props.Pair += addict;
                });
            
            mazeInfo = LevelDesigner.Instance.GetLevelInfoFromScene();

            if (_Remove)
            {
                mazeInfo.Size -= _Row ? V2Int.Up : V2Int.Right;
                foreach (var mazeItem in mazeInfo.MazeItems)
                {
                    mazeItem.Position -= _Row ? V2Int.Up : V2Int.Right;
                    for (int i = 0; i < mazeItem.Path.Count; i++)
                        mazeItem.Path[i] -= _Row ? V2Int.Up : V2Int.Right;
                    mazeItem.Pair -= _Row ? V2Int.Up : V2Int.Right;
                }
                foreach (var pathItem in mazeInfo.PathItems)
                    pathItem.Position -= _Row ? V2Int.Up : V2Int.Right;
            }
            else
            {
                mazeInfo.MazeItems.AddRange(mazeItemsToAdd);
                if (!_RightOrUp)
                {
                    mazeInfo.Size += _Row ? V2Int.Up : V2Int.Right;
                    foreach (var mazeItem in mazeInfo.MazeItems)
                    {
                        mazeItem.Position += _Row ? V2Int.Up : V2Int.Right;
                        for (int i = 0; i < mazeItem.Path.Count; i++)
                            mazeItem.Path[i] += _Row ? V2Int.Up : V2Int.Right;
                        mazeItem.Pair += _Row ? V2Int.Up : V2Int.Right;
                    }
                    foreach (var pathItem in mazeInfo.PathItems)
                        pathItem.Position += _Row ? V2Int.Up : V2Int.Right;
                }
            }
            
            LevelDesignerEditor.Instance.LoadLevel(mazeInfo);
        }
    }
}