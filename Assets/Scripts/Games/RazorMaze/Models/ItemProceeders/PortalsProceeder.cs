using System;
using System.Linq;
using Games.RazorMaze.Models.ProceedInfos;

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

    public interface IPortalsProceeder : IOnMazeChanged
    {
        event PortalEventHandler PortalEvent;
        void OnCharacterMoveContinued(CharacterMovingEventArgs _Args);
    }
    
    
    public class PortalsProceeder : ItemsProceederBase, IPortalsProceeder
    {
        #region nonpublic members

        private PortalEventArgs m_LastArgs;
        
        #endregion

        #region inject

        protected override EMazeItemType[] Types => new[] {EMazeItemType.Portal};

        public PortalsProceeder(IModelMazeData _Data) : base(_Data) { }
        
        #endregion
        
        #region api
        
        public event PortalEventHandler PortalEvent;
        
        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            var infos = Data.ProceedInfos.Values
                .Where(_Info => Types.Contains(_Info.Item.Type));
            MazeItem portalItem = (from info in infos where 
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

        public void OnMazeChanged(MazeInfo _Info)
        {
            CollectItems(_Info);
        }
        
        #endregion
    }
}