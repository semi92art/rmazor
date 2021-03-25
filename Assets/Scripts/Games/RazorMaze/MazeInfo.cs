using System.Collections.Generic;
using Entities;
using Games.RazorMaze.Models;

namespace Games.RazorMaze
{
    public class MazeInfo
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public List<V2Int> Path { get; set; } = new List<V2Int>();
        public List<MazeItem> MazeItems { get; set; } = new List<MazeItem>();
    }
}