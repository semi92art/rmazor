using System.Collections.Generic;
using System.Linq;
using Entities;
using Extensions;
using Games.RazorMaze.Models;
using UnityEngine;

namespace Games.RazorMaze.Prot
{
    public static class RazorMazePrototypingUtils
    {
        public static List<MazeProtItem> CreateMazeItems(MazeInfo _Info, Transform _Parent)
        {
            var res = new List<MazeProtItem>();
            var converter = new CoordinateConverter();
            converter.Init(_Info.Width);
            _Parent.SetPosXY(converter.GetCenter());
            foreach (var item in _Info.Path)
                AddPathItem(res, item, _Parent, _Info.Width);
            foreach (var item in _Info.MazeItems)
                AddMazeItem(res, item.Type, item.Position, item.Path, _Parent, _Info.Width);
            return res;
        }

        private static void AddPathItem(
            ICollection<MazeProtItem> _Items,
            V2Int _Position, 
            Transform _Parent, 
            int _Size)
        {
            bool isStartNode = _Items.All(_Item => !_Item.props.IsStartNode);
            var tr = new GameObject("Node").transform;
            tr.SetParent(_Parent);
            var item = tr.gameObject.AddComponent<MazeProtItem>();
            var props = new MazeProtItemProps
            {
                IsNode = true,
                IsStartNode = isStartNode,
                Size = _Size,
                Position = _Position
            };
            item.Init(props);
            _Items.Add(item);
        }

        private static void AddMazeItem(
            ICollection<MazeProtItem> _Items,
            EMazeItemType _Type,
            V2Int _Position,
            List<V2Int> _Path,
            Transform _Parent,
            int _Size)
        {
            var tr = new GameObject("Maze Item").transform;
            tr.SetParent(_Parent);
            var item = tr.gameObject.AddComponent<MazeProtItem>();
            var props = new MazeProtItemProps
            {
                Type = _Type,
                Size = _Size,
                Position = _Position,
                Path = _Path
            };
            item.Init(props);
            _Items.Add(item);
        }
    }
}