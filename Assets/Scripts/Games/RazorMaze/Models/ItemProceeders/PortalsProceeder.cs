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
    
    
    public class PortalsProceeder : IPortalsProceeder
    {
        #region nonpublic members

        private PortalEventArgs m_LastArgs;
        
        #endregion

        #region inject
        
        private IModelMazeData Data { get; }

        public PortalsProceeder(IModelMazeData _Data)
        {
            Data = _Data;
        }
        
        #endregion
        
        #region api
        
        public event PortalEventHandler PortalEvent;
        
        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            var infos = Data.ProceedInfos.Values
                .Where(_Info => _Info.Item.Type == EMazeItemType.Portal);
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
            var infos = _Info.MazeItems
                .Where(_Item => _Item.Type == EMazeItemType.Portal)
                .Select(_Item => new MazeItemPortalProceedInfo {Item = _Item});
            foreach (var info in infos)
            {
                if (Data.ProceedInfos.ContainsKey(info.Item))
                    Data.ProceedInfos[info.Item] = info;
                else
                    Data.ProceedInfos.Add(info.Item, info);
            }
        }
        
        #endregion
    }
}