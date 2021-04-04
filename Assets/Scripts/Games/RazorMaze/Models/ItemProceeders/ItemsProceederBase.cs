using System.Collections.Generic;
using System.Linq;
using Entities;
using Games.RazorMaze.Models.ProceedInfos;
using UnityGameLoopDI;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public abstract class ItemsProceederBase : Ticker
    {
        protected abstract EMazeItemType[] Types { get; }
        protected IModelMazeData Data { get; set; }

        protected ItemsProceederBase(IModelMazeData _Data) => Data = _Data;

        protected virtual void CollectItems(MazeInfo _Info)
        {
            var infos = _Info.MazeItems
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
            foreach (var info in infos)
            {
                if (Data.ProceedInfos.ContainsKey(info.Item))
                    Data.ProceedInfos[info.Item] = info;
                else
                    Data.ProceedInfos.Add(info.Item, info);
            }
        }
    }
}