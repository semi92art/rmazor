using System.Collections;
using Constants;
using Extensions;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.ContainerGetters;
using Ticker;
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

        private int m_PrevStage = TrapsIncreasingProceeder.StageIdle;
        
        private Animator m_Animator;
        private AnimationEventCounter m_Counter;
        
        #endregion

        #region inject
        
        public IModelMazeData Data { get; }
        public IModelCharacter Character { get; }

        public ViewMazeItemTrapIncreasing(
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IModelMazeData _Data,
            IModelCharacter _Character,
            ITicker _Ticker)
            : base(_CoordinateConverter, _ContainersGetter, _Ticker)
        {
            Data = _Data;
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
        
        public override object Clone() =>
            new ViewMazeItemTrapIncreasing(CoordinateConverter, ContainersGetter, Data, Character, Ticker);
        
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