using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Constants;
using Controllers;
using DI.Extensions;
using Entities;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Shapes;
using Ticker;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItemTrapIncreasing : IViewMazeItem
    {
        void OnIncreasing(MazeItemTrapIncreasingEventArgs _Args);
    }
    
    public class ViewMazeItemTrapIncreasing : ViewMazeItemBase, IViewMazeItemTrapIncreasing, IUpdateTick
    {
        #region nonpublic members

        private static AudioClipArgs AudioClipArgsTrapIncreasingOpen =>
            new AudioClipArgs("sword_open", EAudioClipType.Sound);
        private static AudioClipArgs AudioClipArgsTrapIncreasingRotate => 
            new AudioClipArgs("sword_spinning", EAudioClipType.Sound, _Loop: true);
        private static AudioClipArgs AudioClipArgsTrapIncreasingClose => 
            new AudioClipArgs("sword_close", EAudioClipType.Sound);
        
        private static int AnimKeyOpen => AnimKeys.Anim;
        private static int AnimKeyClose => AnimKeys.Stop;
        
        private Animator           m_Animator;
        private bool?              m_TrapOpened;
        private bool               m_ReadyToKill;
        private AnimationTriggerer m_Triggerer;
        private List<Vector2>      m_DeathZone;
        private bool               m_DoPlaySwordSpinningSound = true;

        #endregion

        #region shapes

        protected override string ObjectName => "Trap Increasing Block";
        private readonly List<Line> m_BladeContainers = new List<Line>();
        private readonly List<SpriteRenderer> m_Blades = new List<SpriteRenderer>();
        private Disc m_Center;

        #endregion

        #region inject
        
        public ViewMazeItemTrapIncreasing(
            ViewSettings _ViewSettings,
            IModelGame _Model,
            IMazeCoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IViewGameTicker _GameTicker,
            IViewAppearTransitioner _Transitioner,
            IManagersGetter _Managers,
            IColorProvider _ColorProvider)
            : base(
                _ViewSettings,
                _Model,
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker,
                _Transitioner,
                _Managers,
                _ColorProvider) { }

        #endregion
        
        #region api
        
        public override Component[] Shapes => new Component[]{m_Center}
            .Concat(m_Blades)
            .Concat(m_BladeContainers)
            .ToArray();
        
        public override object Clone() => new ViewMazeItemTrapIncreasing(
            ViewSettings,
            Model,
            CoordinateConverter, 
            ContainersGetter,
            GameTicker,
            Transitioner,
            Managers,
            ColorProvider);

        public override bool ActivatedInSpawnPool
        {
            get => base.ActivatedInSpawnPool;
            set
            {
                m_Animator.SetGoActive(value);
                base.ActivatedInSpawnPool = value;
            }
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            m_Animator.speed = _Args.Stage == ELevelStage.Paused ? 0f : 1f;
            base.OnLevelStageChanged(_Args);
            if (_Args.Stage == ELevelStage.Finished && m_TrapOpened.HasValue && m_TrapOpened.Value)
                CloseTrap();
        }

        public void OnIncreasing(MazeItemTrapIncreasingEventArgs _Args)
        {
            switch (_Args.Stage)
            {
                case TrapsIncreasingProceeder.StageIncreased: OpenTrap(); break;
                case TrapsIncreasingProceeder.StageIdle: CloseTrap();  break;
            }
        }
        
        public void UpdateTick()
        {
            if (!m_ReadyToKill)
                return;
            CheckForCharacterDeath();
        }

        #endregion

        #region nonpublic methods

        protected override void InitShape()
        {
            var prefab = PrefabUtilsEx.InitPrefab(
                Object.transform, "views", "trap_increasing");
            prefab.transform.SetLocalPosXY(Vector2.zero);
            m_Animator = prefab.GetCompItem<Animator>("animator");
            m_Center = prefab.GetCompItem<Disc>("center");
            m_Triggerer = prefab.GetCompItem<AnimationTriggerer>("triggerer");
            m_Triggerer.Trigger1 += () => m_ReadyToKill = true;
            m_Triggerer.Trigger2 += () => m_ReadyToKill = false;
            m_Triggerer.Trigger4 += () =>
            {
                if (m_DoPlaySwordSpinningSound)
                {
                    Managers.AudioManager.PlayClip(AudioClipArgsTrapIncreasingRotate);
                    m_DoPlaySwordSpinningSound = false;
                }
            };
            for (int i = 1; i <= 4; i++)
            {
                m_BladeContainers.Add(prefab.GetCompItem<Line>($"blade_container_{i}"));
                m_Blades.Add(prefab.GetCompItem<SpriteRenderer>($"blade_{i}"));
            }
            foreach (var bladeContainer in m_BladeContainers)
            {
                bladeContainer.Thickness = 0.07f;
                bladeContainer.EndCaps = LineEndCap.Round;
                bladeContainer.enabled = false;
            }
            foreach (var blade in m_Blades)
                blade.enabled = false;
        }

        protected override void UpdateShape()
        {
            Object.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            Object.transform.localScale = Vector3.one * CoordinateConverter.Scale;
            m_DeathZone = new List<V2Int>
            {
                Props.Position + V2Int.down,
                Props.Position + V2Int.up,
                Props.Position + V2Int.left,
                Props.Position + V2Int.right,
                Props.Position + V2Int.down + V2Int.left,
                Props.Position + V2Int.down + V2Int.right,
                Props.Position + V2Int.up + V2Int.left,
                Props.Position + V2Int.up + V2Int.right
            }.Select(_P => _P.ToVector2())
                .ToList();
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.MazeItem1)
            {
                m_Center.Color = _Color;
                foreach (var item in m_Blades)
                    item.color = _Color;
                foreach (var item in m_BladeContainers)
                    item.Color = _Color;
            }
        }

        private void OpenTrap()
        {
            if (AppearingState != EAppearingState.Appeared)
                return;
            if (m_TrapOpened.HasValue && m_TrapOpened.Value)
                return;
            var stage = Model.LevelStaging.LevelStage;
            if (stage == ELevelStage.Finished || stage == ELevelStage.Unloaded)
                return;
            m_TrapOpened = true;
            OpenTrapCoroutine(true);
            Managers.AudioManager.PlayClip(AudioClipArgsTrapIncreasingOpen);
            Managers.HapticsManager.PlayPreset(EHapticsPresetType.HeavyImpact);
        }

        private void CloseTrap()
        {
            if (AppearingState != EAppearingState.Appeared)
                return;
            if (m_TrapOpened.HasValue && !m_TrapOpened.Value)
                return;
            m_TrapOpened = false;
            OpenTrapCoroutine(false);
            m_DoPlaySwordSpinningSound = true;
            Managers.AudioManager.StopClip(AudioClipArgsTrapIncreasingRotate);
            Managers.AudioManager.PlayClip(AudioClipArgsTrapIncreasingClose);
        }

        private void OpenTrapCoroutine(bool _Open)
        {
            m_Animator.SetTrigger(_Open ? AnimKeyOpen : AnimKeyClose);
        }

        protected override void OnAppearStart(bool _Appear)
        {
            if (_Appear)
            {
                m_Animator.enabled = true;
                m_Animator.ResetTrigger(AnimKeyClose);
                m_Animator.ResetTrigger(AnimKeyOpen);
            }
            else
            {
                CloseTrap();
            }
            base.OnAppearStart(_Appear);
        }

        protected override void OnAppearFinish(bool _Appear)
        {
            if (!_Appear)
                m_Animator.enabled = false;
            base.OnAppearFinish(_Appear);
        }

        private void CheckForCharacterDeath()
        {
            if (!Model.Character.Alive)
                return;
            if (Model.PathItemsProceeder.AllPathsProceeded)
                return;
            if (Model.LevelStaging.LevelStage == ELevelStage.Finished)
                return;
            const float distance = 0.5f;
            var character = Model.Character;
            var cPos = character.IsMoving ? 
                character.MovingInfo.PrecisePosition : character.Position.ToVector2();
            bool death = false;
            for (int i = 0; i < m_DeathZone.Count; i++)
            {
                if (!(Vector2.Distance(m_DeathZone[i], cPos) + RazorMazeUtils.Epsilon < distance))
                    continue;
                death = true;
                break;
            }
            if (death)
                Model.LevelStaging.KillCharacter();
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var col = ColorProvider.GetColor(ColorIds.MazeItem1);
            return new Dictionary<IEnumerable<Component>, Func<Color>>
            {
                {new Component[] { m_Center }, () => col}, 
                {m_Blades, () => col}, 
                {m_BladeContainers, () => col.SetA(0.5f)}
            };
        }

        #endregion
    }
}