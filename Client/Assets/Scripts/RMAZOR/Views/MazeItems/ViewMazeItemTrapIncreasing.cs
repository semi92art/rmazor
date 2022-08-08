using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Enums;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.MazeItems
{
    public interface IViewMazeItemTrapIncreasing : IViewMazeItem
    {
        void OnIncreasing(MazeItemTrapIncreasingEventArgs _Args);
    }
    
    public class ViewMazeItemTrapIncreasing : ViewMazeItemBase, IViewMazeItemTrapIncreasing, IUpdateTick
    {
        #region nonpublic members

        protected override string ObjectName => "Trap Increasing Block";
        
        private static AudioClipArgs AudioClipArgsTrapIncreasingOpen =>
            new AudioClipArgs("sword_open", EAudioClipType.GameSound);
        private static AudioClipArgs AudioClipArgsTrapIncreasingRotate => 
            new AudioClipArgs("sword_spinning", EAudioClipType.GameSound, _Loop: true);
        private static AudioClipArgs AudioClipArgsTrapIncreasingClose => 
            new AudioClipArgs("sword_close", EAudioClipType.GameSound);
        
        private static int AnimKeyOpen => AnimKeys.Anim;
        private static int AnimKeyClose => AnimKeys.Stop;
        
        private          Animator             m_Animator;
        private          bool?                m_TrapOpened;
        private          bool                 m_ReadyToKill;
        private          AnimationTriggerer   m_Triggerer;
        private          List<Vector2>        m_DeathZone;
        private          bool                 m_DoPlaySwordSpinningSound = true;
        private          int                  m_LevelsLoadCount;
        private readonly List<Line>           m_BladeContainers = new List<Line>();
        private readonly List<SpriteRenderer> m_Blades          = new List<SpriteRenderer>();
        private          Disc                 m_Center, m_Center2;

        #endregion

        #region inject

        private ViewMazeItemTrapIncreasing(
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            ICoordinateConverter  _CoordinateConverter,
            IContainersGetter           _ContainersGetter,
            IViewGameTicker             _GameTicker,
            IRendererAppearTransitioner _Transitioner,
            IManagersGetter             _Managers,
            IColorProvider              _ColorProvider,
            IViewInputCommandsProceeder _CommandsProceeder)
            : base(
                _ViewSettings,
                _Model,
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker,
                _Transitioner,
                _Managers,
                _ColorProvider,
                _CommandsProceeder) { }

        #endregion
        
        #region api
        
        public override Component[] Renderers => new Component[]{m_Center, m_Center2}
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
            ColorProvider,
            CommandsProceeder);

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
            m_Animator.speed = _Args.LevelStage == ELevelStage.Paused ? 0f : 1f;
            base.OnLevelStageChanged(_Args);
            switch (_Args.LevelStage)
            {
                case ELevelStage.Loaded:
                    m_LevelsLoadCount++;
                    break;
                case ELevelStage.Finished when m_TrapOpened.HasValue && m_TrapOpened.Value:
                    CloseTrap();
                    break;
            }
        }

        public void OnIncreasing(MazeItemTrapIncreasingEventArgs _Args)
        {
            switch (_Args.Stage)
            {
                case ModelCommonData.TrapIncreasingStageIncreased: OpenTrap();  break;
                case ModelCommonData.StageIdle:                    CloseTrap(); break;
            }
        }
        
        public void UpdateTick()
        {
            if (!Initialized || !ActivatedInSpawnPool)
                return;
            if (ProceedingStage != EProceedingStage.ActiveAndWorking)
                return;
            if (!m_ReadyToKill)
                return;
            CheckForCharacterDeath();
        }

        #endregion

        #region nonpublic methods

        protected override void InitShape()
        {
            var prefab = Managers.PrefabSetManager.InitPrefab(
                Object.transform, "views", "trap_increasing");
            prefab.transform.SetLocalPosXY(Vector2.zero);
            m_Animator = prefab.GetCompItem<Animator>("animator");
            m_Center = prefab.GetCompItem<Disc>("center");
            m_Center2 = prefab.GetCompItem<Disc>("center_2");
            m_Triggerer = prefab.GetCompItem<AnimationTriggerer>("triggerer");
            m_Triggerer.Trigger1 += () => m_ReadyToKill = true;
            m_Triggerer.Trigger2 += () => m_ReadyToKill = false;
            m_Triggerer.Trigger4 += () =>
            {
                if (m_DoPlaySwordSpinningSound)
                {
                    Managers.AudioManager.PlayClip(AudioClipArgsTrapIncreasingRotate);
                    // FIXME костыль, выключающий звук врашающейся ловушки, если он вдруг не остановится по команде CloseTrap
                    int levelsLoadCountCheck = m_LevelsLoadCount;
                    Cor.Run(Cor.Delay(
                    5f, 
                    GameTicker,
                    () =>
                    {
                        var stage = Model.LevelStaging.LevelStage;
                        if (levelsLoadCountCheck != m_LevelsLoadCount
                            || stage == ELevelStage.Finished
                            || stage == ELevelStage.ReadyToUnloadLevel
                            || stage == ELevelStage.Unloaded)
                        {
                            Managers.AudioManager.StopClip(AudioClipArgsTrapIncreasingRotate);
                        }
                    }));
                    m_DoPlaySwordSpinningSound = false;
                }
            };
            for (int i = 1; i <= 4; i++)
            {
                m_BladeContainers.Add(prefab.GetCompItem<Line>($"blade_container_{i}"));
                m_Blades.Add(prefab.GetCompItem<SpriteRenderer>($"blade_{i}"));
            }
            int sortingOrder = SortingOrders.GetBlockSortingOrder(EMazeItemType.TrapIncreasing);
            m_Center.SortingOrder = m_Center2.SortingOrder = sortingOrder;
            foreach (var bladeContainer in m_BladeContainers)
            {
                bladeContainer.SetSortingOrder(sortingOrder - 1)
                    .SetThickness(0.07f)
                    .SetEndCaps(LineEndCap.Round)
                    .enabled = false;
            }
            foreach (var blade in m_Blades)
            {
                blade.sortingOrder = sortingOrder;
                blade.enabled = false;
            }
        }

        protected override void UpdateShape()
        {
            Object.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            Object.transform.localScale = Vector3.one * CoordinateConverter.Scale;
            m_DeathZone = new List<V2Int>
            {
                Props.Position + V2Int.Down,
                Props.Position + V2Int.Up,
                Props.Position + V2Int.Left,
                Props.Position + V2Int.Right,
                Props.Position + V2Int.Down + V2Int.Left,
                Props.Position + V2Int.Down + V2Int.Right,
                Props.Position + V2Int.Up + V2Int.Left,
                Props.Position + V2Int.Up + V2Int.Right
            }.Select(_P => (Vector2)_P)
                .ToList();
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId != ColorIds.MazeItem1)
                return;
            m_Center.Color = _Color;
            m_Center2.Color = _Color;
            foreach (var item in m_Blades)
                item.color = _Color;
            foreach (var item in m_BladeContainers)
                item.Color = _Color.SetA(0.5f);
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
            m_Animator.SetTrigger(AnimKeyOpen);
            Managers.AudioManager.PlayClip(AudioClipArgsTrapIncreasingOpen);
        }

        private void CloseTrap()
        {
            if (AppearingState != EAppearingState.Appeared)
                return;
            if (m_TrapOpened.HasValue && !m_TrapOpened.Value)
                return;
            m_TrapOpened = false;
            m_Animator.SetTrigger(AnimKeyClose);
            m_DoPlaySwordSpinningSound = true;
            Managers.AudioManager.StopClip(AudioClipArgsTrapIncreasingRotate);
            Managers.AudioManager.PlayClip(AudioClipArgsTrapIncreasingClose);
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
                character.MovingInfo.PrecisePosition : character.Position;
            bool death = false;
            for (int i = 0; i < m_DeathZone.Count; i++)
            {
                if (!(Vector2.Distance(m_DeathZone[i], cPos) + MathUtils.Epsilon < distance))
                    continue;
                death = true;
                break;
            }
            if (death)
            {
                CommandsProceeder.RaiseCommand(EInputCommand.KillCharacter, 
                    new object[] { CoordinateConverter.ToLocalCharacterPosition(cPos) });
            }
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var col = ColorProvider.GetColor(ColorIds.MazeItem1);
            return new Dictionary<IEnumerable<Component>, Func<Color>>
            {
                {new Component[] { m_Center, m_Center2 }, () => col}, 
                {m_Blades, () => col}, 
                {m_BladeContainers, () => col.SetA(0.5f)}
            };
        }

        #endregion
    }
}