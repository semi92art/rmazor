using System.Collections.Generic;
using System.Linq;
using Common.CameraProviders;
using Common.Constants;
using Common.Entities;
using Common.Enums;
using Common.Extensions;
using Common.Ticker;
using GameHelpers;
using Managers.Audio;
using RMAZOR.Models;
using RMAZOR.Views.ContainerGetters;
using RMAZOR.Views.Helpers;
using Shapes;
using SpawnPools;
using UnityEngine;

namespace RMAZOR.Views.Common
{
    public class ViewMazeBackgroundCongradItems2 : ViewMazeBackgroundItemsBase, IViewMazeBackgroundCongradItems
    {
        private readonly SpawnPool<Firework>       m_BackCongratsItemsPool = new SpawnPool<Firework>();
        private readonly Dictionary<Firework, int> m_AudioClipIndices      = new Dictionary<Firework, int>();
        
        private float m_LastCongratsItemAnimTime;
        private float m_NextRandomCongratsItemAnimInterval;
        private bool  m_DoAnimateCongrats;
        
        private IPrefabSetManager PrefabSetManager { get; }
        private IAudioManager     AudioManager     { get; }

        public ViewMazeBackgroundCongradItems2(
            IColorProvider _ColorProvider, 
            IViewAppearTransitioner _Transitioner,
            IContainersGetter _ContainersGetter,
            IViewGameTicker _GameTicker, 
            ICameraProvider _CameraProvider,
            IPrefabSetManager _PrefabSetManager,
            IAudioManager _AudioManager)
            : base(
                _ColorProvider, 
                _Transitioner,
                _ContainersGetter,
                _GameTicker, 
                _CameraProvider)
        {
            PrefabSetManager = _PrefabSetManager;
            AudioManager = _AudioManager;
        }
        
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

        protected override void InitItems()
        {
            var sourceGos = new List<GameObject>();
            for (int i = 0; i < 1; i++)
            {
                var sourceGo = PrefabSetManager.GetPrefab(
                    "views", $"background_item_congrats_alt_{i + 1}");
                sourceGos.Add(sourceGo);
            }
            for (int i = 0; i < PoolSize; i++)
            {
                int randIdx = Mathf.FloorToInt(Random.value * sourceGos.Count);
                var sourceGo = sourceGos[randIdx];
                var newGo = Object.Instantiate(sourceGo);
                newGo.SetParent(ContainersGetter.GetContainer(ContainerNames.Background));
                var firework = newGo.GetCompItem<Firework>("firework");
                var content = newGo.GetCompItem<Transform>("content");
                var discs = content.GetComponentsInChildren<Disc>().ToArray();
                var bodies = content.GetComponentsInChildren<Rigidbody>().ToArray();
                firework.InitFirework(discs, bodies, GameTicker);
                m_BackCongratsItemsPool.Add(firework);
                int randAudioIdx = Mathf.FloorToInt(1 + Random.value * 4.9f);
                m_AudioClipIndices.Add(firework, randAudioIdx);
                AudioManager.InitClip(GetAudioClipArgs(firework));
            }
            foreach (var item in m_BackCongratsItemsPool)
                m_BackCongratsItemsPool.Deactivate(item);
            foreach (var go in sourceGos)
                go.DestroySafe();
        }

        protected override void ProceedItems()
        {
            if (!(GameTicker.Time > m_NextRandomCongratsItemAnimInterval + m_LastCongratsItemAnimTime)) 
                return;
            m_LastCongratsItemAnimTime = GameTicker.Time;
            m_NextRandomCongratsItemAnimInterval = 0.05f + Random.value * 0.15f;
            var firework = m_BackCongratsItemsPool.FirstInactive;
            if (firework.IsNull())
                return;
            var tr = firework.transform;
            tr.position = RandomPositionOnScreen();
            tr.localScale = Vector3.one * (0.5f + 1.5f * Random.value);
            m_BackCongratsItemsPool.Activate(firework);
            firework.LaunchFirework();
            if (Random.value < 0.3f)
                AudioManager.PlayClip(GetAudioClipArgs(firework));
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            // do nothing
        }

        private AudioClipArgs GetAudioClipArgs(Firework _Firework)
        {
            int idx = m_AudioClipIndices[_Firework];
            return new AudioClipArgs($"firework_{idx}", EAudioClipType.GameSound, _Id: _Firework.GetInstanceID().ToString());
        }
    }
}