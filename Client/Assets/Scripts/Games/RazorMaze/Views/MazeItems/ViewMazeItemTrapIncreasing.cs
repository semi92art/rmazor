using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Constants;
using DI.Extensions;
using Entities;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
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
        #region constants

        private const string SoundClipNameTrapIncreasingOpen = "sword_open";
        private const string SoundClipNameTrapIncreasingRotate = "sword_rotating";
        private const string SoundClipNameTrapIncreasingClose = "sword_close";

        #endregion
        
        
        #region nonpublic members

        private static int AnimKeyOpen => AnimKeys.Anim;
        private static int AnimKeyClose => AnimKeys.Stop;
        
        private Animator m_Animator;
        private bool? m_TrapOpened;
        private bool m_ReadyToKill;
        private AnimationTriggerer m_Triggerer;
        private List<Vector2> m_DeathZone;

        #endregion

        #region shapes

        protected override object[] DefaultColorShapes => new object[]{m_Center}
            .Concat(m_Blades)
            .Concat(m_BladeContainers)
            .ToArray();
        private readonly List<Line> m_BladeContainers = new List<Line>();
        private readonly List<SpriteRenderer> m_Blades = new List<SpriteRenderer>();
        private Disc m_Center;

        #endregion

        #region inject
        
        public ViewMazeItemTrapIncreasing(
            ViewSettings _ViewSettings,
            IModelGame _Model,
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IGameTicker _GameTicker,
            IViewAppearTransitioner _Transitioner,
            IManagersGetter _Managers)
            : base(
                _ViewSettings,
                _Model,
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker,
                _Transitioner,
                _Managers) { }

        #endregion
        
        #region api
        
        public override object Clone() => new ViewMazeItemTrapIncreasing(
            ViewSettings,
            Model,
            CoordinateConverter, 
            ContainersGetter,
            GameTicker,
            Transitioner,
            Managers);

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
            Object = new GameObject("Trap Increasing");
            Object.SetParent(ContainersGetter.MazeItemsContainer);
            var prefab = PrefabUtilsEx.InitPrefab(
                Object.transform, "views", "trap_increasing");
            prefab.transform.SetLocalPosXY(Vector2.zero);
            prefab.transform.localScale = Vector3.one * CoordinateConverter.GetScale();
            m_Animator = prefab.GetCompItem<Animator>("animator");
            m_Center = prefab.GetCompItem<Disc>("center");
            m_Triggerer = prefab.GetCompItem<AnimationTriggerer>("triggerer");
            m_Triggerer.Trigger1 += () => m_ReadyToKill = true;
            m_Triggerer.Trigger2 += () => m_ReadyToKill = false;
            m_Triggerer.Trigger3 += OnRotationStopped;
            
            m_BladeContainers.Clear();
            m_Blades.Clear();
            for (int i = 1; i <= 4; i++)
            {
                m_BladeContainers.Add(prefab.GetCompItem<Line>($"blade_container_{i}"));
                m_Blades.Add(prefab.GetCompItem<SpriteRenderer>($"blade_{i}"));
            }

            foreach (var bladeContainer in m_BladeContainers)
            {
                bladeContainer.Thickness = 0.07f;
                bladeContainer.EndCaps = LineEndCap.None;
                bladeContainer.enabled = false;
            }
            foreach (var blade in m_Blades)
                blade.enabled = false;
        }

        protected override void UpdateShape()
        {
            Object.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));

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

        private void OnRotationStopped()
        {
            Managers.Notify(_SM => _SM.StopClip(SoundClipNameTrapIncreasingRotate));
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
            Coroutines.Run(OpenTrapCoroutine(true));
            Managers.Notify(_SM => _SM.PlayClip(SoundClipNameTrapIncreasingOpen));
        }

        private void CloseTrap()
        {
            if (AppearingState != EAppearingState.Appeared)
                return;
            if (m_TrapOpened.HasValue && !m_TrapOpened.Value)
                return;
            m_TrapOpened = false;
            Coroutines.Run(OpenTrapCoroutine(false));
            Managers.Notify(_SM => _SM.PlayClip(SoundClipNameTrapIncreasingClose));
        }

        private IEnumerator OpenTrapCoroutine(bool _Open)
        {
            m_Animator.SetTrigger(_Open ? AnimKeyOpen : AnimKeyClose);
            yield return null;
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
            const float distance = 0.5f;
            var character = Model.Character;
            var cPos = character.IsMoving ? 
                character.MovingInfo.PrecisePosition : character.Position.ToVector2();
            if (m_DeathZone.All(_P =>
                Vector2.Distance(_P, cPos) + RazorMazeUtils.Epsilon > distance)) 
                return;
            character.RaiseDeath();
        }

        #endregion
    }
}