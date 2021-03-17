using System.Collections.Generic;
using Extensions;
using UnityEngine;

namespace Games.RazorMaze.Prot
{
    [System.Serializable]
    public class MazeProtItems
    {
        public Transform Container;
        public int Width;
        public int Height;
        public List<MazeProtItem> items = new List<MazeProtItem>();

        public static MazeProtItems Create(
            MazeInfo _Info, 
            Transform _Parent,
            bool _ScaleToScreen = false)
        {
            var scaler = new MazeScreenScaler();
            //var contObj = new GameObject("Maze Items");
            //contObj.SetParent(_Parent);
            if (_ScaleToScreen)
            {
                var rb = _Parent.gameObject.AddComponent<Rigidbody2D>();
                _Parent.SetPosXY(scaler.GetCenter());
                rb.gravityScale = 0;
            }
            
            var result = new MazeProtItems
            {
                Width = _Info.Width, 
                Height = _Info.Height,
                Container = _Parent
            };
            var nodes = _Info.Nodes;
            var wallBlocks = _Info.WallBlocks;

            int k = 0;
            foreach (var node in nodes)
            {
                var go = new GameObject("Node");
                go.SetParent(_Parent);
                float scale = 1f;
                go.transform.position = _ScaleToScreen ? 
                    scaler.GetPosition(node.Position, _Info.Width, out scale) : 
                    node.Position.ToVector2();
                var item = go.AddComponent<MazeProtItem>();
                var props = new PrototypingItemProps
                {
                    Type = k == 0 ? PrototypingItemType.NodeStart : PrototypingItemType.Node,
                    Index = k++,
                    Scale = scale 
                };
                item.Init(props);
                result.items.Add(item);
            }
            foreach (var wallBlock in wallBlocks)
            {
                var go = new GameObject("Wall");
                go.SetParent(_Parent);
                float scale = 1f;
                go.transform.position = _ScaleToScreen ? 
                    scaler.GetPosition(wallBlock.Position, _Info.Width, out scale) :
                    wallBlock.Position.ToVector2();
                var item = go.AddComponent<MazeProtItem>();
                var props = new PrototypingItemProps
                {
                    Type = PrototypingItemType.WallBlockSimple,
                    Scale = scale
                };
                item.Init(props);
                result.items.Add(item);
            }
            return result;
        }
    }
}