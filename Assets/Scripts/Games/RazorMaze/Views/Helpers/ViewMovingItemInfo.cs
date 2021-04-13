using System.Collections.Generic;
using Entities;
using Shapes;
using UnityEngine;

namespace Games.RazorMaze.Views.Helpers
{
    public class ViewMovingItemInfo
    {
        public Vector2 From { get; set; }
        public Vector2 To { get; set; }
        public Dictionary<V2Int, Disc> BusyPositions { get; set; }
    }
}