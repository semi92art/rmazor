using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Constants;
using Common.Extensions;
using mazing.common.Runtime;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.SpawnPools;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;
using UnityEngine.Rendering;

namespace RMAZOR.Views.Common.Additional_Background
{
    public interface IViewMazeAdditionalBackgroundDrawer : IInit, IAppear
    {
        void Draw(List<PointsGroupArgs> _Groups, long _LevelIndex);
        void Disable();
    }
    
    public abstract class ViewMazeAdditionalBackgroundDrawerRmazorBase
        : InitBase, 
          IViewMazeAdditionalBackgroundDrawer
    {
        #region constants
        
        private const   int   MasksPoolCount       = 200;
        private const   int   NetLinesPoolCount    = 1000;
        protected const float BorderRelativeIndent = 0.5f;

        #endregion

        #region types

        private class V2IntPair
        {
            public V2Int Item1 { get; }
            
            public V2Int Item2 { get; }

            public V2IntPair(V2Int _Item1, V2Int _Item2)
            {
                Item1 = _Item1;
                Item2 = _Item2;
            }
            
            public override bool Equals(object _Obj)
            {
                if (!(_Obj is V2IntPair otherPair))
                    return false;
                return Equals(otherPair);
            }

            private bool Equals(V2IntPair _OtherPair)
            {
                return (Item1.Equals(_OtherPair.Item1) && Item2.Equals(_OtherPair.Item2))
                       || (Item1.Equals(_OtherPair.Item2) && Item2.Equals(_OtherPair.Item1));
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Item1, Item2);
            }
        }

        #endregion

        #region nonpublic members
        
        private readonly List<V2IntPair> m_NetLinePairsList = new List<V2IntPair>();
        
        protected readonly BehavioursSpawnPool<Line> NetLinesPool = new BehavioursSpawnPool<Line>();
        
        protected readonly BehavioursSpawnPool<Rectangle> TextureRendererMasksPool = new BehavioursSpawnPool<Rectangle>();
        
        protected float CornerScaleCoefficient => ViewSettings.additionalBackgroundType == 2 ? 4f : 0.5f;
        
        protected Transform Container => ContainersGetter.GetContainer(ContainerNamesMazor.MazeItems);

        #endregion

        #region inject
        
        protected ViewSettings                ViewSettings        { get; }
        protected ICoordinateConverter        CoordinateConverter { get; }
        protected IContainersGetter           ContainersGetter    { get; }
        protected IColorProvider              ColorProvider       { get; }
        protected IRendererAppearTransitioner Transitioner        { get; }

        protected ViewMazeAdditionalBackgroundDrawerRmazorBase(
            ViewSettings                _ViewSettings,
            ICoordinateConverter        _CoordinateConverter,
            IContainersGetter           _ContainersGetter,
            IColorProvider              _ColorProvider,
            IRendererAppearTransitioner _Transitioner)
        {
            ViewSettings        = _ViewSettings;
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter    = _ContainersGetter;
            ColorProvider       = _ColorProvider;
            Transitioner        = _Transitioner;
        }

        #endregion

        #region api
        
        public EAppearingState AppearingState { get; protected set; }

        public abstract void Appear(bool _Appear);

        public override void Init()
        {
            InitNetLinesPool();
            InitMasks();
            ColorProvider.ColorChanged += OnColorChanged;
            base.Init();
        }
        
        public abstract void Draw(List<PointsGroupArgs> _Groups, long _LevelIndex);
        
        public virtual void Disable()
        {
            if (ViewSettings.drawAdditionalMazeNet)
                NetLinesPool.DeactivateAll();
            TextureRendererMasksPool.DeactivateAll();
        }

        #endregion

        #region nonpublic methods
        
        protected virtual void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId != ColorIds.Main)
                return;
            foreach (var line in NetLinesPool)
            {
                line.Color = _Color;
            }
        }

        private void InitMasks()
        {
            for (int i = 1; i <= MasksPoolCount; i++)
            {
                var mask = Container.gameObject.AddComponentOnNewChild<Rectangle>(
                    "Additional Texture Mask", 
                    out GameObject _);
                mask.SetBlendMode(ShapesBlendMode.Subtractive)
                    .SetRenderQueue(0)
                    .SetSortingOrder(SortingOrders.AdditionalBackgroundTexture)
                    .SetZTest(CompareFunction.Less)
                    .SetStencilComp(CompareFunction.Greater)
                    .SetStencilOpPass(StencilOp.Replace)
                    .SetColor(new Color(0f, 0f, 0f, 1f / 255f))
                    .SetType(Rectangle.RectangleType.RoundedSolid)
                    .enabled = false;
                TextureRendererMasksPool.Add(mask);
            }
        }

        private void InitNetLinesPool()
        {
            if (!ViewSettings.drawAdditionalMazeNet)
                return;
            for (int i = 1; i <= NetLinesPoolCount; i++)
            {
                var line = Container.gameObject.AddComponentOnNewChild<Line>(
                    "Additional Net Line", 
                    out GameObject _);
                line.SetSortingOrder(SortingOrders.Path + 1)
                    .SetEndCaps(LineEndCap.Round)
                    .enabled = false;
                NetLinesPool.Add(line);
            }
        }

        protected void DrawMaskForGroup(PointsGroupArgs _Group)
        {
            TextureRendererMasksPool.DeactivateAll();
            int minX = _Group.Points.Min(_P => _P.X);
            int minY = _Group.Points.Min(_P => _P.Y);
            int maxX = _Group.Points.Max(_P => _P.X);
            int maxY = _Group.Points.Max(_P => _P.Y);
            float scale = CoordinateConverter.Scale;
            float width = scale * (maxX - minX + 2f * (0.5f + BorderRelativeIndent));
            float height = scale * (maxY - minY + 2f * (0.5f + BorderRelativeIndent));
            var mask = TextureRendererMasksPool.FirstInactive;
            var centerRaw = new Vector2(minX + (maxX - minX) * 0.5f, minY + (maxY - minY) * 0.5f);
            var center = CoordinateConverter.ToLocalMazeItemPosition(centerRaw);
            mask.SetWidth(width)
                .SetHeight(height)
                .SetStencilRefId(Convert.ToByte(_Group.GroupIndex))
                .SetCornerRadius(ViewSettings.LineThickness * CornerScaleCoefficient * scale)
                .transform.SetLocalPosXY(center);
            TextureRendererMasksPool.Activate(mask);
        }

        protected void DrawNetLines(PointsGroupArgs _Group)
        {
            if (!ViewSettings.drawAdditionalMazeNet)
                return;
            NetLinesPool.DeactivateAll();
            m_NetLinePairsList.Clear();
            var points = _Group.Points;

            var dirs = new[] {V2Int.Left, V2Int.Right, V2Int.Down, V2Int.Up};
            
            foreach (var point in points.ToList())
            {
                var neighorurs = dirs.Select(_Dir => point + _Dir).ToList();
                foreach (var neighorur in neighorurs)
                {
                    if (!points.Contains(neighorur))
                        continue;
                    var pair = new V2IntPair(point, neighorur);
                    if (m_NetLinePairsList.Contains(pair))
                        continue;
                    m_NetLinePairsList.Add(pair);
                    var line = NetLinesPool.FirstInactive;
                    NetLinesPool.Activate(line);
                    var center = ((Vector2)point + neighorur) / 2f;
                    var dir = point - neighorur;
                    var orth = new Vector2(dir.Y, dir.X);
                    var pointA = center + orth * 0.5f;
                    var pointB = center - orth * 0.5f;
                    line.SetColor(ColorProvider.GetColor(ColorIds.Main))
                        .SetThickness(CoordinateConverter.Scale * ViewSettings.LineThickness * 0.1f)
                        .SetStart(CoordinateConverter.ToLocalMazeItemPosition(pointA))
                        .SetEnd(CoordinateConverter.ToLocalMazeItemPosition(pointB));
                }
            }
        }

        #endregion
    }
}