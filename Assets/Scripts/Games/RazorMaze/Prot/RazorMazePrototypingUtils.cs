using System.Collections.Generic;
using Extensions;
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
            var nodes = _Info.Nodes;
            var wallBlocks = _Info.WallBlocks;

            int k = 0;
            foreach (var node in nodes)
            {
                var tr = new GameObject("Node").transform;
                tr.SetParent(_Parent);
                float scale = converter.GetScale();
                tr.position = converter.ToWorldPosition(node.Position);
                var item = tr.gameObject.AddComponent<MazeProtItem>();
                var props = new PrototypingItemProps
                {
                    Type = k == 0 ? PrototypingItemType.NodeStart : PrototypingItemType.Node,
                    Index = k++,
                    Scale = scale 
                };
                item.Init(props);
                res.Add(item);
            }
            foreach (var wallBlock in wallBlocks)
            {
                var tr = new GameObject("Wall").transform;
                tr.SetParent(_Parent);
                float scale = converter.GetScale();
                tr.position = converter.ToWorldPosition(wallBlock.Position);
                var item = tr.gameObject.AddComponent<MazeProtItem>();
                var props = new PrototypingItemProps
                {
                    Type = PrototypingItemType.WallBlockSimple,
                    Scale = scale
                };
                item.Init(props);
                res.Add(item);
            }
            return res;
        }
    }
}