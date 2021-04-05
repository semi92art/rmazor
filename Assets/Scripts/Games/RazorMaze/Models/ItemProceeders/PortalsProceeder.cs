using System;
using System.Linq;

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
                MazeItem portalItem = (from info in infos.Values where 
                        info.Item.Position == _Args.Current select info.Item)
                    .FirstOrDefault();

                if (portalItem == null)
                {
                    m_LastArgs = null;
                    return;
                }

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