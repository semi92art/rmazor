using Common;
using Common.CameraProviders;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.SpawnPools;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Views.Helpers;
using UnityEngine;

namespace RMAZOR.Views.Common.BackgroundIdleItems
{
    public interface IViewMazeBackgroundIdleItems : IInit { }
    
    public class ViewMazeBackgroundIdleItems : ViewMazeBackgroundItemsBase, IViewMazeBackgroundIdleItems
    {
        #region nonpublic members

        protected override int                           PoolSize => 50;
        private            Color                         m_BackItemsColor;
        private readonly   SpawnPool<IViewMazeBackgroundIdleItem> m_BackIdleItemsPool 
            = new SpawnPool<IViewMazeBackgroundIdleItem>();

        #endregion

        #region inject

        private IPrefabSetManager                 PrefabSetManager         { get; }
        private IViewMazeBackgroundIdleItemDisc   BackgroundIdleItemDisc   { get; }
        private IViewMazeBackgroundIdleItemSquare BackgroundIdleItemSquare { get; }

        public ViewMazeBackgroundIdleItems(
            IColorProvider                    _ColorProvider,
            IViewBetweenLevelTransitioner     _Transitioner,
            IContainersGetter                 _ContainersGetter,
            IViewGameTicker                   _GameTicker,
            ICameraProvider                   _CameraProvider,
            IPrefabSetManager                 _PrefabSetManager,
            IViewMazeBackgroundIdleItemDisc   _BackgroundIdleItemDisc,
            IViewMazeBackgroundIdleItemSquare _BackgroundIdleItemSquare)
            : base(
                _ColorProvider,
                _Transitioner,
                _ContainersGetter,
                _GameTicker,
                _CameraProvider)
        {
            PrefabSetManager         = _PrefabSetManager;
            BackgroundIdleItemDisc   = _BackgroundIdleItemDisc;
            BackgroundIdleItemSquare = _BackgroundIdleItemSquare;
        }

        #endregion

        #region nonpublic methods
        
        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            // if (_ColorId != ColorIds.Background1 
            //     && _ColorId != ColorIds.Background2) 
            //     return;
            // var col = Color.Lerp(
            //     ColorProvider.GetColor(ColorIds.Background1),
            //     ColorProvider.GetColor(ColorIds.Background2),
            //     0.5f);
            // m_BackItemsColor = col;
            // foreach (var item in m_BackIdleItemsPool)
            //     item.SetColor(col);

            if (_ColorId == ColorIds.Main)
            {
                m_BackItemsColor = _Color;
                foreach (var item in m_BackIdleItemsPool)
                    item.SetColor(_Color);
            }
        }
        
        protected override void InitItems()
        {
            const float scaleCoeff = 0.5f;
            var physicsMaterial = PrefabSetManager.GetObject<PhysicsMaterial2D>(
                "views",
                "background_idle_items_physics_material");
            for (int i = 0; i < PoolSize; i++)
            {
                float rand = RandomGen.NextFloat();
                var newItem =
                    (IViewMazeBackgroundIdleItem) (rand > 0.001f
                        ? BackgroundIdleItemDisc.Clone()
                        : BackgroundIdleItemSquare.Clone());
                newItem.Init(Container, physicsMaterial);
                switch (newItem)
                {
                    case IViewMazeBackgroundIdleItemDisc disc:
                    {
                        float coeff = 1f + RandomGen.NextFloat() * 3f;
                        disc.SetParams(scaleCoeff * coeff, 0.1f);
                        break;
                    }
                    case IViewMazeBackgroundIdleItemSquare square:
                        float coeff1 = 2f * (1f + RandomGen.NextFloat() * 3f);
                        float coeff2 = 2f * (1f + RandomGen.NextFloat() * 3f);
                        square.SetParams(scaleCoeff * coeff1, scaleCoeff * coeff2, 0.1f);
                        break;
                }
                newItem.Position = RandomPositionOnScreen();
                newItem.SetVelocity(RandomVelocity());
                newItem.SetColor(m_BackItemsColor);
                m_BackIdleItemsPool.Add(newItem);
                m_BackIdleItemsPool.Activate(newItem);
            }
        }

        protected override void ProceedItems()
        {
            foreach (var item in m_BackIdleItemsPool)
            {
                if (IsInsideOfScreenBounds(item.Position, 3f * Vector2.one))
                    continue;
                item.Position = RandomPositionOnScreen(false, 3f * Vector2.one);
                item.SetVelocity(RandomVelocity());
            }
        }

        protected override Vector2 RandomVelocity()
        {
            return new Vector2(
                RandomGen.NextFloatAlt() * 3f, 
                RandomGen.NextFloatAlt() * 3f);
        }

        #endregion
    }
}