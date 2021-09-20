using System;
using System.Linq;
using Entities;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public class SpringboardEventArgs : EventArgs
    {
        public EMazeMoveDirection Direction { get; }
        public MazeItem Item { get; }

        public SpringboardEventArgs(EMazeMoveDirection _Direction, MazeItem _Item)
        {
            Direction = _Direction;
            Item = _Item;
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
        protected override EMazeItemType[] Types => new[] {EMazeItemType.Springboard};
        
        #endregion
        
        #region inject
        
        public SpringboardProceeder(ModelSettings _Settings, IModelMazeData _Data, IModelCharacter _Character)
            : base (_Settings, _Data, _Character) { }
        
        #endregion
        
        #region api
        
        public event SpringboardEventHandler SpringboardEvent;

        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            var infos = GetProceedInfos(Types).Values;
            var item = (from info in infos where info.Item.Position == _Args.Position select info.Item)
                .FirstOrDefault();

            if (item == null)
            {
                m_LastArgs = null;
                return;
            }

            if (m_LastArgs != null)
                return;
            
            var charInverseDir = -RazorMazeUtils.GetDirectionVector(_Args.Direction, Data.Orientation);
            V2Int newDirection = default;
            if (item.Direction == V2Int.up + V2Int.left)
                newDirection = charInverseDir == V2Int.up ? V2Int.left : V2Int.up;
            else if (item.Direction == V2Int.up + V2Int.right)
                newDirection = charInverseDir == V2Int.up ? V2Int.right : V2Int.up;
            else if (item.Direction == V2Int.down + V2Int.left)
                newDirection = charInverseDir == V2Int.down ? V2Int.left : V2Int.down;
            else if (item.Direction == V2Int.down + V2Int.right)
                newDirection = charInverseDir == V2Int.down ? V2Int.right : V2Int.down;
            var moveDirection = RazorMazeUtils.GetMoveDirection(newDirection, Data.Orientation);
            
            m_LastArgs = new SpringboardEventArgs(moveDirection, item);
            SpringboardEvent?.Invoke(m_LastArgs);
        }
        
        #endregion
    }
}