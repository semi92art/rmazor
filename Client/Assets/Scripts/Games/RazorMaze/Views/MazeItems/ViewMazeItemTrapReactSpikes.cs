using System;
using System.Collections;
using System.Linq;
using DI.Extensions;
using Entities;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.Utils;
using Shapes;
using Ticker;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItemTrapReact : IViewMazeItem
    {
        void OnTrapReact(MazeItemTrapReactEventArgs _Args);
    }

    public class ViewMazeItemTrapReactSpikes : ViewMazeItemBase, IViewMazeItemTrapReact, IUpdateTick
    {
        #region constants

        private const float StartPos = 0.1f;
        private const float MiddlePos = 0.2f;
        private const float FinalPos = 0.7f;
        private const string SoundClipNameTrapReactOut = "trap_react_out"; 
        
        #endregion
        
        #region nonpublic members
        
        private float m_Progress;
        
        #endregion
        
        #region shapes

        protected override string ObjectName => "Trap React Spikes Block";
        private Line m_Line;
        private SpriteRenderer m_Trap;
        private SpriteMask m_Mask;
        
        #endregion
        
        #region inject

        private ModelSettings ModelSettings { get; }

        public ViewMazeItemTrapReactSpikes(
            ViewSettings _ViewSettings,
            ModelSettings _ModelSettings,
            IModelGame _Model,
            IMazeCoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IGameTicker _GameTicker,
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
                _ColorProvider)
        {
            ModelSettings = _ModelSettings;
        }
        
        #endregion
        
        #region api
        
        public override Component[] Shapes => new Component[] {m_Line, m_Trap};

        public override bool ActivatedInSpawnPool
        {
            get => base.ActivatedInSpawnPool;
            set
            {
                base.ActivatedInSpawnPool = value;
                m_Mask.enabled = value;
            }
        }

        public override object Clone() => new ViewMazeItemTrapReactSpikes(
            ViewSettings,
            ModelSettings,
            Model, 
            CoordinateConverter,
            ContainersGetter,
            GameTicker,
            Transitioner,
            Managers,
            ColorProvider);
        
        public void UpdateTick()
        {
            if (!Initialized || !ActivatedInSpawnPool)
                return;
            if (ProceedingStage != EProceedingStage.ActiveAndWorking)
                return;
            CheckForCharacterDeath();
        }

        public void OnTrapReact(MazeItemTrapReactEventArgs _Args)
        {
            IEnumerator coroutine = null;
            switch (_Args.Stage)
            {
                case ItemsProceederBase.StageIdle: break;
                case TrapsReactProceeder.StagePreReact:   coroutine = HandlePreReact(); break;
                case TrapsReactProceeder.StageReact:
                    Managers.Notify(_SM => 
                        _SM.PlayClip(SoundClipNameTrapReactOut, _Tags: $"{_Args.Info}"));
                    coroutine = HandleReact(); break;
                case TrapsReactProceeder.StageAfterReact: coroutine = HandlePostReact(); break;
                default: throw new ArgumentOutOfRangeException(
                    nameof(_Args.Stage), $"Stage {_Args.Stage} was not implemented");
            }
            Coroutines.Run(coroutine);
        }

        #endregion

        #region nonpublic methods
        
        protected override void InitShape()
        {
            var line = Object.gameObject.AddComponentOnNewChild<Line>("Trap React Item", out _);
            line.Color = ColorProvider.GetColor(ColorIds.MazeItem1);
            line.EndCaps = LineEndCap.Round;
            var trap = Object.AddComponentOnNewChild<SpriteRenderer>("Trap Sprite", out _);
            trap.sprite = PrefabUtilsEx.GetObject<Sprite>("views", "trap_react_spikes");
            trap.sortingOrder = SortingOrders.GetBlockSortingOrder(Props.Type);
            trap.color = ColorProvider.GetColor(ColorIds.MazeItem1);
            trap.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
            var maskGo = PrefabUtilsEx.InitPrefab(
                Object.transform, "views", "turret_bullet_mask");
            maskGo.name = "Trap React Mask";
            var mask = maskGo.GetCompItem<SpriteMask>("mask");
            maskGo.SetParent(Object);
            maskGo.transform.SetLocalPosXY(Vector2.zero);
            mask.enabled = true;
            mask.isCustomRangeActive = true;
            mask.frontSortingOrder = SortingOrders.GetBlockSortingOrder(Props.Type);
            (m_Trap, m_Line, m_Mask) = (trap, line, mask);
        }

        protected override void UpdateShape()
        {
            (m_Line.Start, m_Line.End) = GetTrapPosRotAndLineEdges();
            var scale = CoordinateConverter.Scale;
            var dir = Props.Directions.First().ToVector2();
            var trapTr = m_Trap.transform;
            trapTr.localRotation = Quaternion.Euler(0f, 0f, GetTrapAngle(Props.Directions.First()));
            trapTr.SetLocalPosXY(dir * scale * StartPos);
            trapTr.localScale = Vector3.one * scale * 0.95f;
            m_Line.Thickness = ViewSettings.LineWidth * scale;
            m_Mask.transform.localScale = Vector3.one * scale * 0.8f;
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.MazeItem1)
            {
                m_Line.Color = _Color;
                m_Trap.color = _Color;
            }
        }

        private float GetTrapAngle(V2Int _Direction)
        {
            if (_Direction == V2Int.left)
                return 90f;
            if (_Direction == V2Int.up)
                return 0f;
            if (_Direction == V2Int.right)
                return 270f;
            if (_Direction == V2Int.down)
                return 180f;
            return 0f;
        }

        private IEnumerator HandlePreReact()
        {
            var scale = CoordinateConverter.Scale;
            var dir = Props.Directions.First().ToVector2();
            yield return Coroutines.Lerp(
                StartPos,
                MiddlePos,
                0.1f,
                _Progress => SetReactProgress(dir, scale, _Progress),
                GameTicker
            );
        }

        private IEnumerator HandleReact()
        {
            var scale = CoordinateConverter.Scale;
            var dir = Props.Directions.First().ToVector2();
            yield return Coroutines.Lerp(
                MiddlePos,
                FinalPos,
                0.02f,
                _Progress => SetReactProgress(dir, scale, _Progress),
                GameTicker
            );
        }

        private IEnumerator HandlePostReact()
        {
            var scale = CoordinateConverter.Scale;
            var dir = Props.Directions.First().ToVector2();
            yield return Coroutines.Lerp(
                FinalPos,
                StartPos,
                0.05f,
                _Progress => SetReactProgress(dir, scale, _Progress),
                GameTicker
            );
        }

        private Tuple<Vector2, Vector2> GetTrapPosRotAndLineEdges()
        {
            var dir = Props.Directions.First().ToVector2();
            var dirOrth = new Vector2(dir.y, dir.x);
            var A = dir * 0.35f;
            var B = A + dirOrth * 0.45f;
            var C = A - dirOrth * 0.45f;
            B *= CoordinateConverter.Scale;
            C *= CoordinateConverter.Scale;
            return new Tuple<Vector2, Vector2>(B, C);
        }

        private void SetReactProgress(Vector2 _Direction, float _Scale, float _Progress)
        {
            m_Progress = _Progress;
            m_Trap.transform.SetLocalPosXY(_Direction * _Scale * _Progress);
        }

        private void CheckForCharacterDeath()
        {
            if (!Model.Character.Alive)
                return;
            if (Model.PathItemsProceeder.AllPathsProceeded)
                return;
            if (Model.LevelStaging.LevelStage == ELevelStage.Finished)
                return;
            if (m_Progress < 0.3f)
                return;
            var dir = Props.Directions.First();
            var pos = Props.Position;
            var itemPos = (dir + pos).ToVector2();
            var character = Model.Character;
            var cPos = character.IsMoving ? 
                character.MovingInfo.PrecisePosition : character.Position.ToVector2();
            if (Vector2.Distance(cPos, itemPos) + RazorMazeUtils.Epsilon > 1f) 
                return;
            Model.LevelStaging.KillCharacter();
        }
        
        #endregion
    }
}