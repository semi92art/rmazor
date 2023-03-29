using System;
using System.Collections.Generic;
using Common;
using mazing.common.Runtime;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Utils;
using RMAZOR.Views.Common;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.MazeItems.Props;
using UnityEngine;

namespace RMAZOR.Views.MazeItems.ViewMazeItemPath
{
    public interface IViewMazeItemPathItemIdle
        : ICloneable,
          IAppear,
          IInit
    {
        bool        IsCollected { get; }
        Component[] Renderers   { get; }
        void        InitShape(Func<ViewMazeItemProps> _GetProps, Transform _Parent);
        void        UpdateShape();
        void        Collect(bool                 _Collect);
        void        EnableInitializedShapes(bool _Enable);
    }

    public abstract class ViewMazeItemPathItemIdleBase : InitBase, IViewMazeItemPathItemIdle
    {
        #region nonpublic members
        
        protected Func<ViewMazeItemProps> GetProps { get; private set; }
        protected Transform               Parent   { get; private set; }

        #endregion

        #region inject

        protected ViewSettings                ViewSettings        { get; }
        protected IColorProvider              ColorProvider       { get; }
        protected ICoordinateConverter        CoordinateConverter { get; }
        protected IRendererAppearTransitioner Transitioner        { get; }
        
        protected ViewMazeItemPathItemIdleBase(
            ViewSettings                _ViewSettings,
            IColorProvider              _ColorProvider,
            ICoordinateConverter        _CoordinateConverter,
            IRendererAppearTransitioner _Transitioner)
        {
            ViewSettings        = _ViewSettings;
            ColorProvider       = _ColorProvider;
            CoordinateConverter = _CoordinateConverter;
            Transitioner        = _Transitioner;
        }

        #endregion

        #region api

        public override void Init()
        {
            if (Initialized)
                return;
            ColorProvider.ColorChanged += OnColorChanged;
            base.Init();
        }
        
        public abstract object Clone();

        public void Appear(bool _Appear)
        {
            AppearCore(_Appear);
        }

        public          EAppearingState AppearingState { get; private set; }
        public          bool            IsCollected    { get; protected set; }
        public abstract Component[]     Renderers      { get; }

        public virtual void InitShape(Func<ViewMazeItemProps> _GetProps, Transform _Parent)
        {
            GetProps = _GetProps;
            Parent   = _Parent;
            Init();
        }

        public abstract void UpdateShape();

        public virtual void Collect(bool _Collect)
        {
            IsCollected = _Collect;
        }
        
        public abstract void EnableInitializedShapes(bool _Enable);

        #endregion

        #region nonpublic methods

        protected abstract void OnColorChanged(int _ColorId, Color _Color);
        
        private void AppearCore(bool _Appear)
        {
            if (!ViewSettings.showPathItems)
                return;
            if (!GetProps().IsMoneyItem)
                return;
            var appearSets = GetAppearSets(_Appear);
            Cor.Run(Cor.WaitWhile(
                () => !Initialized,
                () =>
                {
                    OnAppearStart(_Appear);
                    Transitioner.DoAppearTransition(
                        _Appear,
                        appearSets,
                        ViewSettings.betweenLevelTransitionTime,
                        () => OnAppearFinish(_Appear));
                }));
        }

        private void OnAppearStart(bool _Appear)
        {
            var props = GetProps();
            if (!_Appear && (!props.Blank) 
                || _Appear && (props.Blank || props.IsStartNode))
            {
                EnableInitializedShapes(false);
            }
            AppearingState = _Appear ? EAppearingState.Appearing : EAppearingState.Dissapearing;
        }

        private void OnAppearFinish(bool _Appear)
        {
            AppearingState = _Appear ? EAppearingState.Appeared : EAppearingState.Dissapeared;
        }

        private Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var dict = new Dictionary<IEnumerable<Component>, Func<Color>>();
            var props = GetProps();   
            if ((!_Appear || props.Blank || props.IsStartNode)
                && (_Appear || !props.Blank))
            {
                return dict;
            }
            if (!ViewSettings.showPathItems)
                return dict;
            if (props.IsMoneyItem) 
                return dict;
            var pathItemCol = ColorProvider.GetColor(ColorIds.PathItem);
            dict.Add(Renderers, () => pathItemCol);
            return dict;
        }

        #endregion
    }
}