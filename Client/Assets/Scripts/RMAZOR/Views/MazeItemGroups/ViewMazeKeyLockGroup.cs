using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Extensions;
using mazing.common.Runtime;
using mazing.common.Runtime.Extensions;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;
using RMAZOR.Views.Common;
using RMAZOR.Views.MazeItems;

namespace RMAZOR.Views.MazeItemGroups
{
    public interface IViewMazeKeyLockGroup
        : IInit,
          IViewMazeItemGroup,
          ICharacterMoveFinished
    {
        void OnKeyLock(KeyLockEventArgs _Args);
    }
    
    public class ViewMazeKeyLockGroup : ViewMazeItemsGroupBase, IViewMazeKeyLockGroup
    {
        public ViewMazeKeyLockGroup(IViewMazeCommon _Common) : base(_Common) { }

        public override IEnumerable<EMazeItemType> Types => new[] {EMazeItemType.KeyLock};

        public void OnKeyLock(KeyLockEventArgs _Args)
        {
            var locksAndKeysDict = new Dictionary<IMazeItemProceedInfo, IList<IMazeItemProceedInfo>>(); 
            foreach (var (key, locks) in _Args.KeysAndLocksDictionary)
            {
                foreach (var @lock in locks)
                {
                    if (!locksAndKeysDict.ContainsKey(@lock))
                        locksAndKeysDict.SetSafe(@lock, new List<IMazeItemProceedInfo>());
                    locksAndKeysDict[@lock].Add(key);
                }
            }
            foreach (var (@lock, keys) in locksAndKeysDict.ToList())
                locksAndKeysDict[@lock] = keys.Distinct().ToList();
            foreach (var (@lock, keys) in locksAndKeysDict.ToList())
            {
                if (keys.Count % 2 == 0)
                    continue;
                var viewMazeItemLock = Common.GetItem<IViewMazeItemKeyLock>(@lock);
                viewMazeItemLock.OnKeyLockStateChanged(@lock, true);
            }
            foreach (var key in locksAndKeysDict
                .ToList()
                .SelectMany(_Kvp => _Kvp.Value)
                .Distinct())
            {
                var viewMazeItemKey = Common.GetItem<IViewMazeItemKeyLock>(key);
                viewMazeItemKey.OnKeyLockStateChanged(key, false);
            }
        }

        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            if (_Args.BlockWhoStopped == null || _Args.BlockWhoStopped.Type != EMazeItemType.KeyLock)
                return;
            GetItems(true)
                .Cast<IViewMazeItemKeyLock>()
                .Single(_Item => _Item.Props.Position == _Args.BlockWhoStopped.StartPosition)
                .OnCharacterMoveFinished(_Args);
        }
    }
}