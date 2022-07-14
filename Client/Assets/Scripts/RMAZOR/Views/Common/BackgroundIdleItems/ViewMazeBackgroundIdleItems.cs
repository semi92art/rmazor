using System.Collections.Generic;
using Common;
using Common.CameraProviders;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.SpawnPools;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Helpers;
using UnityEngine;

namespace RMAZOR.Views.Common.BackgroundIdleItems
{
    public interface IViewMazeBackgroundIdleItems : IInit, IOnLevelStageChanged
    {
        void SetSpawnPool(int _Index);
    }
    
    public class ViewMazeBackgroundIdleItems : ViewMazeBackgroundItemsBase, IViewMazeBackgroundIdleItems
    {
        #region nonpublic members

        protected override int PoolSize => 15;
        
        private            Color                                          m_BackItemsColor;
        private            SpawnPool<IViewMazeBackgroundIdleItemDisc>     m_DiscsPool;
        private            SpawnPool<IViewMazeBackgroundIdleItemSquare>   m_SquaresPool;
        private            SpawnPool<IViewMazeBackgroundIdleItemTriangle> m_TrianglesPool;

        private SpawnPool<IViewMazeBackgroundIdleItem> m_CurrentPool;
        
        #endregion

        #region inject

        private IPrefabSetManager                   PrefabSetManager           { get; }
        private IViewMazeBackgroundIdleItemDisc     BackgroundIdleItemDisc     { get; }
        private IViewMazeBackgroundIdleItemSquare   BackgroundIdleItemSquare   { get; }
        private IViewMazeBackgroundIdleItemTriangle BackgroundIdleItemTriangle { get; }

        private ViewMazeBackgroundIdleItems(
            IColorProvider                      _ColorProvider,
            IRendererAppearTransitioner         _Transitioner,
            IContainersGetter                   _ContainersGetter,
            IViewGameTicker                     _GameTicker,
            ICameraProvider                     _CameraProvider,
            IPrefabSetManager                   _PrefabSetManager,
            IViewMazeBackgroundIdleItemDisc     _BackgroundIdleItemDisc,
            IViewMazeBackgroundIdleItemSquare   _BackgroundIdleItemSquare,
            IViewMazeBackgroundIdleItemTriangle _BackgroundIdleItemTriangle)
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
        }

        #endregion

        #region api

        public void SetSpawnPool(int _Index)
        {
            m_DiscsPool    .DeactivateAll();
            m_SquaresPool  .DeactivateAll();
            m_TrianglesPool.DeactivateAll();
            m_CurrentPool = _Index switch
            {
                0 => ToSpawnPool<IViewMazeBackgroundIdleItemSquare, IViewMazeBackgroundIdleItem>(m_SquaresPool),
                1 => ToSpawnPool<IViewMazeBackgroundIdleItemTriangle, IViewMazeBackgroundIdleItem>(m_TrianglesPool),
                _ => throw new SwitchCaseNotImplementedException(_Index)
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
            foreach (var disc in m_DiscsPool.GetAllActiveItems())
                disc.OnLevelStageChanged(_Args);
            foreach (var square in m_SquaresPool.GetAllActiveItems())
                square.OnLevelStageChanged(_Args);
            foreach (var triangle in m_TrianglesPool.GetAllActiveItems())
                triangle.OnLevelStageChanged(_Args);
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
            const float multiplier = 0.5f;
            var physicsMaterial = PrefabSetManager.GetObject<PhysicsMaterial2D>(
                "background",
                "background_idle_items_physics_material");
            m_DiscsPool = new SpawnPool<IViewMazeBackgroundIdleItemDisc>();
            m_SquaresPool = new SpawnPool<IViewMazeBackgroundIdleItemSquare>();
            m_TrianglesPool = new SpawnPool<IViewMazeBackgroundIdleItemTriangle>();
            for (int i = 0; i < PoolSize; i++)
            {
                var discItem     = (IViewMazeBackgroundIdleItemDisc)BackgroundIdleItemDisc.Clone();
                var squareItem   = (IViewMazeBackgroundIdleItemSquare)BackgroundIdleItemSquare.Clone();
                var triangleItem = (IViewMazeBackgroundIdleItemTriangle)BackgroundIdleItemTriangle.Clone();
                
                discItem    .Init(Container, physicsMaterial);
                squareItem  .Init(Container, physicsMaterial);
                triangleItem.Init(Container, physicsMaterial);
                
                float coeff = 1f + RandomGen.NextFloat() * 1f;
                discItem    .SetParams(multiplier * coeff, 0.1f);
                squareItem  .SetParams(3f * multiplier * coeff, 0.1f);
                triangleItem.SetParams(3f * multiplier * coeff, 0.1f);
                
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
                if (IsInsideOfScreenBounds(item.Position, 3f * Vector2.one))
                    continue;
                item.Position = RandomPositionOnScreen(false, 3f * Vector2.one);
                item.SetVelocity(RandomVelocity(), RandomAngularVelocity());
            }
        }

        protected override Vector2 RandomVelocity()
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

        #endregion
    }

    public class ViewMazeBackgroundIdleItemsFake : InitBase, IViewMazeBackgroundIdleItems
    {
        public void SetSpawnPool(int                   _Index) { }
        public void OnLevelStageChanged(LevelStageArgs _Args) { }
    }
}