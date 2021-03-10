using System.Collections.Generic;
using System.Linq;
using Extensions;
using UnityEngine;

namespace Games.RazorMaze.Prot
{
    [System.Serializable]
    public class LevelProt
    {
        public int Width;
        public int Height;
        public List<ProtItem> items = new List<ProtItem>();

        public static LevelProt Create(
            LevelInfo _Info, 
            Transform _Parent)
        {
            var result = new LevelProt {Width = _Info.Width, Height = _Info.Height};
            var nodes = _Info.Nodes;
            var wallBlocks = _Info.WallBlocks;

            int k = 0;
            foreach (var node in nodes)
            {
                var go = new GameObject("Node");
                go.SetParent(_Parent);
                go.transform.position = (Vector2)node.Position;
                var item = go.AddComponent<ProtItem>();
                var props = new PrototypingItemProps
                {
                    Type = k == 0 ? PrototypingItemType.NodeStart : PrototypingItemType.Node,
                    Index = k++
                };
                item.Init(props);
                result.items.Add(item);
            }
            foreach (var wallBlock in wallBlocks)
            {
                var go = new GameObject("Wall");
                go.SetParent(_Parent);
                go.transform.position = (Vector2)wallBlock.Position;
                var item = go.AddComponent<ProtItem>();
                var props = new PrototypingItemProps {Type = PrototypingItemType.WallBlockSimple};
                item.Init(props);
                result.items.Add(item);
            }
            return result;
        }
    }
}