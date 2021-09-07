using System;
using System.Linq;
using UnityEngine;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public class PortalEventArgs : EventArgs
    {
        public MazeItem Item { get; }

        public PortalEventArgs(MazeItem _Item)
        {
            Item = _Item;
        }
    }

    public delegate void PortalEventHandler(PortalEventArgs _Args);

    public interface IPortalsProceeder : IOnMazeChanged, ICharacterMoveContinued
    {
        event PortalEventHandler PortalEvent;
    }
    
    
    public class PortalsProceeder : ItemsProceederBase, IPortalsProceeder
    {
        #region nonpublic members

        private PortalEventArgs m_LastArgs;
        protected override EMazeItemType[] Types => new[] {EMazeItemType.Portal};
        
        #endregion

        #region inject
        
        public PortalsProceeder(ModelSettings _Settings, IModelMazeData _Data) 
            : base(_Settings, _Data) { }
        
        #endregion
        
        #region api
        
        public event PortalEventHandler PortalEvent;
        
        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            foreach (var type in Types)
            {
                var infos = GetProceedInfos(type);
                var possiblePortals = (from info in infos.Values
                        where RazorMazeUtils.PathContainsItem(_Args.From, _Args.To, info.Item.Position) 
                              && RazorMazeUtils.CompareItemsOnPath(
                            _Args.From, _Args.To, _Args.Current, info.Item.Position) >= 0
                        select info.Item)
                    .ToList();
                MazeItem portalItem = null;

                if (possiblePortals.Count == 1)
                    portalItem = possiblePortals.First();
                else if (possiblePortals.Count > 1)
                {
                    float distToStart = float.MaxValue;
                    foreach (var possiblePortal in possiblePortals)
                    {
                        float newDistToStart = Vector2.Distance(
                            _Args.From.ToVector2(), possiblePortal.Position.ToVector2());
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
                var A = portalItem.Position.ToVector2();
                var B = _Args.From.ToVector2() + V * _Args.Progress * (_Args.To - _Args.From).ToVector2().magnitude;
                var C = V * (A - B);
                var m = C.x + C.y;
                if (m > 0f)
                    return;

                if (m_LastArgs != null)
                    return;

                m_LastArgs = new PortalEventArgs(portalItem);
                PortalEvent?.Invoke(m_LastArgs);
            }
        }

        public void OnMazeChanged(MazeInfo _Info)
        {
            CollectItems(_Info);
        }
        
        #endregion
    }
}