using System;
using System.Linq;
using Games.RazorMaze.Models.ProceedInfos;
using Ticker;
using UnityEngine;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public class PortalEventArgs : EventArgs
    {
        public EMazeMoveDirection Direction { get; }
        public IMazeItemProceedInfo Info { get; }

        public PortalEventArgs(EMazeMoveDirection _Direction, IMazeItemProceedInfo _Info)
        {
            Direction = _Direction;
            Info = _Info;
        }
    }

    public delegate void PortalEventHandler(PortalEventArgs _Args);

    public interface IPortalsProceeder : IItemsProceeder, ICharacterMoveContinued
    {
        event PortalEventHandler PortalEvent;
    }
    
    
    public class PortalsProceeder : ItemsProceederBase, IPortalsProceeder
    {
        #region nonpublic members

        private PortalEventArgs m_LastArgs;

        #endregion

        #region inject
        
        public PortalsProceeder(
            ModelSettings _Settings,
            IModelData _Data, 
            IModelCharacter _Character,
            IGameTicker _GameTicker) 
            : base(_Settings, _Data, _Character, _GameTicker) { }
        
        #endregion
        
        #region api
        
        public override EMazeItemType[] Types => new[] {EMazeItemType.Portal};
        public event PortalEventHandler PortalEvent;
        
        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            var possiblePortals = (from info in GetProceedInfos(Types)
                    where RazorMazeUtils.PathContainsItem(_Args.From, _Args.To, info.CurrentPosition) 
                          && RazorMazeUtils.CompareItemsOnPath(
                        _Args.From, _Args.To, _Args.Position, info.CurrentPosition) >= 0
                    select info)
                .ToList();
            IMazeItemProceedInfo portalItem = null;

            if (possiblePortals.Count == 1)
                portalItem = possiblePortals.First();
            else if (possiblePortals.Count > 1)
            {
                float distToStart = float.MaxValue;
                foreach (var possiblePortal in possiblePortals)
                {
                    float newDistToStart = Vector2.Distance(
                        _Args.From.ToVector2(), possiblePortal.CurrentPosition.ToVector2());
                    if (newDistToStart > distToStart)
                        continue;
                    portalItem = possiblePortal;
                    distToStart = newDistToStart;
                }
            }

            if (portalItem == null)
            {
                m_LastArgs = null;
                return;
            }
            
            var V = (_Args.To - _Args.From).Normalized;
            var A = portalItem.CurrentPosition.ToVector2();
            var B = _Args.From.ToVector2() + V * _Args.Progress * (_Args.To - _Args.From).ToVector2().magnitude;
            var C = V * (A - B);
            var m = C.x + C.y;
            if (m > 0f)
                return;

            if (m_LastArgs != null)
                return;

            m_LastArgs = new PortalEventArgs(_Args.Direction, portalItem);
            PortalEvent?.Invoke(m_LastArgs);
        }

        #endregion
    }
}