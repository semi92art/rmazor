using System.Collections.Generic;
using System.Linq;
using Entities;
using Games.RazorMaze.Models.ProceedInfos;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public interface IItemsProceeder
    {
        void Start();
        void Stop();
    }
    
    public abstract class ItemsProceederBase : IItemsProceeder
    {
        #region nonpublic members
        
        protected abstract EMazeItemType[] Types { get; }
        
        protected bool m_Proceeding;
        protected ModelSettings Settings { get; }
        protected IModelMazeData Data { get; }
        
        #endregion

        #region api

        public virtual void Start() => m_Proceeding = true;
        public virtual void Stop() => m_Proceeding = false;
        
        #endregion
        
        #region nonpublic methods
        
        protected ItemsProceederBase(ModelSettings _Settings, IModelMazeData _Data)
        {
            Settings = _Settings;
            Data = _Data;
        }

        protected void CollectItems(MazeInfo _Info)
        {
            var newInfos = _Info.MazeItems
                .Where(_Item => Types.Contains(_Item.Type))
                .Select(_Item => new MazeItemMovingProceedInfo
                {
                    Item = _Item,
                    MoveByPathDirection = EMazeItemMoveByPathDirection.Forward,
                    IsProceeding = false,
                    PauseTimer = 0,
                    BusyPositions = new List<V2Int>{_Item.Position},
                    ProceedingStage = 0
                });
            var infos = Data.ProceedInfos;
            foreach (var newInfo in newInfos)
            {
                if (!infos.ContainsKey(newInfo.Item.Type))
                    infos.Add(newInfo.Item.Type, new Dictionary<MazeItem, IMazeItemProceedInfo>());
                var dict = infos[newInfo.Item.Type];
                
                if (!dict.ContainsKey(newInfo.Item))
                    dict.Add(newInfo.Item, newInfo);
                else
                    dict[newInfo.Item] = newInfo;
            }
        }

        protected Dictionary<MazeItem, IMazeItemProceedInfo> GetProceedInfos(EMazeItemType _Type)
        {
            return Data.ProceedInfos[_Type];
        }
        
        #endregion
    }
}