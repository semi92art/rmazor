using System.Collections.Generic;
using Games.RazorMaze.Nodes;
using Games.RazorMaze.WallBlocks;

namespace Games.RazorMaze
{
    public class LevelInfo
    {
        public int Width { get; }
        public int Height { get; }
        public List<INode> Nodes { get; }
        public List<IWallBlock> WallBlocks { get; }

        public LevelInfo(int _Width, int _Height, List<INode> _Nodes, List<IWallBlock> _WallBlocks)
        {
            Width = _Width;
            Height = _Height;
            Nodes = _Nodes;
            WallBlocks = _WallBlocks;
        }
    }
}