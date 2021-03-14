using System.Collections.Generic;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.Obstacles;

namespace Games.RazorMaze
{
    public class MazeInfo
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public List<Node> Nodes { get; set; } = new List<Node>();
        public List<WallBlock> WallBlocks { get; set; } = new List<WallBlock>();
        public List<MovingItem> MovingItems { get; set; } = new List<MovingItem>();
    }
}