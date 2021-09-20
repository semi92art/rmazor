using System.Collections.Generic;
using Entities;

namespace Games.RazorMaze.Models.ProceedInfos
{
    public interface IMazeItemProceedInfo
    {
        MazeItem Item { get; set; }
        bool IsProceeding { get; set; }
        int ProceedingStage { get; set; }
        bool ReadyToSwitchStage { get; set; }
        float PauseTimer { get; set; }
        EMazeItemMoveByPathDirection MoveByPathDirection { get; set; }
        List<V2Int> BusyPositions { get; set; }
        bool IsMoving { get; set; }
    }
    
    public class MazeItemProceedInfo : IMazeItemProceedInfo
    {
        public MazeItem Item { get; set; }
        public bool IsProceeding { get; set; }
        public int ProceedingStage { get; set; }
        public bool ReadyToSwitchStage { get; set; }
        public float PauseTimer { get; set; }
        public EMazeItemMoveByPathDirection MoveByPathDirection { get; set; }
        public List<V2Int> BusyPositions { get; set; }
        public bool IsMoving { get; set; }
    }
}