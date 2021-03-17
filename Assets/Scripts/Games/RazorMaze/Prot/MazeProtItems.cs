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
            var contObj = new GameObject("Maze Items");
            contObj.SetParent(_Parent);
            if (_ScaleToScreen)
            {
                var rb = contObj.AddComponent<Rigidbody2D>();
                contObj.transform.SetPosXY(scaler.GetCenter(_Info.Width));
                rb.gravityScale = 0;    
            }
            
            var result = new MazeProtItems
            {
                Width = _Info.Width, 
                Height = _Info.Height,
                Container = contObj.transform
            };
            var nodes = _Info.Nodes;
            var wallBlocks = _Info.WallBlocks;

            int k = 0;
            foreach (var node in nodes)
            {
                var go = new GameObject("Node");
                go.SetParent(contObj);
                float scale = 1f;
                go.transform.position = _ScaleToScreen ? 
                    scaler.GetWorldPosition(node.Position, _Info.Width, out scale) : 
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
                go.SetParent(contObj);
                float scale = 1f;
                go.transform.position = _ScaleToScreen ? 
                    scaler.GetWorldPosition(wallBlock.Position, _Info.Width, out scale) :
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