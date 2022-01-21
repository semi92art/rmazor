using System;
using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Ticker;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;
using UnityEngine;

namespace RMAZOR.Models.ItemProceeders
{
    public class PortalEventArgs : EventArgs
    {
        public EMazeMoveDirection   Direction  { get; }
        public IMazeItemProceedInfo Info       { get; }
        public bool                 IsPortFrom { get; }

        public PortalEventArgs(EMazeMoveDirection _Direction, IMazeItemProceedInfo _Info, bool _IsPortFrom)
        {
            Direction = _Direction;
            Info = _Info;
            IsPortFrom = _IsPortFrom;
        }
    }

    public delegate void PortalEventHandler(PortalEventArgs _Args);

    public interface IPortalsProceeder : IItemsProceeder, ICharacterMoveStarted, ICharacterMoveContinued
    {
        event PortalEventHandler PortalEvent;
    }
    
    
    public class PortalsProceeder : ItemsProceederBase, IPortalsProceeder
    {
        #region nonpublic members

        private PortalEventArgs m_LastArgs;
        private List<V2Int>     m_CurrentFullPath;

        #endregion

        #region inject
        
        public PortalsProceeder(
            ModelSettings _Settings,
            IModelData _Data, 
            IModelCharacter _Character,
            IModelGameTicker _GameTicker) 
            : base(_Settings, _Data, _Character, _GameTicker) { }
        
        #endregion
        
        #region api
        
        public override EMazeItemType[] Types => new[] {EMazeItemType.Portal};
        public event PortalEventHandler PortalEvent;
        
        public void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args)
        {
            m_CurrentFullPath = RazorMazeUtils.GetFullPath(_Args.From, _Args.To);
        }
        
        public void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args)
        {
            var possiblePortals = new List<IMazeItemProceedInfo>();
            for (int i = 0; i < ProceedInfos.Length; i++)
            {
                var info = ProceedInfos[i];
                if (!m_CurrentFullPath.Contains(info.CurrentPosition))
                    continue;
                if (RazorMazeUtils.CompareItemsOnPath(
                    m_CurrentFullPath, _Args.Position, info.CurrentPosition) < 0)
                    continue;
                possiblePortals.Add(info);
            }
            IMazeItemProceedInfo portalItem = null;
            if (possiblePortals.Count == 1)
                portalItem = possiblePortals.First();
            else if (possiblePortals.Count > 1)
            {
                float distToStart = float.MaxValue;
                for (int i = 0; i < possiblePortals.Count; i++)
                {
                    var possiblePortal = possiblePortals[i];
                    float newDistToStart = Vector2.Distance(_Args.From, possiblePortal.CurrentPosition);
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
            var v = (_Args.To - _Args.From).Normalized;
            Vector2 a = portalItem.CurrentPosition;
            var b =
                _Args.From
                + v * _Args.Progress * ((Vector2)(_Args.To - _Args.From)).magnitude;
            var c = v * (a - b);
            float m = c.x + c.y;
            if (m > 0f)
                return;
            if (m_LastArgs != null)
                return;
            m_LastArgs = new PortalEventArgs(
                _Args.Direction,
                portalItem, 
                _Args.Position != _Args.From);
            PortalEvent?.Invoke(m_LastArgs);
        }

        #endregion
    }
}