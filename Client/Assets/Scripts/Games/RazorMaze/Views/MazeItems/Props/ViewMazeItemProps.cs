using System;
using System.Collections.Generic;
using Entities;
using Games.RazorMaze.Models;

namespace Games.RazorMaze.Views.MazeItems.Props
{
    [Serializable]
    public class ViewMazeItemProps
    {
        public bool IsNode;
        public bool IsStartNode;
        public EMazeItemType Type;
        public V2Int Position;
        public List<V2Int> Path = new List<V2Int>();
        public List<V2Int> Directions = new List<V2Int>{V2Int.zero};
        public V2Int Pair;
    }
}