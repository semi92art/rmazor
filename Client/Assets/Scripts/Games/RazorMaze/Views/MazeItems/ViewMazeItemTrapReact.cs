using System;
using System.Collections;
using System.Linq;
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
    public interface IViewMazeItemTrapReact : IViewMazeItem
    {
        void OnTrapReact(MazeItemTrapReactEventArgs _Args);
    }
    
    public class ViewMazeItemTrapReact : ViewMazeItemBase, IViewMazeItemTrapReact, IUpdateTick
    {
        #region constants

        private const float StartPos = 0.1f;
        private const float MiddlePos = 0.2f;
        private const float FinalPos = 0.7f;
        
        #endregion
        
        #region nonpublic members
        
        private float m_Progress;
        
        #endregion
        
        #region shapes

        protected override object[] Shapes => new object[] {m_Line, m_Trap};
        private Line m_Line;
        private SpriteRenderer m_Trap;
        
        #endregion
        
        #region inject

        public IModelCharacter Character { get; }
        private ModelSettings ModelSettings { get; }

        public ViewMazeItemTrapReact(
            IModelMazeData _Data,
            IModelCharacter _Character,
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IGameTicker _GameTicker,
            IGameTimeProvider _GameTimeProvider,
            ModelSettings _ModelSettings,
            ViewSettings _ViewSettings)
            : base(_ViewSettings, _Data, _CoordinateConverter, _ContainersGetter, _GameTimeProvider, _GameTicker)
        {
            Character = _Character;
            ModelSettings = _ModelSettings;
        }
        
        #endregion
        
        #region api

        public override bool Activated
        {
            get => m_Activated;
            set
            {
                m_Activated = value;
                m_Line.enabled = value;
                m_Trap.enabled = value;
            }
        }

        public void UpdateTick()
        {
            if (!Initialized || !Activated)
                return;
            if (!Proceeding)
                return;
            float rotSpeed = ViewSettings.MovingTrapRotationSpeed * Time.deltaTime; 
            m_Trap.transform.Rotate(Vector3.forward * rotSpeed);
            CheckForCharacterDeath();
        }
        
        public override object Clone() => new ViewMazeItemTrapReact(
            Data, Character, CoordinateConverter, ContainersGetter, GameTicker, GameTimeProvider, ModelSettings, ViewSettings);
        
        public void OnTrapReact(MazeItemTrapReactEventArgs _Args)
        {
            IEnumerator coroutine = null;
            switch (_Args.Stage)
            {
                case ItemsProceederBase.StageIdle: break;
                case TrapsReactProceeder.StagePreReact:   coroutine = HandlePreReact(); break;
                case TrapsReactProceeder.StageReact:      coroutine = HandleReact(); break;
                case TrapsReactProceeder.StageAfterReact: coroutine = HandlePostReact(); break;
                default: throw new ArgumentOutOfRangeException(
                    nameof(_Args.Stage), $"Stage {_Args.Stage} was not implemented");
            }
            Coroutines.Run(coroutine);
        }

        #endregion

        #region nonpublic methods
        
        protected override void SetShape()
        {
            var scale = CoordinateConverter.GetScale();
            var go = Object;
            var line = ContainersGetter.MazeItemsContainer.gameObject
                .GetOrAddComponentOnNewChild<Line>(
                    "Trap React Item",
                    ref go,
                    CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            Object = go;
            
            line.Color = DrawingUtils.ColorLines;
            line.EndCaps = LineEndCap.Round;
            line.Thickness = ViewSettings.LineWidth * scale;

            var trap = go.AddComponentOnNewChild<SpriteRenderer>("Trap Sprite", out _, Vector2.zero);
            trap.sprite = PrefabUtilsEx.GetObject<Sprite>("views", "trap_react");
            trap.sortingOrder = DrawingUtils.GetBlockSortingOrder(Props.Type);
            (line.Start, line.End) = GetTrapPosRotAndLineEdges();
            trap.color = DrawingUtils.ColorLines;
            trap.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
            var dir = Props.Directions.First().ToVector2();
            var trapTr = trap.transform;
            trapTr.SetLocalPosXY(dir * scale * StartPos);
            trapTr.localScale = Vector3.one * scale * 0.95f;
            
            var maskGo = PrefabUtilsEx.InitPrefab(
                ContainersGetter.MazeItemsContainer, "views", "turret_bullet_mask");
            var mask = maskGo.GetCompItem<SpriteMask>("mask");
            maskGo.SetParent(Object);
            maskGo.transform.SetLocalPosXY(Vector2.zero);
            mask.enabled = true;
            mask.transform.localScale = Vector3.one * scale * 0.8f;
            mask.isCustomRangeActive = true;
            mask.frontSortingOrder = DrawingUtils.GetBlockSortingOrder(Props.Type);

            m_Line = line;
            m_Trap = trap;
            Proceeding = true;
        }

        private IEnumerator HandlePreReact()
        {
            var scale = CoordinateConverter.GetScale();
            var dir = Props.Directions.First().ToVector2();
            yield return Coroutines.Lerp(
                StartPos,
                MiddlePos,
                0.1f,
                _Progress => SetReactProgress(dir, scale, _Progress),
                GameTimeProvider
            );
        }

        private IEnumerator HandleReact()
        {
            var scale = CoordinateConverter.GetScale();
            var dir = Props.Directions.First().ToVector2();
            yield return Coroutines.Lerp(
                MiddlePos,
                FinalPos,
                0.05f,
                _Progress => SetReactProgress(dir, scale, _Progress),
                GameTimeProvider
            );
        }

        private IEnumerator HandlePostReact()
        {
            var scale = CoordinateConverter.GetScale();
            var dir = Props.Directions.First().ToVector2();
            yield return Coroutines.Lerp(
                FinalPos,
                StartPos,
                ModelSettings.trapAfterReactTime,
                _Progress => SetReactProgress(dir, scale, _Progress),
                GameTimeProvider
            );
        }

        private Tuple<Vector2, Vector2> GetTrapPosRotAndLineEdges()
        {
            var dir = Props.Directions.First().ToVector2();
            var dirOrth = new Vector2(dir.y, dir.x);
            var A = dir * 0.35f;
            var B = A + dirOrth * 0.45f;
            var C = A - dirOrth * 0.45f;
            B *= CoordinateConverter.GetScale();
            C *= CoordinateConverter.GetScale();
            return new Tuple<Vector2, Vector2>(B, C);
        }

        private void SetReactProgress(Vector2 _Direction, float _Scale, float _Progress)
        {
            m_Progress = _Progress;
            m_Trap.transform.SetLocalPosXY(_Direction * _Scale * _Progress);
        }

        private void CheckForCharacterDeath()
        {
            if (m_Progress < 0.3f)
                return;
            var dir = Props.Directions.First();
            var pos = Props.Position;
            var itemPos = (dir + pos).ToVector2();
            var cPos = Character.IsMoving ? 
                Character.MovingInfo.PrecisePosition : Character.Position.ToVector2();
            if (Vector2.Distance(cPos, itemPos) + RazorMazeUtils.Epsilon > 1f) 
                return;
            Character.RaiseDeath();
        }
        
        #endregion
    }
}