using System.Collections.Generic;
using System.Linq;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Extensions;
using Common.Helpers;
using Common.Ticker;
using Common.Utils;
using GameHelpers;
using RMAZOR.Models;
using RMAZOR.Views.ContainerGetters;
using RMAZOR.Views.Helpers;
using Shapes;
using SpawnPools;
using UnityEngine;

namespace RMAZOR.Views.Common
{
    public interface IViewMazeBackgroundCongradItems: IInit, IOnLevelStageChanged { }

    public class ViewMazeBackgroundCongradItems : ViewMazeBackgroundItemsBase, IViewMazeBackgroundCongradItems 
    {
        #region constants
        
        private const float CongratsAnimSpeed = 1f;

        #endregion

        #region nonpublic members
        
        private readonly BehavioursSpawnPool<Animator> m_BackCongratsItemsPool = 
            new BehavioursSpawnPool<Animator>();
        private readonly Dictionary<Animator, ShapeRenderer[]> m_BackCongratsItemsDict = 
            new Dictionary<Animator, ShapeRenderer[]>();
        
        private static readonly Color[] CongradColorSet =
        {
            new Color(0.72f, 1f, 0.58f),
            new Color(1f, 0.81f, 0.45f),
            new Color(0.56f, 1f, 0.86f),
            new Color(0.44f, 0.55f, 1f),
            new Color(0.91f, 0.52f, 1f),
            new Color(1f, 0.49f, 0.52f),
        };
        
        private float m_LastCongratsItemAnimTime;
        private float m_NextRandomCongratsItemAnimInterval;
        private bool  m_DoAnimateCongrats;

        #endregion

        #region inject
        
        private IPrefabSetManager PrefabSetManager { get; }
        
        public ViewMazeBackgroundCongradItems(
            IColorProvider _ColorProvider,
            IViewAppearTransitioner _Transitioner, 
            IContainersGetter _ContainersGetter, 
            IViewGameTicker _GameTicker,
            ICameraProvider _CameraProvider,
            IPrefabSetManager _PrefabSetManager) 
            : base(
                _ColorProvider, 
                _Transitioner, 
                _ContainersGetter, 
                _GameTicker,
                _CameraProvider)
        {
            PrefabSetManager = _PrefabSetManager;
        }

        #endregion

        #region api
        
        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            m_DoAnimateCongrats = false;
            if (_Args.Stage == ELevelStage.Finished)
            {
                m_DoAnimateCongrats = true;
            }
            else if (_Args.Stage == ELevelStage.Unloaded)
            {
                foreach (var item in m_BackCongratsItemsPool)
                    m_BackCongratsItemsPool.Deactivate(item);
            }
        }
        
        public override void UpdateTick()
        {
            if (!Initialized)
                return;
            if (!m_DoAnimateCongrats)
                return;
            ProceedItems();
        }

        #endregion

        #region nonpublic methods
        
        protected override void InitItems()
        {
            var sourceGos = new List<GameObject>();
            for (int i = 0; i < 2; i++)
            {
                var sourceGo = PrefabSetManager.GetPrefab(
                    "views", $"background_item_congrats_{i + 1}");
                sourceGos.Add(sourceGo);
            }
            for (int i = 0; i < PoolSize; i++)
            {
                int randIdx = Mathf.FloorToInt(Random.value * sourceGos.Count);
                var sourceGo = sourceGos[randIdx];
                var newGo = Object.Instantiate(sourceGo);
                newGo.SetParent(ContainersGetter.GetContainer(ContainerNames.Background));
                var sourceAnim = newGo.GetCompItem<Animator>("animator");
                sourceAnim.speed = CongratsAnimSpeed;
                var triggerer = newGo.GetCompItem<AnimationTriggerer>("triggerer");
                var content = newGo.GetCompItem<Transform>("content");
                var shapes = m_PossibleSourceTypes
                    .SelectMany(_T => content.GetComponentsInChildren(_T))
                    .Cast<ShapeRenderer>()
                    .ToArray();
                
                int colIdx = Mathf.FloorToInt(Random.value * CongradColorSet.Length);
                var col = CongradColorSet[colIdx];

                triggerer.Trigger1 = () =>
                {
                    foreach (var shape in shapes)
                        shape.Color = col;
                };
                triggerer.Trigger2 = () =>
                {
                    var startColA = col.a;
                    Cor.Run(Cor.Lerp(
                    1f,
                    0f,
                    1f / (6f * CongratsAnimSpeed),
                    _Progress =>
                    {
                        foreach (var shape in shapes)
                            shape.Color = shape.Color.SetA(_Progress * startColA);
                    },
                    GameTicker));
                };
                triggerer.Trigger3 = () => m_BackCongratsItemsPool.Deactivate(sourceAnim);
                m_BackCongratsItemsPool.Add(sourceAnim);
                m_BackCongratsItemsDict.Add(sourceAnim, shapes);
            }

            foreach (var item in m_BackCongratsItemsPool)
                m_BackCongratsItemsPool.Deactivate(item);
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            // do nothing
        }

        protected override void ProceedItems()
        {
            if (!(GameTicker.Time > m_NextRandomCongratsItemAnimInterval + m_LastCongratsItemAnimTime)) 
                return;
            m_LastCongratsItemAnimTime = GameTicker.Time;
            m_NextRandomCongratsItemAnimInterval = 0.05f + Random.value * 0.15f;
            var item = m_BackCongratsItemsPool.FirstInactive;
            if (item.IsNull())
                return;
            var tr = item.transform;
            tr.position = RandomPositionOnScreen();
            tr.localScale = Vector3.one * (0.5f + 2f * Random.value);
            m_BackCongratsItemsPool.Activate(item);
            item.SetTrigger(AnimKeys.Anim);
        }

        #endregion
    }
}