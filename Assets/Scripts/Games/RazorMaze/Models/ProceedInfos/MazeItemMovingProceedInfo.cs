using System.Collections.Generic;
using Entities;

namespace Games.RazorMaze.Models.ProceedInfos
{
    public class MazeItemMovingProceedInfo : MazeItemProceedInfoBase
    {
        public EMazeItemMoveByPathDirection MoveByPathDirection { get; set; }
        public List<V2Int> BusyPositions { get; set; }
    }
}