using System;
using System.Collections.Generic;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;
using System.Linq;
using Common.Entities;
using Common.Extensions;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Ticker;

namespace RMAZOR.Models.ItemProceeders
{
    public class KeyLockEventArgs : EventArgs
    {
        public Dictionary<IMazeItemProceedInfo, IList<IMazeItemProceedInfo>> KeysAndLocksDictionary { get; }
        
        public KeyLockEventArgs(Dictionary<IMazeItemProceedInfo, IList<IMazeItemProceedInfo>> _KeysAndLocksDictionary)
        {
            KeysAndLocksDictionary = _KeysAndLocksDictionary;
        }
    }

    public delegate void KeyLockPairEventHandler(KeyLockEventArgs _Args);

    public interface IKeyLockMazeItemsProceeder : 
        IItemsProceeder, 
        ICharacterMoveFinished,
        IGetAllProceedInfos
    {
        event KeyLockPairEventHandler KeyLockPairEventHandler;
    }
    
    public class KeyLockMazeItemsProceeder : ItemsProceederBase, IKeyLockMazeItemsProceeder
    {
        #region inject

        private KeyLockMazeItemsProceeder(
            ModelSettings      _Settings,
            IModelData         _Data,
            IModelCharacter    _Character,
            IModelGameTicker   _GameTicker,
            IModelMazeRotation _Rotation) 
            : base(
                _Settings,
                _Data, 
                _Character, 
                _GameTicker,
                _Rotation) { }

        #endregion

        #region api

        protected override EMazeItemType[] Types => new[] {EMazeItemType.KeyLock};
        
        public event KeyLockPairEventHandler KeyLockPairEventHandler;
        
        public Func<IMazeItemProceedInfo[]> GetAllProceedInfos { get; set; }

        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            var characterPath = RmazorUtils.GetFullPath(_Args.From, _Args.To);
            var keysAndLocksDict = new Dictionary<IMazeItemProceedInfo, IList<IMazeItemProceedInfo>>();
            foreach (var info in ProceedInfos)
            {
                if (!characterPath.Contains(info.CurrentPosition))
                    continue;
                if (info.CurrentPosition == characterPath.First())
                    continue;
                if (IsLock(info))
                    continue;
                var keyItemInfo = info;
                if (!keysAndLocksDict.ContainsKey(keyItemInfo))
                    keysAndLocksDict.SetSafe(keyItemInfo, new List<IMazeItemProceedInfo>());
                foreach (var lockPairItemPosition in keyItemInfo.Path)
                {
                    if (lockPairItemPosition == keyItemInfo.StartPosition)
                        continue;
                    var lockItem = ProceedInfos.FirstOrDefault(
                        _Info => _Info.StartPosition == lockPairItemPosition);
                    if (lockItem != null)
                        keysAndLocksDict[keyItemInfo].Add(lockItem);
                }
            }
            if (!keysAndLocksDict.Any())
                return;
            foreach (var (key, locks) in keysAndLocksDict)
            {
                key.ProceedingStage = key.ProceedingStage == ModelCommonData.KeyLockStage1
                    ? ModelCommonData.KeyLockStage2
                    : ModelCommonData.KeyLockStage1;
                foreach (var @lock in locks)
                {
                    @lock.ProceedingStage = @lock.ProceedingStage == ModelCommonData.KeyLockStage1
                        ? ModelCommonData.KeyLockStage2
                        : ModelCommonData.KeyLockStage1;
                }
            }
            var args = new KeyLockEventArgs(keysAndLocksDict);
            KeyLockPairEventHandler?.Invoke(args);
        }

        #endregion

        #region nonpublic methods

        private static bool IsLock(IMazeItemProceedInfo _ProceedInfo)
        {
            return _ProceedInfo.Direction == V2Int.Right;
        }
        
        #endregion
    }
}