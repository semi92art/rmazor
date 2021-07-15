﻿using System;
using System.Collections;
using System.Linq;
using Extensions;
using GameHelpers;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Utils;
using Shapes;
using TimeProviders;
using UnityEngine;
using UnityGameLoopDI;
using Utils;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItemTrapReact : IViewMazeItem
    {
        void OnTrapReact(MazeItemTrapReactEventArgs _Args);
    }
    
    public class ViewMazeItemTrapReact : ViewMazeItemBase, IViewMazeItemTrapReact, IUpdateTick
    {

        #region nonpublic members
        
        private float StartPos => 0.1f;
        private float MiddlePos => 0.2f;
        private float FinalPos => 0.7f;
        private Line m_Line;
        private SpriteRenderer m_Trap;
        private bool m_Rotate;
        
        #endregion
        
        #region inject
        
        private ViewSettings ViewSettings { get; }
        private IGameTimeProvider GameTimeProvider { get; }

        public ViewMazeItemTrapReact(
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            ViewSettings _ViewSettings,
            IGameTimeProvider _GameTimeProvider)
            : base(_CoordinateConverter, _ContainersGetter)
        {
            ViewSettings = _ViewSettings;
            GameTimeProvider = _GameTimeProvider;
        }
        
        #endregion
        
        #region api
        
        public override bool Proceeding
        {
            get => m_Rotate;
            set
            {
                if (value) StartRotation();
                else StopRotation();
            }
        }
        
        public void UpdateTick()
        {
            if (!m_Rotate)
                return;
            float rotSpeed = ViewSettings.MovingTrapRotationSpeed * Time.deltaTime; 
            m_Trap.transform.Rotate(Vector3.forward * rotSpeed);
        }
        
        public object Clone() => new ViewMazeItemTrapReact(
            CoordinateConverter, ContainersGetter, ViewSettings, GameTimeProvider);
        
        public void OnTrapReact(MazeItemTrapReactEventArgs _Args)
        {
            IEnumerator coroutine = null;
            switch (_Args.Stage)
            {
                case TrapsReactProceeder.StageIdle: break;
                case TrapsReactProceeder.StagePreReact:   coroutine = HandlePreReact(); break;
                case TrapsReactProceeder.StageReact:      coroutine = HandleReact(); break;
                case TrapsReactProceeder.StageAfterReact: coroutine = HandlePostReact(); break;
                default: throw new ArgumentOutOfRangeException(nameof(_Args.Stage), $"Stage {_Args.Stage} was not implemented");
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
            
            line.Color = ViewUtils.ColorLines;
            line.EndCaps = LineEndCap.Round;
            line.Thickness = ViewSettings.LineWidth * scale;

            var trap = go.AddComponentOnNewChild<SpriteRenderer>("Trap Sprite", out _, Vector2.zero);
            trap.sprite = PrefabUtilsEx.GetObject<Sprite>("views", "trap_react");
            trap.sortingOrder = ViewUtils.GetBlockSortingOrder(Props.Type);
            (line.Start, line.End) = GetTrapPosRotAndLineEdges();
            trap.color = ViewUtils.ColorLines;
            trap.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
            var dir = Props.Directions.First().ToVector2();
            var trapTr = trap.transform;
            trapTr.SetLocalPosXY(dir * scale * StartPos);
            trapTr.localScale = Vector3.one * scale;
            
            var maskGo = PrefabUtilsEx.InitPrefab(
                ContainersGetter.MazeItemsContainer, "views", "turret_bullet_mask");
            var mask = maskGo.GetCompItem<SpriteMask>("mask");
            maskGo.SetParent(Object);
            maskGo.transform.SetLocalPosXY(Vector2.zero);
            mask.enabled = true;
            mask.transform.localScale = Vector3.one * scale * 0.8f;
            mask.isCustomRangeActive = true;
            mask.frontSortingOrder = ViewUtils.GetBlockSortingOrder(Props.Type);

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
                _Progress => m_Trap.transform.SetLocalPosXY(dir * scale * _Progress),
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
                _Progress => m_Trap.transform.SetLocalPosXY(dir * scale * _Progress),
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
                0.1f,
                _Progress => m_Trap.transform.SetLocalPosXY(dir * scale * _Progress),
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
        
        private void StartRotation()
        {
            m_Rotate = true;
        }

        private void StopRotation()
        {
            m_Rotate = false;
        }

        #endregion
    }
}