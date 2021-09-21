using System.Collections;
using System.Collections.Generic;
using Constants;
using Extensions;
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

        private int m_PrevStage = ItemsProceederBase.StageIdle;
        
        private Animator m_Animator;
        private AnimationEventCounter m_Counter;
        private readonly List<Line> m_BladeContainers = new List<Line>();
        private readonly List<SpriteRenderer> m_Blades = new List<SpriteRenderer>();
        
        #endregion

        #region inject
        
        public IModelCharacter Character { get; }

        public ViewMazeItemTrapIncreasing(
            ViewSettings _ViewSettings,
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IGameTimeProvider _GameTimeProvider,
            IModelMazeData _Data,
            IModelCharacter _Character,
            ITicker _Ticker)
            : base(_ViewSettings, _Data, _CoordinateConverter, _ContainersGetter, _GameTimeProvider, _Ticker)
        {
            Character = _Character;
        }

        #endregion
        
        #region api

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

            m_PrevStage = _Args.Stage;
        }
        
        public override object Clone() => new ViewMazeItemTrapIncreasing(
                ViewSettings, CoordinateConverter, ContainersGetter, GameTimeProvider, Data, Character, Ticker);

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
            m_Counter = prefab.GetCompItem<AnimationEventCounter>("counter");
            
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

        protected override void Appear(bool _Appear)
        {
            Coroutines.Run(Coroutines.WaitWhile(
                () => !Initialized,
                () =>
                {
                    RazorMazeUtils.DoAppearTransitionSimple(
                        _Appear,
                        GameTimeProvider,
                        new Dictionary<IEnumerable<ShapeRenderer>, Color>
                        {
                            {m_BladeContainers, DrawingUtils.ColorLines}
                        });
                    RazorMazeUtils.DoAppearTransitionSimple(
                        _Appear,
                        GameTimeProvider,
                        new Dictionary<IEnumerable<Renderer>, Color>
                        {
                            {m_Blades, DrawingUtils.ColorLines}
                        });
                }));
        }

        private void OpenTrap() => Coroutines.Run(OpenTrapCoroutine(true));

        private void CloseTrap() => Coroutines.Run(OpenTrapCoroutine(false));

        private IEnumerator OpenTrapCoroutine(bool _Open)
        {
            if (!_Open)
            {
                int eventsCount = m_Counter.count;
                while (eventsCount == m_Counter.count)
                    yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
            m_Animator.SetTrigger(_Open ? AnimKeyOpen : AnimKeyClose);
        }

        #endregion
    }
}