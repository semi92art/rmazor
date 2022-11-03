using System;
using System.Collections;
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
using RMAZOR.Views.Common;
using RMAZOR.Views.Common.Additional_Background;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;
using UnityEngine.Rendering;

namespace RMAZOR.Views.MazeItems
{
    public interface IViewMazeItemTrapReact : IViewMazeItem
    {
        void OnTrapReact(MazeItemTrapReactEventArgs _Args);
    }

    public class ViewMazeItemTrapReactSpikes : ViewMazeItemBase, IViewMazeItemTrapReact, IUpdateTick
    {
        #region constants

        private const float StartPos  = 0.1f;
        private const float MiddlePos = 0.2f;
        private const float FinalPos  = 0.7f;

        #endregion
        
        #region nonpublic members
        
        private static readonly Dictionary<V2Int, Rectangle> Masks        = new Dictionary<V2Int, Rectangle>();
        private static readonly int                          StencilRefId = Shader.PropertyToID("_StencilRef");
        
        private AudioClipArgs AudioClipInfoTrapReactOut => 
            new AudioClipArgs(
                "trap_react_out", 
                EAudioClipType.GameSound, 
                _Id: GetHashCode().ToString());
        
        protected override      string ObjectName => "Trap React Spikes Block";

        private float          m_Progress;
        private Line           m_Line;
        private SpriteRenderer m_Trap;
        
        #endregion

        #region static ctor

        static ViewMazeItemTrapReactSpikes()
        {
            Masks.Clear();
        }

        #endregion
        
        #region inject

        private ModelSettings                 ModelSettings        { get; }
        private IViewMazeAdditionalBackground AdditionalBackground { get; }

        private ViewMazeItemTrapReactSpikes(
            ViewSettings                  _ViewSettings,
            ModelSettings                 _ModelSettings,
            IModelGame                    _Model,
            ICoordinateConverter    _CoordinateConverter,
            IContainersGetter             _ContainersGetter,
            IViewGameTicker               _GameTicker,
            IRendererAppearTransitioner   _Transitioner,
            IManagersGetter               _Managers,
            IColorProvider                _ColorProvider,
            IViewInputCommandsProceeder   _CommandsProceeder,
            IViewMazeAdditionalBackground _AdditionalBackground)
            : base(
                _ViewSettings,
                _Model,
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker,
                _Transitioner,
                _Managers,
                _ColorProvider,
                _CommandsProceeder)
        {
            ModelSettings        = _ModelSettings;
            AdditionalBackground = _AdditionalBackground;
        }
        
        #endregion
        
        #region api
        
        public override Component[] Renderers => new Component[] {m_Line, m_Trap};

        public override bool ActivatedInSpawnPool
        {
            get => base.ActivatedInSpawnPool;
            set
            {
                base.ActivatedInSpawnPool = value;
                GetMask().enabled = value;
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
            ColorProvider,
            CommandsProceeder,
            AdditionalBackground);
        
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
                case ModelCommonData.StageIdle:
                    break;
                case ModelCommonData.TrapReactStagePreReact:
                    coroutine = HandlePreReact(); 
                    break;
                case ModelCommonData.TrapReactStageReact:
                    Managers.AudioManager.PlayClip(AudioClipInfoTrapReactOut);
                    coroutine = HandleReact();
                    break;
                case ModelCommonData.TrapReactStageAfterReact:
                    coroutine = HandlePostReact();
                    break;
                default: throw new ArgumentOutOfRangeException(
                    nameof(_Args.Stage), $"Stage {_Args.Stage} was not implemented");
            }
            Cor.Run(coroutine);
        }

        #endregion

        #region nonpublic methods

        private Rectangle GetMask()
        {
            return Masks.GetSafe(Props.Position, out bool _);
        }

        private void SetMask(Rectangle _Mask)
        {
            Masks.SetSafe(Props.Position, _Mask);
        }
        
        protected override void InitShape()
        {
            var col = ColorProvider.GetColor(ColorIds.MazeItem1);
            var line = Object.gameObject.AddComponentOnNewChild<Line>("Trap React Item", out _);
            line.EndCaps = LineEndCap.Round;
            var trap = Object.AddComponentOnNewChild<SpriteRenderer>("Trap Sprite", out _);
            line.Color = col;
            trap.sprite = Managers.PrefabSetManager.GetObject<Sprite>(
                "views", "trap_react_spikes_sprite");
            trap.material = Managers.PrefabSetManager.InitObject<Material>(
                "materials", "trap_react_spikes_material");
            trap.sortingOrder = GetSortingOrder();
            trap.color = col;
            trap.maskInteraction = SpriteMaskInteraction.None;
            AdditionalBackground.GroupsCollected += SetStencilRefValues;
            (m_Trap, m_Line) = (trap, line);
        }
        
        protected override void UpdateShape()
        {
            var mask = GetMask();
            if (mask.IsNull())
            {
                mask = ContainersGetter.GetContainer(ContainerNames.MazeItems)
                    .AddComponentOnNewChild<Rectangle>("Trap React Mask", out GameObject _)
                    .SetBlendMode(ShapesBlendMode.Subtractive)
                    .SetRenderQueue(-1)
                    .SetSortingOrder(SortingOrders.AdditionalBackgroundTexture)
                    .SetZTest(CompareFunction.Less)
                    .SetStencilComp(CompareFunction.Greater)
                    .SetStencilOpPass(StencilOp.Replace)
                    .SetColor(new Color(0f, 0f, 0f, 100f / 255f));
                mask.enabled = false;
                SetMask(mask);
            }
            mask.transform
                .SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position))
                .SetPosZ(-0.1f);
            (m_Line.Start, m_Line.End) = GetTrapPosRotAndLineEdges();
            float scale = CoordinateConverter.Scale;
            Vector2 dir = Props.Directions.First();
            var trapTr = m_Trap.transform;
            trapTr.localRotation = Quaternion.Euler(0f, 0f, GetTrapAngle(Props.Directions.First()));
            trapTr.SetLocalPosXY(dir * scale * StartPos);
            trapTr.localScale = Vector3.one * scale * 0.95f;
            m_Line.Thickness = 9f * 0.01f * scale;
            mask.Width = mask.Height = scale * 0.8f;
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId != ColorIds.MazeItem1) 
                return;
            m_Line.Color = _Color;
            m_Trap.color = _Color;
        }
        
        private void SetStencilRefValues(List<PointsGroupArgs> _Groups)
        {
            int GetGroupIndexByPoint()
            {
                if (_Groups == null)
                    return -2;
                foreach (var group in _Groups
                    .Where(_Group => _Group.Points.Contains(Props.Position + Props.Directions.First())))
                {
                    return group.GroupIndex;
                }
                return -1;
            }
            int stencilRef = GetGroupIndexByPoint();
            if (stencilRef < 0)
                return;
            GetMask().StencilRefID = Convert.ToByte(stencilRef + 1);
            m_Trap.sharedMaterial.SetFloat(StencilRefId, stencilRef);
        }

        private static  float GetTrapAngle(V2Int _Direction)
        {
            if (_Direction == V2Int.Left)
                return 90f;
            if (_Direction == V2Int.Up)
                return 0f;
            if (_Direction == V2Int.Right)
                return 270f;
            return _Direction == V2Int.Down ? 180f : 0f;
        }

        private IEnumerator HandlePreReact()
        {
            float scale = CoordinateConverter.Scale;
            Vector2 dir = Props.Directions.First();
            yield return Cor.Lerp(
                GameTicker,
                0.1f,
                StartPos,
                MiddlePos,
                _Progress => SetReactProgress(dir, scale, _Progress)
            );
        }

        private IEnumerator HandleReact()
        {
            float scale = CoordinateConverter.Scale;
            Vector2 dir = Props.Directions.First();
            yield return Cor.Lerp(
                GameTicker,
                0.02f,
                MiddlePos,
                FinalPos,
                _Progress => SetReactProgress(dir, scale, _Progress)
            );
        }

        private IEnumerator HandlePostReact()
        {
            float scale = CoordinateConverter.Scale;
            Vector2 dir = Props.Directions.First();
            yield return Cor.Lerp(
                GameTicker,
                0.05f,
                FinalPos,
                StartPos,
                _Progress => SetReactProgress(dir, scale, _Progress)
            );
        }

        private Tuple<Vector2, Vector2> GetTrapPosRotAndLineEdges()
        {
            Vector2 dir = Props.Directions.First();
            var dirOrth = new Vector2(dir.y, dir.x); //-V3066
            var a = dir * 0.35f;
            var b = a + dirOrth * 0.45f;
            var c = a - dirOrth * 0.45f;
            b *= CoordinateConverter.Scale;
            c *= CoordinateConverter.Scale;
            return new Tuple<Vector2, Vector2>(b, c);
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
            var itemPos = dir + pos;
            var character = Model.Character;
            if (character.IsMoving)
            {
                var charInfo = Model.Character.MovingInfo;
                var path = RmazorUtils.GetFullPath(
                    (V2Int) charInfo.PreviousPrecisePosition, (V2Int) charInfo.PrecisePosition);
                if (path.Contains(itemPos))
                    CommandsProceeder.RaiseCommand(EInputCommand.KillCharacter, 
                        new object[] { CoordinateConverter.ToLocalMazeItemPosition(itemPos) });
            }
            else if (itemPos == character.Position) 
            {
                CommandsProceeder.RaiseCommand(EInputCommand.KillCharacter, null);
            }
        }

        #endregion
    }
}