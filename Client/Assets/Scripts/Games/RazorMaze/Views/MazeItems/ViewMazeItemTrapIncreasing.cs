using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Constants;
using DI.Extensions;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Utils;
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
    
    public class ViewMazeItemTrapIncreasing : ViewMazeItemBase, IViewMazeItemTrapIncreasing
    {

        #region nonpublic members

        private static int AnimKeyOpen => AnimKeys.Anim;
        private static int AnimKeyClose => AnimKeys.Stop;
        
        private Animator m_Animator;
        private bool? m_TrapOpened;

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
    }
}