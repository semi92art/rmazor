using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using RMAZOR.Models.MazeInfos;

namespace RMAZOR.Models.ProceedInfos
{
    public enum EMoveByPathDirection
    {
        Forward, 
        Backward
    }
    
    public interface IMazeItemProceedInfo
    {
        EMazeItemType        Type                { get; }
        V2Int                StartPosition       { get; }
        List<V2Int>          Path                { get; }
        V2Int                Pair                { get; }
        V2Int                Direction           { get; }
        bool                 IsProceeding        { get; set; }
        int                  ProceedingStage     { get; set; }
        bool                 ReadyToSwitchStage  { get; set; }
        float                PauseTimer          { get; set; }
        EMoveByPathDirection MoveByPathDirection { get; set; }
        List<V2Int>          BusyPositions       { get; set; }
        bool                 IsMoving            { get; set; }
        V2Int                CurrentPosition     { get; set; }
        V2Int                NextPosition        { get; set; }
        
        void SetItem(MazeItem _Item);
    }
    
    public class MazeItemProceedInfo : IMazeItemProceedInfo
    {
        public EMazeItemType        Type                { get; private set; }
        public V2Int                StartPosition       { get; private set; }
        public List<V2Int>          Path                { get; private set; }
        public V2Int                Pair                { get; private set; }
        public V2Int                Direction           { get; private set; }
        public bool                 IsProceeding        { get; set; }
        public int                  ProceedingStage     { get; set; }
        public bool                 ReadyToSwitchStage  { get; set; }
        public float                PauseTimer          { get; set; }
        public EMoveByPathDirection MoveByPathDirection { get; set; }
        public List<V2Int>          BusyPositions       { get; set; }
        public bool                 IsMoving            { get; set; }
        public V2Int                CurrentPosition     { get; set; }
        public V2Int                NextPosition        { get; set; }

        public void SetItem(MazeItem _Item)
        {
            Type = _Item.Type;
            StartPosition = CurrentPosition = _Item.Position;
            Path = _Item.Path.ToList();
            Pair = _Item.Pair;
            Direction = _Item.Directions.First();
        }
    }
}