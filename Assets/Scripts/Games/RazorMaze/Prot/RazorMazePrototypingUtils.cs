using System.Collections.Generic;
using System.Linq;
using Entities;
using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.MazeItems;
using UnityEngine;

namespace Games.RazorMaze.Prot
{
    public static class RazorMazePrototypingUtils
    {
        public static List<IViewMazeItem> CreateMazeItems(MazeInfo _Info, Transform _Parent)
        {
            var res = new List<IViewMazeItem>();
            var converter = new CoordinateConverter();
            converter.Init(_Info.Size);
            _Parent.SetPosXY(converter.GetCenter());
            foreach (var item in _Info.Path)
                AddPathItem(res, item, _Parent, _Info.Size);
            foreach (var item in _Info.MazeItems)
                AddMazeItem(res, item.Type, item.Position, item.Path, _Parent, _Info.Size);
            return res;
        }

        private static void AddPathItem(
            ICollection<IViewMazeItem> _Items,
            V2Int _Position, 
            Transform _Parent, 
            V2Int _Size)
        {
            bool isStartNode = _Items.All(_Item => !_Item.Props.IsStartNode);
            var tr = new GameObject("Node").transform;
            tr.SetParent(_Parent);
            var item = tr.gameObject.AddComponent<ViewMazeItemProt>();
            var props = new ViewMazeItemProps
            {
                IsNode = true,
                IsStartNode = isStartNode,
                Position = _Position
            };
            item.Init(props, _Size);
            _Items.Add(item);
        }

        private static void AddMazeItem(
            ICollection<IViewMazeItem> _Items,
            EMazeItemType _Type,
            V2Int _Position,
            List<V2Int> _Path,
            Transform _Parent,
            V2Int _Size)
        {
            var tr = new GameObject("Maze Item").transform;
            tr.SetParent(_Parent);
            var item = tr.gameObject.AddComponent<ViewMazeItemProt>();
            var props = new ViewMazeItemProps
            {
                Type = _Type,
                Position = _Position,
                Path = _Path
            };
            item.Init(props, _Size);
            _Items.Add(item);
        }
    }
}