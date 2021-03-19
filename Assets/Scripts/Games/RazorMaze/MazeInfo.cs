using System.Collections.Generic;
using Games.RazorMaze.Models;

namespace Games.RazorMaze
{
    public class MazeInfo
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public List<Node> Nodes { get; set; } = new List<Node>();
        public List<Obstacle> Obstacles { get; set; } = new List<Obstacle>();
    }
}