using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using Common.Extensions;
using mazing.common.Runtime;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Extensions;
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
            IViewMazeItem _ProtItem,
            bool          _RightOrUp,
            bool          _Remove,
            bool          _Row)
        {
            var protItems = LevelDesigner.Instance.maze;
            var pos = _ProtItem.Props.Position;
            var mazeInfo = LevelDesigner.Instance.GetLevelInfoFromScene();
            var mazeItemsToAdd = new List<MazeItem>();
            if (!_Remove)
                mazeItemsToAdd = CreateMazeItemsToAdd(mazeInfo, pos, _Row, _RightOrUp);
            else
                protItems = DestroyMazeItemsToRemove(protItems, mazeInfo, pos, _Row);

            V2Int GetAddict()
            {
                if (_Remove) 
                    return _Row ? V2Int.Down : V2Int.Left;
                return _Row ? V2Int.Up : V2Int.Right;
            }
            var addict = GetAddict();
            bool MustBeShifted(V2Int _Pos)
            {
                if (_Remove)
                    return _Row ? _Pos.Y > pos.Y : _Pos.X > pos.X;
                return _Row switch
                {
                    true  => _RightOrUp ? _Pos.Y > pos.Y : _Pos.Y > pos.Y - 1,
                    false => _RightOrUp ? _Pos.X > pos.X : _Pos.X > pos.X - 1
                };
            }
            protItems
                .ToList()
                .ForEach(_P =>
                {
                    if (MustBeShifted(_P.Props.Position))
                        _P.Props.Position += addict;
                    var path = _P.Props.Path;
                    for (int i = 0; i < path.Count; i++)
                    {
                        if (MustBeShifted(path[i]))
                            path[i] += addict;
                    }
                    if (MustBeShifted(_P.Props.Pair) && _P.Props.Type == EMazeItemType.Portal)
                        _P.Props.Pair += addict;
                });
            
            var mazeInfo1 = LevelDesigner.Instance.GetLevelInfoFromScene();
            if (!_Remove)
            {
                mazeInfo1.MazeItems.AddRange(mazeItemsToAdd);
                mazeInfo1.Size = new V2Int(
                    Math.Max(mazeInfo1.Size.X, mazeItemsToAdd.Max(_I => _I.Position.X) + 1),
                    Math.Max(mazeInfo1.Size.Y, mazeItemsToAdd.Max(_I => _I.Position.Y) + 1));
            }
            Dbg.Log("New maze size: " + mazeInfo1.Size);
            LevelDesignerEditorWindow.Instance.LoadLevel(mazeInfo1);
        }

        private static List<MazeItem> CreateMazeItemsToAdd(MazeInfo _Info, V2Int _Position, bool _Row, bool _RightOrUp)
        {
            var mazeItemsToAdd = new List<MazeItem>();
            if (_Row)
                for (int i = 0; i < _Info.Size.X; i++)
                {
                    var mazeItem = new MazeItem
                    {
                        Position = new V2Int(i, _Position.Y + (_RightOrUp ? 1 : 0)),
                        Type = EMazeItemType.Block
                    };
                    mazeItemsToAdd.Add(mazeItem);
                }
            else
                for (int i = 0; i < _Info.Size.Y; i++)
                {
                    var mazeItem = new MazeItem
                    {
                        Position = new V2Int(_Position.X + (_RightOrUp ? 1 : 0), i),
                        Type = EMazeItemType.Block
                    };
                    mazeItemsToAdd.Add(mazeItem);
                }
            return mazeItemsToAdd;
        }

        private static List<ViewMazeItemProt> DestroyMazeItemsToRemove(
            List<ViewMazeItemProt> _ProtItems,
            MazeInfo               _Info,
            V2Int                  _Position,
            bool                   _Row)
        {
            var protItemsToRemove = new List<ViewMazeItemProt>();
            if (_Row)
                for (int i = 0; i < _Info.Size.X; i++)
                {
                    var pos1 = new V2Int(i, _Position.Y);
                    protItemsToRemove.AddRange(_ProtItems.Where(_I => _I.Props.Position == pos1));
                }
            else
                for (int i = 0; i < LevelDesigner.Instance.size.Y; i++)
                {
                    var pos1 = new V2Int(_Position.X, i);
                    protItemsToRemove.AddRange(_ProtItems.Where(_I => _I.Props.Position == pos1));
                }
            if (protItemsToRemove.Any())
            {
                _ProtItems.RemoveRange(protItemsToRemove);
                foreach (var protItem in protItemsToRemove)
                    DestroyImmediate(protItem.gameObject);
            }
            return _ProtItems;
        }
    }
}