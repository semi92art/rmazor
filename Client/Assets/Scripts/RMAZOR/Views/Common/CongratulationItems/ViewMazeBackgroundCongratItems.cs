using System.Collections.Generic;
using System.Linq;
using Common.Constants;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Utils;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.SpawnPools;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Common.CongratulationItems
{
    public class ViewMazeBackgroundCongratItems : ViewMazeBackgroundItemsBase, IViewMazeBackgroundCongratItems 
    {
        #region constants
        
        private const float CongratsAnimSpeed = 1f;

        #endregion

        #region nonpublic members
        
        private readonly BehavioursSpawnPool<Animator> m_BackCongratsItemsPool = 
            new BehavioursSpawnPool<Animator>();
        private readonly Dictionary<Animator, ShapeRenderer[]> m_BackCongratsItemsDict = 
            new Dictionary<Animator, ShapeRenderer[]>();
        
        private static readonly Color[] ColorSet =
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

        private ViewMazeBackgroundCongratItems(
            IColorProvider              _ColorProvider,
            IRendererAppearTransitioner _Transitioner,
            IContainersGetter           _ContainersGetter,
            IViewGameTicker             _GameTicker,
            ICameraProvider             _CameraProvider,
            IPrefabSetManager           _PrefabSetManager)
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

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            m_DoAnimateCongrats = false;
            switch (_Args.LevelStage)
            {
                case ELevelStage.Finished:
                    m_DoAnimateCongrats = true;
                    break;
                case ELevelStage.Unloaded:
                {
                    m_BackCongratsItemsPool.DeactivateAll();
                    break;
                }
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
                    "background", $"background_item_congrats_{i + 1}");
                sourceGos.Add(sourceGo);
            }
            for (int i = 0; i < PoolSize; i++)
            {
                int randIdx = Mathf.FloorToInt(Random.value * sourceGos.Count);
                var sourceGo = sourceGos[randIdx];
                var newGo = Object.Instantiate(sourceGo);
                newGo.SetParent(ContainersGetter.GetContainer(ContainerNamesCommon.Background));
                var sourceAnim = newGo.GetCompItem<Animator>("animator");
                sourceAnim.speed = CongratsAnimSpeed;
                var triggerer = newGo.GetCompItem<AnimationTriggerer>("triggerer");
                var content = newGo.GetCompItem<Transform>("content");
                var shapes = PossibleSourceTypes
                    .SelectMany(_T => content.GetComponentsInChildren(_T))
                    .Cast<ShapeRenderer>()
                    .ToArray();
                int colIdx = Mathf.FloorToInt(Random.value * ColorSet.Length);
                var col = ColorSet[colIdx];
                triggerer.Trigger1 = () =>
                {
                    foreach (var shape in shapes)
                        SetColorByTheme(shape, col);
                };
                triggerer.Trigger2 = () =>
                {
                    float startColA = col.a;
                    Cor.Run(Cor.Lerp(
                        GameTicker,
                    1f / (6f * CongratsAnimSpeed),
                    1f,
                    0f,
                    _Progress =>
                    {
                        foreach (var shape in shapes)
                            shape.Color = shape.Color.SetA(_Progress * startColA);
                    }));
                };
                triggerer.Trigger3 = () => m_BackCongratsItemsPool.Deactivate(sourceAnim);
                m_BackCongratsItemsPool.Add(sourceAnim);
                m_BackCongratsItemsDict.Add(sourceAnim, shapes);
            }
            m_BackCongratsItemsPool.DeactivateAll();
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

        private void SetColorByTheme(ShapeRenderer _Renderer, Color? _Color = null)
        {
            const float saturation = 80f / 100f;
            var col = _Color ?? _Renderer.Color;
            Color.RGBToHSV(col, out float h, out _, out float v);
            col = Color.HSVToRGB(h, saturation, v);
            _Renderer.Color = col;
        }

        #endregion
    }
}