using System;
using System.Linq;
using Common;
using Common.Entities;
using Common.Exceptions;
using Common.Providers;
using Common.Ticker;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;
using RMAZOR.Views.MazeItems.Props;
using UnityEngine;

namespace RMAZOR.Views.MazeItems.ViewMazeItemPath
{
    public interface IViewMazeItemsPathInformer : ICloneable
    {
        Func<ViewMazeItemProps> GetProps { set; }
        bool                    IsBorderNearTrapReact(EDirection      _Side);
        bool                    IsBorderNearTrapIncreasing(EDirection _Side);
        bool                    TurretExist(V2Int                             _Position);
        bool                    SpringboardExist(V2Int                        _Position);
        V2Int                   GetSpringboardDirection(V2Int                 _Position);
        bool                    PathExist(V2Int                               _Position);
        bool                    IsAnyBlockOfConcreteTypeWithSamePosition(EMazeItemType _Type);
        IMazeItemProceedInfo    GetItemInfoByPositionAndType(V2Int _Position, EMazeItemType _Type);
        Vector2                 GetCornerAngles(bool _Right, bool _Up, bool _Inner);
        bool                    MustInitBorder(EDirection _Side);
        Color                   GetHighlightColor();

        Tuple<Vector2, Vector2> GetBorderPointsRaw(
            EDirection _Side,
            bool               _StartLimit,
            bool               _EndLimit,
            float              _Scale);
    }
    
    public class ViewMazeItemsPathInformer : IViewMazeItemsPathInformer
    {
        #region inject
        
        private IModelGame      Model          { get; }
        private ViewSettings    ViewSettings   { get; }
        private IColorProvider  ColorProvider  { get; }
        private IViewGameTicker ViewGameTicker { get; }

        private ViewMazeItemsPathInformer(
            IModelGame      _Model,
            ViewSettings    _ViewSettings,
            IColorProvider  _ColorProvider,
            IViewGameTicker _ViewGameTicker)
        {
            Model          = _Model;
            ViewSettings   = _ViewSettings;
            ColorProvider  = _ColorProvider;
            ViewGameTicker = _ViewGameTicker;
        }

        #endregion

        #region api
        
        public Func<ViewMazeItemProps> GetProps { private get; set; }
        
        public object Clone()
        {
            var @object = new ViewMazeItemsPathInformer(
                Model,
                ViewSettings,
                ColorProvider,
                ViewGameTicker) {GetProps = null};
            return @object;
        }

        public bool IsBorderNearTrapReact(EDirection _Side)
        {
            var dir = RmazorUtils.GetDirectionVector(_Side, EMazeOrientation.North);
            return Model.GetAllProceedInfos().Any(_Item =>
                _Item.Type == EMazeItemType.TrapReact 
                && _Item.StartPosition == GetProps().Position + dir
                && _Item.Direction == -dir);
        }

        public bool IsBorderNearTrapIncreasing(EDirection _Side)
        {
            var dir = RmazorUtils.GetDirectionVector(_Side, EMazeOrientation.North);
            return Model.GetAllProceedInfos().Any(_Item =>
                _Item.CurrentPosition == GetProps().Position + dir
                && _Item.Type == EMazeItemType.TrapIncreasing);
        }

        public bool TurretExist(V2Int _Position)
        {
            var info = GetItemInfoByPositionAndType(_Position, EMazeItemType.Turret);
            return info != null;
        }

        public bool SpringboardExist(V2Int _Position)
        {
            var info = GetItemInfoByPositionAndType(_Position, EMazeItemType.Springboard);
            return info != null;
        }

        public V2Int GetSpringboardDirection(V2Int _Position)
        {
            var info = GetItemInfoByPositionAndType(_Position, EMazeItemType.Springboard);
            if (info != null)
                return info.Direction;
            Dbg.LogError("Info cannot be null");
            return default;
        }

        public bool PathExist(V2Int _Position)
        {
            return Model.PathItemsProceeder.PathProceeds.Keys.Contains(_Position);
        }

        public bool  IsAnyBlockOfConcreteTypeWithSamePosition(EMazeItemType _Type)
        {
            return Model.GetAllProceedInfos().Any(_I =>
                _I.Type == _Type && _I.StartPosition == GetProps().Position);
        }

        public Tuple<Vector2, Vector2> GetBorderPointsRaw(
            EDirection _Side,
            bool               _StartLimit,
            bool               _EndLimit,
            float              _Scale)
        {
            Vector2 pos = GetProps().Position;
            float cr = ViewSettings.CornerRadius;
            Vector2 left, right, down, up, zero;
            (left, right, down, up, zero) = (Vector2.left, Vector2.right, Vector2.down, Vector2.up, Vector2.zero);
            Vector2 start, end;
            float c1 = IsBorderNearTrapReact(_Side) ? 1f : 1f;
            switch (_Side)
            {
                case EDirection.Up:
                    start = pos + (c1 * up + left    * _Scale) * 0.5f + (_StartLimit ? right * cr : zero);
                    end   = pos + (c1 * up + right   * _Scale) * 0.5f + (_EndLimit   ? left  * cr : zero);
                    break;
                case EDirection.Right:
                    start = pos + (c1 * right + down * _Scale) * 0.5f + (_StartLimit ? up    * cr : zero);
                    end   = pos + (c1 * right + up   * _Scale) * 0.5f + (_EndLimit   ? down  * cr : zero);
                    break;
                case EDirection.Down:
                    start = pos + (c1 * down + left  * _Scale) * 0.5f + (_StartLimit ? right * cr : zero);
                    end   = pos + (c1 * down + right * _Scale) * 0.5f + (_EndLimit   ? left  * cr : zero);
                    break;
                case EDirection.Left:
                    start = pos + (c1 * left + down * _Scale)  * 0.5f + (_StartLimit ? up     * cr : zero);
                    end   = pos + (c1 * left + up   * _Scale)  * 0.5f + (_EndLimit   ? down   * cr : zero);
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Side);
            }
            return new Tuple<Vector2, Vector2>(start, end);
        }
        
        public IMazeItemProceedInfo GetItemInfoByPositionAndType(V2Int _Position, EMazeItemType _Type)
        {
            return Model.GetAllProceedInfos()?
                .FirstOrDefault(_Item => _Item.CurrentPosition == _Position
                                         && _Item.Type == _Type); 
        }
        
        public Vector2 GetCornerAngles(bool _Right, bool _Up, bool _Inner)
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
        
        public bool MustInitBorder(EDirection _Side)
        {
            var pos = GetProps().Position + RmazorUtils.GetDirectionVector(_Side, EMazeOrientation.North);
            return !TurretExist(pos) && !PathExist(pos);
        }
        
        public Color GetHighlightColor()
        {
            const float lerpSpeed = 4f;
            const float maxLerpValue = 0.2f;
            var col1 = ColorProvider.GetColor(ColorIds.Main);
            var col2 = ColorProvider.GetColor(ColorIds.Background1);
            float lerpCoeff = maxLerpValue * (1f + 0.5f * Mathf.Cos(lerpSpeed * ViewGameTicker.Time));
            var col = Color.Lerp(col1, col2, lerpCoeff);
            return col;
        }

        #endregion
    }
}