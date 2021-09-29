using System.Collections.Generic;
using System.Linq;
using Entities;

namespace Games.RazorMaze.Models.ProceedInfos
{
    public interface IMazeItemProceedInfo
    {
        EMazeItemType Type { get; }
        V2Int StartPosition { get; }
        List<V2Int> Path { get; }
        V2Int Pair { get; }
        V2Int Direction { get; }
        
        bool IsProceeding { get; set; }
        int ProceedingStage { get; set; }
        bool ReadyToSwitchStage { get; set; }
        float PauseTimer { get; set; }
        EMazeItemMoveByPathDirection MoveByPathDirection { get; set; }
        List<V2Int> BusyPositions { get; set; }
        bool IsMoving { get; set; }
        V2Int CurrentPosition { get; set; }
        
        void SetItem(MazeItem _Item);
    }
    
    public class MazeItemProceedInfo : IMazeItemProceedInfo
    {
        private MazeItem m_Item;

        public EMazeItemType Type => m_Item.Type;
        public V2Int StartPosition => m_Item.Position;
        public List<V2Int> Path => m_Item.Path.ToList();
        public V2Int Pair => m_Item.Pair;
        public V2Int Direction => m_Item.Direction;


        public bool IsProceeding { get; set; }
        public int ProceedingStage { get; set; }
        public bool ReadyToSwitchStage { get; set; }
        public float PauseTimer { get; set; }
        public EMazeItemMoveByPathDirection MoveByPathDirection { get; set; }
        public List<V2Int> BusyPositions { get; set; }
        public bool IsMoving { get; set; }
        public V2Int CurrentPosition { get; set; }
        
        public void SetItem(MazeItem _Item)
        {
            m_Item = _Item;
            CurrentPosition = StartPosition;
        }
    }
}