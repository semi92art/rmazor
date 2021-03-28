using System.Collections.Generic;
using Entities;

namespace Games.RazorMaze.Models
{
    public class MazeInfo
    {
        public V2Int Size { get; set; }
        public List<V2Int> Path { get; set; } = new List<V2Int>();
        public List<MazeItem> MazeItems { get; set; } = new List<MazeItem>();
        public int LevelGroup { get; set; }
        public int LevelIndex { get; set; }
    }
}