using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Exceptions;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;
using RMAZOR.Views;

namespace RMAZOR.Models.ItemProceeders
{
    public interface IItemsProceeder : IOnLevelStageChanged
    {
        IMazeItemProceedInfo[] ProceedInfos { get; }
    }

    public abstract class ItemsProceederBase : IItemsProceeder
    {
        #region types

        private class ViewMazeItemCoroutines
        {
            public  IEnumerator[] Coroutines { get; private set; }
            private int           m_Count;

            public ViewMazeItemCoroutines()
            {
                ClearArray();
            }

            public void AddCoroutine(IEnumerator _Coroutine)
            {
                m_Count = MathUtils.ClampInverse(m_Count + 1, 0, 499);
                Coroutines[m_Count] = _Coroutine;
            }

            public void ClearArray()
            {
                Coroutines = new IEnumerator[500];
                m_Count = 0;
            }
        }

        #endregion
        
        #region nonpublic members
        
        private readonly Dictionary<IMazeItemProceedInfo, ViewMazeItemCoroutines> m_CoroutinesDict =
            new Dictionary<IMazeItemProceedInfo, ViewMazeItemCoroutines>();

        protected virtual bool StopProceedOnLevelFinish => true;
        
        #endregion

        #region inject
        
        protected ModelSettings      Settings     { get; }
        protected IModelData         Data         { get; }
        protected IModelCharacter    Character    { get; }
        protected IModelGameTicker   GameTicker   { get; }
        
        protected ItemsProceederBase(
            ModelSettings _Settings, 
            IModelData _Data,
            IModelCharacter _Character,
            IModelGameTicker _GameTicker)
        {
            Settings = _Settings;
            Data = _Data;
            Character = _Character;
            GameTicker = _GameTicker;
            GameTicker.Register(this);
        }

        #endregion

        #region api

        protected abstract EMazeItemType[] Types { get; }
        public IMazeItemProceedInfo[] ProceedInfos { get; private set; } = new IMazeItemProceedInfo[0];

        public virtual void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                    CollectItems(Data.Info);
                    ClearCoroutines();
                    break;
                case ELevelStage.ReadyToStart when _Args.PreviousStage == ELevelStage.Loaded:
                    StartProceed();
                    break;
                case ELevelStage.ReadyToStart when _Args.PreviousStage == ELevelStage.Paused:
                case ELevelStage.StartedOrContinued:
                    ContinueProceed();
                    break;
                case ELevelStage.Paused:
                    PauseProceed();
                    break;
                case ELevelStage.Finished when _Args.PreviousStage != ELevelStage.Paused && StopProceedOnLevelFinish:
                    ClearCoroutines();
                    break;
                case ELevelStage.ReadyToStart when _Args.PreviousStage == ELevelStage.CharacterKilled:
                case ELevelStage.Finished:
                case ELevelStage.ReadyToUnloadLevel: 
                case ELevelStage.Unloaded:
                case ELevelStage.CharacterKilled:
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Args.Stage);
            }
        }
        
        #endregion
        
        #region nonpublic methods
        
        protected void CollectItems(MazeInfo _Info)
        {
            var infos = new List<IMazeItemProceedInfo>();

            var newInfos = _Info.MazeItems
                .Where(_Item => Types.Contains(_Item.Type))
                .Select(_Item =>
                {
                    IMazeItemProceedInfo res = new MazeItemProceedInfo
                    {
                        MoveByPathDirection = EMoveByPathDirection.Forward,
                        IsProceeding = false,
                        PauseTimer = 0,
                        BusyPositions = new List<V2Int> {_Item.Position},
                        ProceedingStage = 0,
                        CurrentPosition = _Item.Position,
                        NextPosition = _Item.Position
                    };
                    res.SetItem(_Item);
                    return res;
                });
            foreach (var newInfo in newInfos)
            {
                m_CoroutinesDict.Add(newInfo, new ViewMazeItemCoroutines());
                if (!infos.Contains(newInfo))
                    infos.Add(newInfo);
            }
            ProceedInfos = infos.ToArray();
        }
        
        private void StartProceed()
        {
            foreach (var info in ProceedInfos)
            {
                info.IsProceeding = true;
                info.ReadyToSwitchStage = true;
                info.CurrentPosition = info.StartPosition;
                info.ProceedingStage = ModelCommonData.StageIdle;
            }
        }
        
        private void ContinueProceed()
        {
            foreach (var info in ProceedInfos)
                info.IsProceeding = true;
        }

        private void PauseProceed()
        {
            foreach (var info in ProceedInfos)
                info.IsProceeding = false;
        }

        protected void ProceedCoroutine(IMazeItemProceedInfo _ProceedInfo, IEnumerator _Coroutine)
        {
            m_CoroutinesDict[_ProceedInfo].AddCoroutine(_Coroutine);
            Cor.Run(_Coroutine);
        }
        
        private void ClearCoroutines()
        {
            foreach (var coroutines in m_CoroutinesDict
                .Select(_Kvp => _Kvp.Value))
            {
                foreach (var coroutine in coroutines.Coroutines)
                    Cor.Stop(coroutine);
                coroutines.ClearArray();
            }
        }
        
        protected static bool PathContainsItem(V2Int _From, V2Int _To, V2Int _Point)
        {
            var fullPath = RmazorUtils.GetFullPath(_From, _To);
            return fullPath.Contains(_Point);
        }
        
        #endregion
    }
}