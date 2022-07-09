using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using Common.Enums;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.CoordinateConverters;
using RMAZOR.Views.MazeItems.Props;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.MazeItems.Additional
{
    public abstract class ViewTurretBodyBase : InitBase, IViewTurretBody
    {
        #region nonpublic members

        private   Rectangle         m_TurretBackground;
        private   bool              m_Activated;
        protected GameObject        Container;
        protected ViewMazeItemProps Props;
        
        #endregion

        #region inject

        protected IModelGame                  Model               { get; }
        protected IViewGameTicker             GameTicker          { get; }
        protected ViewSettings                ViewSettings        { get; }
        protected IColorProvider              ColorProvider       { get; }
        protected ICoordinateConverterRmazor  CoordinateConverter { get; }
        protected IViewFullscreenTransitioner Transitioner        { get; }

        protected ViewTurretBodyBase(
            IModelGame                  _Model,
            IViewGameTicker             _GameTicker,
            ViewSettings                _ViewSettings,
            IColorProvider              _ColorProvider,
            ICoordinateConverterRmazor  _CoordinateConverter,
            IViewFullscreenTransitioner _Transitioner)
        {
            Model               = _Model;
            GameTicker          = _GameTicker;
            ViewSettings        = _ViewSettings;
            ColorProvider       = _ColorProvider;
            CoordinateConverter = _CoordinateConverter;
            Transitioner        = _Transitioner;
        }

        #endregion

        #region api

        public EAppearingState AppearingState { get; private set; }

        public abstract object Clone();
        
        public virtual bool ActivatedInSpawnPool 
        { 
            get => m_Activated;
            set
            {
                m_TurretBackground.enabled = false;
                m_Activated = value;
            }
        }

        public virtual void Update(ViewMazeItemProps _Props)
        {
            Props = _Props;
            Cor.Run(Cor.WaitWhile(() => Container.IsNull(),
                () =>
                {
                    if (!Initialized)
                    {
                        ColorProvider.ColorChanged += OnColorChanged;
                        InitShape();
                        base.Init();
                    }
                    UpdateShape();
                }));
        }

        public void SetTurretContainer(GameObject _Container)
        {
            Container = _Container;
        }
        
        public void Appear(bool _Appear)
        {
            Cor.Run(Cor.WaitWhile(
                () => !Initialized,
                () =>
                {
                    OnAppearStart(_Appear);
                    Transitioner.DoAppearTransition(
                        _Appear,
                        GetAppearSets(_Appear),
                        () => OnAppearFinish(_Appear));
                }));
        }
        
        public abstract void OpenBarrel(bool      _Open, bool _Instantly = false, bool _Forced = false);
        public abstract void HighlightBarrel(bool _Open, bool _Instantly = false, bool _Forced = false);

        #endregion

        #region nonpublic methods

        protected virtual void OnColorChanged(int _ColorId, Color _Color)
        {
            switch (_ColorId)
            {
                case ColorIds.Background1: m_TurretBackground.Color = _Color; break;
            }
        }

        protected virtual void UpdateShape()
        {
            float scale = CoordinateConverter.Scale;
            m_TurretBackground.SetWidth(scale).SetHeight(scale);
            m_TurretBackground.SetCornerRadii(GetBackgroundCornerRadii())
                .SetColor(Color.Lerp(
                    ColorProvider.GetColor(ColorIds.Background1),
                    ColorProvider.GetColor(ColorIds.Background2),
                    0.5f));
        }

        protected virtual Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var mask3Col = ColorProvider.GetColor(ColorIds.Background1);
            return new Dictionary<IEnumerable<Component>, Func<Color>> {{new [] {m_TurretBackground},  () => mask3Col}};
        }

        protected virtual void InitShape()
        {
            m_TurretBackground = Container.AddComponentOnNewChild<Rectangle>(
                    "Turret Background", out GameObject _)
                .SetSortingOrder(SortingOrders.PathLine - 1)
                .SetColor(Color.Lerp(
                    ColorProvider.GetColor(ColorIds.Background1),
                    ColorProvider.GetColor(ColorIds.Background2),
                    0.5f))
                .SetType(Rectangle.RectangleType.RoundedSolid)
                .SetCornerRadiusMode(Rectangle.RectangleCornerRadiusMode.PerCorner);
        }
        
        protected virtual void OnAppearStart(bool _Appear)
        {
            if (_Appear)
                m_TurretBackground.enabled = true;
            AppearingState = _Appear ? EAppearingState.Appearing : EAppearingState.Dissapearing;
        }
        
        protected virtual void OnAppearFinish(bool _Appear)
        {
            if (!_Appear)
                m_TurretBackground.enabled = false;
            AppearingState = _Appear ? EAppearingState.Appeared : EAppearingState.Dissapeared;
        }
        
        protected static int GetSortingOrder()
        {
            return SortingOrders.GetBlockSortingOrder(EMazeItemType.Turret);
        }

        protected abstract Vector4 GetBackgroundCornerRadii();
        
        protected bool IsPathItem(V2Int _Point)
        {
            return Model.PathItemsProceeder.PathProceeds.Keys.Contains(_Point);
        }

        #endregion


        
    }
}