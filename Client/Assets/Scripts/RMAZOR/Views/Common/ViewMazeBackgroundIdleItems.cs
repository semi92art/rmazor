using System;
using System.Collections.Generic;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using RMAZOR.Models;
using RMAZOR.Views.ContainerGetters;
using RMAZOR.Views.Helpers;
using RMAZOR.Views.Utils;
using Shapes;
using SpawnPools;
using UnityEngine;

namespace RMAZOR.Views.Common
{
    public interface IViewMazeBackgroundIdleItems : IInit, IOnLevelStageChanged { }
    
    public class ViewMazeBackgroundIdleItems : ViewMazeBackgroundItemsBase, IViewMazeBackgroundIdleItems
    {
        private          Color         m_BackItemsColor;
        private readonly List<Vector2> m_BackIdleItemSpeeds = new List<Vector2>(PoolSize);
        private readonly BehavioursSpawnPool<ShapeRenderer> m_BackIdleItemsPool = 
            new BehavioursSpawnPool<ShapeRenderer>();


        public ViewMazeBackgroundIdleItems(
            IColorProvider _ColorProvider, 
            IViewAppearTransitioner _Transitioner,
            IContainersGetter _ContainersGetter,
            IViewGameTicker _GameTicker,
            ICameraProvider _CameraProvider)
            : base(
                _ColorProvider,
                _Transitioner,
                _ContainersGetter,
                _GameTicker, 
                _CameraProvider) { }

        public override void Init()
        {
            m_BackItemsColor = ColorProvider.GetColor(ColorIds.BackgroundIdleItems);
            base.Init();
        }
        
        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                    AppearBackgroundIdleItems(true);
                    break;
                case ELevelStage.Finished:
                    AppearBackgroundIdleItems(false);
                    break;
            }
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId != ColorIds.BackgroundIdleItems) 
                return;
            m_BackItemsColor = _Color;
            foreach (var rend in m_BackIdleItemsPool)
                rend.Color = _Color;
        }
        
        protected override void InitItems()
        {
            var sources = new List<Disc>();
            for (int i = 0; i < 3; i++)
            {
                var sourceGo = new GameObject("Background Item");
                sourceGo.SetParent(ContainersGetter.GetContainer(ContainerNames.Background));
                var source = sourceGo.AddComponent<Disc>();
                source.Color = m_BackItemsColor;
                source.Radius = 0.25f * i;
                source.SortingOrder = SortingOrders.BackgroundItem;
                source.enabled = false;
                sources.Add(source);
            }
            for (int i = 0; i < PoolSize; i++)
            {
                int randIdx = Mathf.FloorToInt(UnityEngine.Random.value * sources.Count);
                var source = sources[randIdx];
                var newGo = UnityEngine.Object.Instantiate(source.gameObject);
                newGo.SetParent(ContainersGetter.GetContainer(ContainerNames.Background));
                var pos = RandomPositionOnScreen();
                newGo.transform.SetPosXY(pos);
                Component newSourceRaw = null;
                foreach (var possibleType in PossibleSourceTypes)
                {
                    newSourceRaw = newGo.GetComponent(possibleType);
                    if (newSourceRaw != null)
                        break;
                }
                if (newSourceRaw == null)
                    return;
                var newSource = (ShapeRenderer)newSourceRaw;
                if (newSource == null)
                    return;
                newSource.enabled = true;
                m_BackIdleItemsPool.Add(newSource);
                m_BackIdleItemSpeeds.Add(RandomSpeed());
            }
        }

        private void AppearBackgroundIdleItems(bool _Appear)
        {
            Transitioner.DoAppearTransition(_Appear,
                new Dictionary<IEnumerable<Component>, Func<Color>>
                {
                    {m_BackIdleItemsPool, () => m_BackItemsColor}
                },
                _Type: EAppearTransitionType.WithoutDelay);
        }
        
        protected override void ProceedItems()
        {
            for (int i = 0; i < m_BackIdleItemsPool.Count; i++)
            {
                var shape = m_BackIdleItemsPool[i];
                var speed = m_BackIdleItemSpeeds[i] * GameTicker.DeltaTime;
                shape.transform.PlusPosXY(speed.x, speed.y);
                if (IsInsideOfScreenBounds(shape.transform.position.XY(), Vector2.one))
                    continue;
                shape.transform.SetPosXY(RandomPositionOnScreen(false, Vector2.one));
            }
        }


    }
}