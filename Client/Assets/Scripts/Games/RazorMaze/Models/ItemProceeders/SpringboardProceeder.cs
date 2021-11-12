using System;
using System.Linq;
using Entities;
using Games.RazorMaze.Models.ProceedInfos;
using Ticker;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public class SpringboardEventArgs : EventArgs
    {
        public EMazeMoveDirection Direction { get; }
        public IMazeItemProceedInfo Info { get; }

        public SpringboardEventArgs(EMazeMoveDirection _Direction, IMazeItemProceedInfo _Info)
        {
            Direction = _Direction;
            Info = _Info;
        }
    }

    public delegate void SpringboardEventHandler(SpringboardEventArgs _Args);

    public interface ISpringboardProceeder : IItemsProceeder, ICharacterMoveContinued
    {
        event SpringboardEventHandler SpringboardEvent;
    }
    
    public class SpringboardProceeder : ItemsProceederBase, ISpringboardProceeder
    {
        #region nonpbulic members

        private SpringboardEventArgs m_LastArgs;

        #endregion
        
        #region inject
        
        public SpringboardProceeder(
            ModelSettings _Settings,
            IModelData _Data,
            IModelCharacter _Character,
            IModelLevelStaging _LevelStaging,
            IGameTicker _Ticker)
            : base (_Settings, _Data, _Character, _LevelStaging, _Ticker) { }
        
        #endregion
        
        #region api
        
        public override EMazeItemType[] Types => new[] {EMazeItemType.Springboard};
        public event SpringboardEventHandler SpringboardEvent;

        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            var info = (from inf in ProceedInfos
                    where inf.CurrentPosition == _Args.Position select inf)
                .FirstOrDefault();

            if (info == null)
            {
                m_LastArgs = null;
                return;
            }

            if (m_LastArgs != null)
                return;
            
            var charInverseDir = -RazorMazeUtils.GetDirectionVector(_Args.Direction, Data.Orientation);
            V2Int newDirection = default;
            if (info.Direction == V2Int.up + V2Int.left)
                newDirection = charInverseDir == V2Int.up ? V2Int.left : V2Int.up;
            else if (info.Direction == V2Int.up + V2Int.right)
                newDirection = charInverseDir == V2Int.up ? V2Int.right : V2Int.up;
            else if (info.Direction == V2Int.down + V2Int.left)
                newDirection = charInverseDir == V2Int.down ? V2Int.left : V2Int.down;
            else if (info.Direction == V2Int.down + V2Int.right)
                newDirection = charInverseDir == V2Int.down ? V2Int.right : V2Int.down;
            var moveDirection = RazorMazeUtils.GetMoveDirection(newDirection, Data.Orientation);
            
            m_LastArgs = new SpringboardEventArgs(moveDirection, info);
            SpringboardEvent?.Invoke(m_LastArgs);
        }
        
        #endregion
    }
}