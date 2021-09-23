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
using Shapes;
using Ticker;
using TimeProviders;
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

        private static int AnimKeyOpen => AnimKeys.Anim;
        private static int AnimKeyClose => AnimKeys.Stop;
        
        private Animator m_Animator;
        private bool? m_TrapOpened;
        private bool m_ReadyToKill;
        private AnimationTriggerer m_Triggerer;

        #endregion

        #region shapes

        protected override object[] Shapes => new object[]{m_Center}
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
            IGameTimeProvider _GameTimeProvider,
            IGameTicker _GameTicker)
            : base(
                _ViewSettings,
                _Model,
                _CoordinateConverter,
                _ContainersGetter,
                _GameTimeProvider,
                _GameTicker) { }

        #endregion
        
        #region api
        
        public override object Clone() => new ViewMazeItemTrapIncreasing(
            ViewSettings,
            Model,
            CoordinateConverter, 
            ContainersGetter,
            GameTimeProvider,
            GameTicker);
        
        public override bool Activated
        {
            get => m_Activated;
            set
            {
                m_Activated = value;
                m_Animator.SetGoActive(value);
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
        
        #endregion

        #region nonpublic methods

        protected override void SetShape()
        {
            Object = new GameObject("Trap Increasing");
            Object.SetParent(ContainersGetter.MazeItemsContainer);
            Object.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            var prefab = PrefabUtilsEx.InitPrefab(
                Object.transform, "views", "trap_increasing");
            prefab.transform.SetLocalPosXY(Vector2.zero);
            prefab.transform.localScale = Vector3.one * CoordinateConverter.GetScale();
            m_Animator = prefab.GetCompItem<Animator>("animator");
            m_Center = prefab.GetCompItem<Disc>("center");
            m_Triggerer = prefab.GetCompItem<AnimationTriggerer>("triggerer");
            m_Triggerer.Trigger1 += () => m_ReadyToKill = true;
            m_Triggerer.Trigger2 += () => m_ReadyToKill = false;
            
            m_BladeContainers.Clear();
            m_Blades.Clear();
            for (int i = 1; i <= 4; i++)
            {
                m_BladeContainers.Add(prefab.GetCompItem<Line>($"blade_container_{i}"));
                m_Blades.Add(prefab.GetCompItem<SpriteRenderer>($"blade_{i}"));
            }

            foreach (var bladeContainer in m_BladeContainers)
                bladeContainer.enabled = false;
            foreach (var blade in m_Blades)
                blade.enabled = false;
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
        }

        private void CloseTrap()
        {
            if (AppearingState != EAppearingState.Appeared)
                return;
            if (m_TrapOpened.HasValue && !m_TrapOpened.Value)
                return;
            m_TrapOpened = false;
            Coroutines.Run(OpenTrapCoroutine(false));
        }

        private IEnumerator OpenTrapCoroutine(bool _Open)
        {
            m_Animator.SetTrigger(_Open ? AnimKeyOpen : AnimKeyClose);
            yield return null;
        }

        protected override void Appear(bool _Appear)
        {
            if (_Appear)
            {
                m_Animator.ResetTrigger(AnimKeyClose);
                m_Animator.ResetTrigger(AnimKeyOpen);
            }
            else
            {
                CloseTrap();
            }
            base.Appear(_Appear);
        }

        #endregion

        public void UpdateTick()
        {
            if (!m_ReadyToKill)
                return;
            var positions = new[]
            {
                Props.Position + V2Int.down,
                Props.Position + V2Int.up,
                Props.Position + V2Int.left,
                Props.Position + V2Int.right,
                Props.Position + V2Int.down + V2Int.left,
                Props.Position + V2Int.down + V2Int.right,
                Props.Position + V2Int.up + V2Int.left,
                Props.Position + V2Int.up + V2Int.right
            }.Select(_P => _P.ToVector2());

            var character = Model.Character;
            var cPos = character.IsMoving ? 
                character.MovingInfo.PrecisePosition : character.Position.ToVector2();
            if (positions.All(_P => Vector2.Distance(_P, cPos) + RazorMazeUtils.Epsilon > 1f)) 
                return;
            character.RaiseDeath();
        }
    }
}