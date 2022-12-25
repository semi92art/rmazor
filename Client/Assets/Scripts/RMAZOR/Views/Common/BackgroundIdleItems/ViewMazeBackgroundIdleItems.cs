using System.Collections.Generic;
using Common;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Utils;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Exceptions;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.SpawnPools;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Coordinate_Converters;
using UnityEngine;

namespace RMAZOR.Views.Common.BackgroundIdleItems
{
    public interface IViewMazeBackgroundIdleItems : IInit, IOnLevelStageChanged
    {
        void SetSpawnPool(long _LevelIndex);
    }
    
    public class ViewMazeBackgroundIdleItemsFake : InitBase, IViewMazeBackgroundIdleItems
    {
        public void SetSpawnPool(long                  _LevelIndex) { }
        public void OnLevelStageChanged(LevelStageArgs _Args)       { }
    }
    
    public class ViewMazeBackgroundIdleItems
        : ViewMazeBackgroundItemsBase,
          IViewMazeBackgroundIdleItems
    {
        #region nonpublic members

        protected override int PoolSize => 30;
        
        private            Color                                          m_BackItemsColor;
        private            SpawnPool<IViewMazeBackgroundIdleItemDisc>     m_DiscsPool;
        private            SpawnPool<IViewMazeBackgroundIdleItemSquare>   m_SquaresPool;
        private            SpawnPool<IViewMazeBackgroundIdleItemTriangle> m_TrianglesPool;

        private SpawnPool<IViewMazeBackgroundIdleItem> m_CurrentPool;

        private bool m_CurrentLevelIsBonus;
        
        #endregion

        #region inject

        private IPrefabSetManager                   PrefabSetManager           { get; }
        private IViewMazeBackgroundIdleItemDisc     BackgroundIdleItemDisc     { get; }
        private IViewMazeBackgroundIdleItemSquare   BackgroundIdleItemSquare   { get; }
        private IViewMazeBackgroundIdleItemTriangle BackgroundIdleItemTriangle { get; }
        private ICoordinateConverter                CoordinateConverter        { get; }

        private ViewMazeBackgroundIdleItems(
            IColorProvider                      _ColorProvider,
            IRendererAppearTransitioner         _Transitioner,
            IContainersGetter                   _ContainersGetter,
            IViewGameTicker                     _GameTicker,
            ICameraProvider                     _CameraProvider,
            IPrefabSetManager                   _PrefabSetManager,
            IViewMazeBackgroundIdleItemDisc     _BackgroundIdleItemDisc,
            IViewMazeBackgroundIdleItemSquare   _BackgroundIdleItemSquare,
            IViewMazeBackgroundIdleItemTriangle _BackgroundIdleItemTriangle,
            ICoordinateConverter                _CoordinateConverter)
            : base(
                _ColorProvider,
                _Transitioner,
                _ContainersGetter,
                _GameTicker,
                _CameraProvider)
        {
            PrefabSetManager           = _PrefabSetManager;
            BackgroundIdleItemDisc     = _BackgroundIdleItemDisc;
            BackgroundIdleItemSquare   = _BackgroundIdleItemSquare;
            BackgroundIdleItemTriangle = _BackgroundIdleItemTriangle;
            CoordinateConverter        = _CoordinateConverter;
        }

        #endregion

        #region api

        public void SetSpawnPool(long _LevelIndex)
        {
            m_DiscsPool    .DeactivateAll();
            m_SquaresPool  .DeactivateAll();
            m_TrianglesPool.DeactivateAll();
            long poolIdx = _LevelIndex % 3;
            m_CurrentPool = poolIdx switch
            {
                0 => ToSpawnPool<IViewMazeBackgroundIdleItemSquare, IViewMazeBackgroundIdleItem>(m_SquaresPool),
                1 => ToSpawnPool<IViewMazeBackgroundIdleItemTriangle, IViewMazeBackgroundIdleItem>(m_TrianglesPool),
                2 => ToSpawnPool<IViewMazeBackgroundIdleItemDisc, IViewMazeBackgroundIdleItem>(m_DiscsPool),
                _ => throw new SwitchCaseNotImplementedException(_LevelIndex)
            };
            m_CurrentPool.ActivateAll();
            foreach (var item in m_CurrentPool)
            {
                item.Position = RandomPositionOnScreen(false, 3f * Vector2.one);
                item.SetVelocity(RandomVelocity(), RandomAngularVelocity());
            }
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.LevelStage != ELevelStage.Loaded)
                return;
            string levelType = (string) _Args.Args.GetSafe(CommonInputCommandArg.KeyNextLevelType, out _);
            m_CurrentLevelIsBonus = levelType == CommonInputCommandArg.ParameterLevelTypeBonus;
        }
        
        #endregion

        #region nonpublic methods
        
        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId != ColorIds.Main)
                return;
            m_BackItemsColor = _Color;
            foreach (var item in m_DiscsPool)
                item.SetColor(_Color);
            foreach (var item in m_SquaresPool)
                item.SetColor(_Color);
            foreach (var item in m_TrianglesPool)
                item.SetColor(_Color);
        }
        
        protected override void InitItems()
        {
            var physicsMaterial = PrefabSetManager.GetObject<PhysicsMaterial2D>(
                "background",
                "background_idle_items_physics_material");
            m_DiscsPool     = new SpawnPool<IViewMazeBackgroundIdleItemDisc>();
            m_SquaresPool   = new SpawnPool<IViewMazeBackgroundIdleItemSquare>();
            m_TrianglesPool = new SpawnPool<IViewMazeBackgroundIdleItemTriangle>();
            for (int i = 0; i < PoolSize; i++)
            {
                var discItem     = (IViewMazeBackgroundIdleItemDisc)BackgroundIdleItemDisc.Clone();
                var squareItem   = (IViewMazeBackgroundIdleItemSquare)BackgroundIdleItemSquare.Clone();
                var triangleItem = (IViewMazeBackgroundIdleItemTriangle)BackgroundIdleItemTriangle.Clone();
                
                discItem    .Init(Container, physicsMaterial);
                squareItem  .Init(Container, physicsMaterial);
                triangleItem.Init(Container, physicsMaterial);
                
                const float multiplier = 1.5f;
                float coeff = 1f + RandomGen.NextFloat() * 1f;
                discItem    .SetScale(multiplier * coeff);
                squareItem  .SetScale(multiplier * coeff);
                triangleItem.SetScale(multiplier * coeff);
                
                discItem    .SetColor(m_BackItemsColor);
                squareItem  .SetColor(m_BackItemsColor);
                triangleItem.SetColor(m_BackItemsColor);
                
                m_DiscsPool    .Add(discItem);
                m_SquaresPool  .Add(squareItem);
                m_TrianglesPool.Add(triangleItem);
            }
        }

        protected override void ProceedItems()
        {
            if (m_CurrentPool == null)
                return;
            foreach (var item in m_CurrentPool)
            {
                if (GetMazeSpaceBounds().Contains(item.Position))
                    continue;
                item.Position = RandomPositionOnMazeSpace(false);
                item.SetVelocity(RandomVelocity(), RandomAngularVelocity());
            }
        }

        private Vector2 RandomVelocity()
        {
            return new Vector2(
                RandomGen.NextFloatAlt() * 3f, 
                RandomGen.NextFloatAlt() * 3f);
        }
        
        private float RandomAngularVelocity()
        {
            return RandomGen.NextFloatAlt() * 0.2f;
        }
        
        private static SpawnPool<T2> ToSpawnPool<T1, T2>(IEnumerable<T1> _Collection)
            where T1 : class, T2
            where T2 : class, ISpawnPoolItem
        {
            var pool = new SpawnPool<T2>();
            foreach (var item in _Collection)
                pool.Add(item);
            return pool;
        }

        private Vector2 RandomPositionOnMazeSpace(bool _Inside)
        {
            var bounds = GetMazeSpaceBounds();
            float xDelta, yDelta;
            if (_Inside)
            {
                xDelta = RandomGen.NextFloatAlt() * bounds.size.x * 0.5f;
                yDelta = RandomGen.NextFloatAlt() * bounds.size.y * 0.5f;
            }
            else
            {
                int posCase = RandomGen.Next(1, 4);
                switch (posCase)
                {
                    case 1: // right
                        xDelta = bounds.size.x  * 0.5f;
                        yDelta = bounds.size.y  * 0.5f * RandomGen.NextFloatAlt();
                        break;
                    case 2: // left
                        xDelta = -bounds.size.x * 0.5f;
                        yDelta = bounds.size.y  * 0.5f * RandomGen.NextFloatAlt();
                        break;
                    case 3: // top
                        xDelta = bounds.size.x  * 0.5f * RandomGen.NextFloatAlt();
                        yDelta = bounds.size.y  * 0.5f;
                        break;
                    case 4: // bottom
                        xDelta = bounds.size.x  * 0.5f * RandomGen.NextFloatAlt();
                        yDelta = -bounds.size.y * 0.5f;
                        break;
                    default:
                        throw new SwitchCaseNotImplementedException(posCase);
                }
            }
            float x = bounds.center.x + xDelta;
            float y = bounds.center.y + yDelta;
            return (Vector2)CameraProvider.Camera.transform.position + new Vector2(x, y);
        }
        
        private Bounds GetMazeSpaceBounds()
        {
            var screenBounds = GraphicUtils.GetVisibleBounds(CameraProvider.Camera);
            var mazeBounds = CoordinateConverter.GetMazeBounds();
            screenBounds.size += 6f * Vector3.one;
            var bounds = m_CurrentLevelIsBonus
                ? new Bounds(mazeBounds.center, mazeBounds.size + screenBounds.size)
                : screenBounds;
            return bounds;
        }

        #endregion
    }
}