using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Helpers;
using Common.Providers;
using Common.SpawnPools;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.MazeItems.Props;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.MazeItems.ViewMazeItemPath.ExtraBorders
{
    public interface IViewMazeItemPathExtraBorders : ICloneable, IInit, IActivated
    {
        Func<GameObject>               GetParent                  { set; }
        Func<ViewMazeItemProps>        GetProps                   { set; }
        Func<Color>                    GetBorderColor             { set; }
        Component[]                    Renderers                  { get; }
        void                           HighlightBordersAndCorners();
        void                           DrawBorders();
        void                           AdjustBorders();
        void                           AdjustBordersOnCornerInitialization(bool _Right, bool _Up, bool _Inner);
        void                           EnableInitializedShapes(bool             _Enable);
        Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear);
    }

    public abstract class ViewMazeItemPathExtraBordersBase 
        : InitBase,
          IViewMazeItemPathExtraBorders
    {
        #region nonpublic members
        
        protected bool
            LeftExtraBordersInited,
            RightExtraBordersInited,
            BottomExtraBordersInited,
            TopExtraBordersInited;

        #endregion
        
        #region inject
        
        protected ViewSettings               ViewSettings        { get; }
        protected IModelGame                 Model               { get; }
        protected ICoordinateConverter       CoordinateConverter { get; }
        protected IContainersGetter          ContainersGetter    { get; }
        protected IColorProvider             ColorProvider       { get; }
        protected IViewMazeItemsPathInformer Informer            { get; }

        protected ViewMazeItemPathExtraBordersBase(
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            ICoordinateConverter        _CoordinateConverter,
            IContainersGetter           _ContainersGetter,
            IColorProvider              _ColorProvider,
            IViewMazeItemsPathInformer  _Informer)
        {
            ViewSettings        = _ViewSettings;
            Model               = _Model;
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter    = _ContainersGetter;
            ColorProvider       = _ColorProvider;
            Informer            = _Informer;
        }

        #endregion
        
        #region api

        public virtual bool Activated { get; set; }

        public override void Init()
        {
            Informer.GetProps = GetProps;
            ColorProvider.ColorChanged += OnColorChanged;
            base.Init();
        }
        
        public Func<GameObject>        GetParent      { protected get; set; }
        public Func<ViewMazeItemProps> GetProps       { protected get; set; }
        public Func<Color>             GetBorderColor { protected get; set; }
        
        public abstract Component[] Renderers { get; }

        public abstract object Clone();

        public abstract Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear);

        public abstract void HighlightBordersAndCorners();

        public abstract void DrawBorders();
        
        public virtual void AdjustBorders() { }

        public virtual void AdjustBordersOnCornerInitialization(bool _Right, bool _Up, bool _Inner) { }

        public abstract void EnableInitializedShapes(bool _Enable);

        #endregion

        #region nonpublic methods

        protected abstract void OnColorChanged(int _ColorId, Color _Color);

        protected static void EnableBorderShapes(
            bool                       _Enable,
            IEnumerable<ShapeRenderer> _Rectangles,
            bool                       _Inited)
        {
            if (!_Inited) 
                return;
            foreach (var rect in _Rectangles
                .Where(_Rect => _Rect.Color.a > MathUtils.Epsilon))
            {
                rect.enabled = _Enable;
            }
        }

        #endregion
    }
}