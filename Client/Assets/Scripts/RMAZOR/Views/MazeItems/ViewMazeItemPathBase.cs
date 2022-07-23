using System.Linq;
using Common;
using Common.Entities;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;
using RMAZOR.Views.Common.ViewMazeMoneyItems;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.Views.MazeItems
{
    public interface IViewMazeItemPath : 
        IViewMazeItem,
        ICharacterMoveStarted, 
        ICharacterMoveContinued
    {
        event UnityAction MoneyItemCollected;
        void              Collect(bool _Collect, bool _OnStart = false);
    }
    
    public abstract class ViewMazeItemPathBase : ViewMazeItemBase, IViewMazeItemPath
    {
        #region inject
        
        protected IViewMazeMoneyItem MoneyItem { get; }

        protected ViewMazeItemPathBase(
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            ICoordinateConverter        _CoordinateConverter,
            IContainersGetter           _ContainersGetter,
            IViewGameTicker             _GameTicker,
            IRendererAppearTransitioner _Transitioner,
            IManagersGetter             _Managers,
            IColorProvider              _ColorProvider,
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewMazeMoneyItem          _MoneyItem)
            : base(
                _ViewSettings,
                _Model,
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker,
                _Transitioner,
                _Managers,
                _ColorProvider,
                _CommandsProceeder)
        {
            MoneyItem = _MoneyItem;
        }

        #endregion

        #region api
        
        public event UnityAction MoneyItemCollected;
        
        public override bool ActivatedInSpawnPool
        {
            get => base.ActivatedInSpawnPool;
            set
            {
                m_ActivatedInSpawnPool = value;
                EnableInitializedShapes(false);
            }
        }

        public abstract void Collect(bool _Collect, bool _OnStart = false);
        public abstract void OnCharacterMoveStarted(CharacterMovingStartedEventArgs     _Args);
        public abstract void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args);

        #endregion

        #region nonpublic methods

        protected abstract void EnableInitializedShapes(bool _Enable);

        protected void RaiseMoneyItemCollected()
        {
            MoneyItemCollected?.Invoke();
        }
        
        protected Vector2 GetCornerAngles(bool _Right, bool _Up, bool _Inner)
        {
            if (_Right)
            {
                if (_Up)
                    return _Inner ? new Vector2(0, 90) : new Vector2(180, 270);
                return _Inner ? new Vector2(270, 360) : new Vector2(90, 180);
            }
            if (_Up)
                return _Inner ? new Vector2(90, 180) : new Vector2(270, 360);
            return _Inner ? new Vector2(180, 270) : new Vector2(0, 90);
        }
        
        protected IMazeItemProceedInfo GetItemInfoByPositionAndType(V2Int _Position, EMazeItemType _Type)
        {
            return Model.GetAllProceedInfos()?
                .FirstOrDefault(_Item => _Item.CurrentPosition == _Position
                                         && _Item.Type == _Type); 
        }
        
        protected bool TurretExist(V2Int _Position)
        {
            var info = GetItemInfoByPositionAndType(_Position, EMazeItemType.Turret);
            return info != null;
        }
        
        protected bool SpringboardExist(V2Int _Position)
        {
            var info = GetItemInfoByPositionAndType(_Position, EMazeItemType.Springboard);
            return info != null;
        }
        
        protected V2Int GetSpringboardDirection(V2Int _Position)
        {
            var info = GetItemInfoByPositionAndType(_Position, EMazeItemType.Springboard);
            if (info != null) 
                return info.Direction;
            Dbg.LogError("Info cannot be null");
            return default;
        }
        
        protected bool PathExist(V2Int _Position)
        {
            return Model.PathItemsProceeder.PathProceeds.Keys.Contains(_Position);
        }
        
        protected bool IsAnyBlockOfConcreteTypeWithSamePosition(EMazeItemType _Type)
        {
            return Model.GetAllProceedInfos().Any(_I =>
                _I.Type == _Type && _I.StartPosition == Props.Position);
        }

        #endregion
    }
}