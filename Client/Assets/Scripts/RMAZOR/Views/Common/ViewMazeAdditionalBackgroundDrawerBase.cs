using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Enums;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.SpawnPools;
using RMAZOR.Models;
using RMAZOR.Views.Helpers;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;
using UnityEngine.Rendering;

namespace RMAZOR.Views.Common
{
    public interface IViewMazeAdditionalBackgroundDrawer : IInit, IAppear
    {
        void Draw(List<PointsGroupArgs> _Groups);
    }
    
    public abstract class ViewMazeAdditionalBackgroundDrawerBase
        : InitBase, IViewMazeAdditionalBackgroundDrawer
    {
        #region constants

        private const float AdditionalScale       = 1.005f;
        private const int   BordersPoolCount      = 100;
        private const int   CornersPoolCount      = 20;
        private const int   MasksPoolCount        = 10;
        private const int   TextureTypesCount     = 3;
        private const int   TextureRenderersCount = 3;
        private const float BorderRelativeIndent  = 0.5f;

        #endregion
        
        #region nonpublic members

        private static readonly int ColorId      = Shader.PropertyToID("_Color");
        private static readonly int StencilRefId = Shader.PropertyToID("_StencilRef");

        private Transform Container => ContainersGetter.GetContainer(ContainerNames.MazeItems);

        private readonly RendererSpawnPool<SpriteRenderer> m_TextureRenderers = new RendererSpawnPool<SpriteRenderer>();
        private readonly List<Sprite>                      m_TextureSprites   = new List<Sprite>();

        private readonly BehavioursSpawnPool<Line>      m_Borders              = new BehavioursSpawnPool<Line>();
        private readonly BehavioursSpawnPool<Disc>      m_Corners              = new BehavioursSpawnPool<Disc>();
        private readonly BehavioursSpawnPool<Rectangle> m_TextureRendererMasks = new BehavioursSpawnPool<Rectangle>();

        #endregion
        
        #region inject

        private ViewSettings                  ViewSettings        { get; }
        private IModelGame                    Model               { get; }
        private IColorProvider                ColorProvider       { get; }
        private IContainersGetter             ContainersGetter    { get; }
        private IMazeCoordinateConverter      CoordinateConverter { get; }
        private IPrefabSetManager             PrefabSetManager    { get; }
        private IViewBetweenLevelTransitioner Transitioner        { get; }

        protected ViewMazeAdditionalBackgroundDrawerBase(
            ViewSettings                  _ViewSettings,
            IModelGame                    _Model,
            IColorProvider                _ColorProvider,
            IContainersGetter             _ContainersGetter,
            IMazeCoordinateConverter      _CoordinateConverter,
            IPrefabSetManager             _PrefabSetManager,
            IViewBetweenLevelTransitioner _Transitioner)
        {
            Model               = _Model;
            ViewSettings        = _ViewSettings;
            ColorProvider       = _ColorProvider;
            ContainersGetter    = _ContainersGetter;
            CoordinateConverter = _CoordinateConverter;
            PrefabSetManager    = _PrefabSetManager;
            Transitioner        = _Transitioner;
        }

        #endregion
        
        #region api
        
        public EAppearingState AppearingState { get; private set; }

        public override void Init()
        {
            ColorProvider.ColorChanged += OnColorChanged;
            InitPools();
            InitTextures();
            base.Init();
        }
        
        public void Draw(List<PointsGroupArgs> _Groups)
        {
            DeactivateAll();
            foreach (var group in _Groups)
                DrawBordersForGroup(group);
            foreach (var group in _Groups)
                DrawCornersForGroup(group);
            foreach (var group in _Groups)
                DrawTextureForGroup(group);
            foreach (var group in _Groups)
                DrawHolesForGroup(group);
        }

        public void Appear(bool _Appear)
        {
            AppearingState = _Appear ? EAppearingState.Appearing : EAppearingState.Dissapearing;
            var color = ColorProvider.GetColor(ColorIds.Main);
            var dict = new Dictionary<IEnumerable<Component>, Func<Color>>
            {
                {m_Borders.GetAllActiveItems(), () => color},
                {m_Corners.GetAllActiveItems(), () => color},
                {m_TextureRenderers.GetAllActiveItems(), () => color.SetA(0.3f)}
            };
            Transitioner.DoAppearTransition(_Appear, dict, _OnFinish: () =>
            {
                AppearingState = _Appear ? EAppearingState.Appeared : EAppearingState.Dissapeared;
            });
        }
        
        #endregion

        #region nonpublic members
        
        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId != ColorIds.Main)
                return;
            m_TextureRenderers.GetAllActiveItems().ForEach(_R => _R.color = _Color.SetA(_R.color.a));
            var activeBorders = m_Borders.GetAllActiveItems();
            foreach (var border in activeBorders)
                border.Color = _Color.SetA(border.Color.a);
            var activeCorners = m_Corners.GetAllActiveItems();
            foreach (var corner in activeCorners)
                corner.Color = _Color.SetA(corner.Color.a);
        }
        
        private void InitPools()
        {
            InitBordersPool();
            InitCornersPool();
        }

        private void InitBordersPool()
        {
            for (int i = 0; i < BordersPoolCount; i++)
            {
                var go = new GameObject("Additional Background Border");
                go.SetParent(Container);
                var border = go.AddComponent<Line>();
                border.SortingOrder = SortingOrders.AdditionalBackgroundBorder;
                border.DashSpace = DashSpace.FixedCount;
                border.DashSnap = DashSnapping.Off;
                border.DashType = DashType.Rounded;
                border.EndCaps = LineEndCap.None;
                m_Borders.Add(border);
            }
            m_Borders.DeactivateAll(true);
        }

        private void InitCornersPool()
        {
            for (int i = 0; i < CornersPoolCount; i++)
            {
                var go = new GameObject("Additional Background Corner");
                go.SetParent(Container);
                var corner = go.AddComponent<Disc>();
                corner.SortingOrder = SortingOrders.AdditionalBackgroundCorner;
                corner.Type = DiscType.Arc;
                corner.ArcEndCaps = ArcEndCap.Round;
                m_Corners.Add(corner);
            }
            m_Corners.DeactivateAll(true);
        }

        private void InitTextures()
        {
            var go = PrefabSetManager.InitPrefab(
                Container,
                "views",
                "additional_background");
            for (int i = 0; i < TextureRenderersCount; i++)
            {
                var renderer = go.GetCompItem<SpriteRenderer>("renderer");
                renderer.sortingOrder = SortingOrders.AdditionalBackgroundPolygon;
                m_TextureRenderers.Add(renderer);
            }
            for (int i = 1; i <= TextureTypesCount; i++)
            {
                var textureSprite = PrefabSetManager.GetObject<Sprite>(
                    "views", 
                    $"additional_background_texture_{i}");
                m_TextureSprites.Add(textureSprite);
            }
            for (int i = 1; i <= MasksPoolCount; i++)
            {
                var mask = Container.gameObject.AddComponentOnNewChild<Rectangle>(
                    "Additional Texture Mask", 
                    out GameObject _);
                mask.enabled = false;
                mask.BlendMode = ShapesBlendMode.Subtractive;
                mask.RenderQueue = -1;
                mask.SortingOrder = SortingOrders.AdditionalBackgroundPolygon;
                mask.ZTest = CompareFunction.Less;
                mask.StencilComp = CompareFunction.Greater;
                mask.StencilOpPass = StencilOp.Replace;
                mask.Color = new Color(0f, 0f, 0f, 1f / 255f);
                mask.Type = Rectangle.RectangleType.RoundedSolid;
                m_TextureRendererMasks.Add(mask);
            }
        }
        
        private void DeactivateAll()
        {
            m_TextureRenderers    .DeactivateAll();
            m_TextureRendererMasks.DeactivateAll();
            m_Borders             .DeactivateAll();
            m_Corners             .DeactivateAll();
        }

        private void DrawBordersForGroup(PointsGroupArgs _Group)
        {
            int minX = _Group.Points.Min(_P => _P.X);
            int minY = _Group.Points.Min(_P => _P.Y);
            int maxX = _Group.Points.Max(_P => _P.X);
            int maxY = _Group.Points.Max(_P => _P.Y);
            var xPositions = Enumerable.Range(minX, maxX + 1 - minX).ToList();
            var yPositions = Enumerable.Range(minY, maxY + 1 - minY).ToList();
            var leftBorderPoints = yPositions
                .Select(_Y => new V2Int(minX, _Y))
                .ToList();
            var rightBorderPoints = yPositions
                .Select(_Y => new V2Int(maxX, _Y))
                .ToList();
            var bottomBorderPoints = xPositions
                .Select(_X => new V2Int(_X, minY))
                .ToList();
            var topBorderPoints = xPositions
                .Select(_X => new V2Int(_X, maxY))
                .ToList();
            V2Int first, last;
            (first, last) = (leftBorderPoints.First(), leftBorderPoints.Last());
            foreach (var point in leftBorderPoints)
                DrawBorder(point, EMazeMoveDirection.Left, point == first, point == last);
            (first, last) = (rightBorderPoints.First(), rightBorderPoints.Last());
            foreach (var point in rightBorderPoints)
                DrawBorder(point, EMazeMoveDirection.Right, point == last, point == first);
            (first, last) = (bottomBorderPoints.First(), bottomBorderPoints.Last());
            foreach (var point in bottomBorderPoints)
                DrawBorder(point, EMazeMoveDirection.Down, point == last, point == first);
            (first, last) = (topBorderPoints.First(), topBorderPoints.Last());
            foreach (var point in topBorderPoints)
                DrawBorder(point, EMazeMoveDirection.Up, point == first, point == last);
        }

        private void DrawCornersForGroup(PointsGroupArgs _Group)
        {
            int minX = _Group.Points.Min(_P => _P.X);
            int minY = _Group.Points.Min(_P => _P.Y);
            int maxX = _Group.Points.Max(_P => _P.X);
            int maxY = _Group.Points.Max(_P => _P.Y);
            var pos = new V2Int(minX, minY);
            DrawCorner(pos, false, false, true);
            pos = new V2Int(minX, maxY);
            DrawCorner(pos, false, true, true);
            pos = new V2Int(maxX, maxY);
            DrawCorner(pos, true, true, true);
            pos = new V2Int(maxX, minY);
            DrawCorner(pos, true, false, true);
        }

        private void DrawTextureForGroup(PointsGroupArgs _Group)
        {
            int minX = _Group.Points.Min(_P => _P.X);
            int minY = _Group.Points.Min(_P => _P.Y);
            int maxX = _Group.Points.Max(_P => _P.X);
            int maxY = _Group.Points.Max(_P => _P.Y);
            float scale = CoordinateConverter.Scale;
            int group = RazorMazeUtils.GetGroupIndex(Model.LevelStaging.LevelIndex);
            int textureTypeIdx = group % TextureTypesCount;
            var renderer = m_TextureRenderers.FirstInactive;
            m_TextureRenderers.Activate(renderer);
            renderer.sprite = m_TextureSprites[textureTypeIdx];
            renderer.transform.localScale = Vector3.one * scale * 0.5f;
            var centerRaw = new Vector2(minX + (maxX - minX) * 0.5f, minY + (maxY - minY) * 0.5f);
            var center = CoordinateConverter.ToLocalMazeItemPosition(centerRaw);
            renderer.transform.SetLocalPosXY(center);
            renderer.sharedMaterial.SetFloat(StencilRefId, _Group.GroupIndex);
            var mask = m_TextureRendererMasks.FirstInactive;
            m_TextureRendererMasks.Activate(mask);
            mask.transform.SetLocalPosXY(center);
            mask.Width = (maxX - minX) * scale + 2f * scale * (0.5f + BorderRelativeIndent);
            mask.Height = (maxY - minY) * scale + 2f * scale * (0.5f + BorderRelativeIndent);
            mask.StencilRefID = Convert.ToByte(_Group.GroupIndex);
            mask.CornerRadius = ViewSettings.CornerRadius * scale;
        }

        protected abstract void DrawHolesForGroup(PointsGroupArgs _Group);
        
        private void DrawBorder(V2Int _Position, EMazeMoveDirection _Side, bool _StartLimit, bool _EndLimit)
        {
            var border = m_Borders.FirstInactive;
            border.Thickness = ViewSettings.LineWidth * CoordinateConverter.Scale * 0.5f;
            (border.Start, border.End) = GetBorderPointsAndDashed(_Position, _Side, _StartLimit, _EndLimit);
            border.transform.position = ContainersGetter.GetContainer(ContainerNames.MazeItems).transform.position;
            border.name = _Side + " " + "border";
            m_Borders.Activate(border);
        }
        
        private void DrawCorner(
            V2Int _Position,
            bool _Right,
            bool _Up,
            bool _Inner)
        {
            var corner = m_Corners.FirstInactive;
            // ReSharper disable once PossibleNullReferenceException
            corner.transform.position = ContainersGetter.GetContainer(ContainerNames.MazeItems).transform.position; //-V3080
            corner.transform.PlusLocalPosXY(GetCornerCenter(_Position, _Right, _Up));
            corner.Radius = ViewSettings.CornerRadius * CoordinateConverter.Scale;
            corner.Thickness = ViewSettings.CornerWidth * CoordinateConverter.Scale;
            var angles = GetCornerAngles(_Right, _Up, _Inner);
            corner.AngRadiansStart = Mathf.Deg2Rad * angles.x;
            corner.AngRadiansEnd = Mathf.Deg2Rad * angles.y;
            m_Corners.Activate(corner);
        }
        
        private Tuple<Vector2, Vector2> GetBorderPointsAndDashed(
            V2Int _Point,
            EMazeMoveDirection _Side, 
            bool _StartLimit, 
            bool _EndLimit)
        {
            Vector2 start, end;
            Vector2 pos = _Point;
            float cr = ViewSettings.CornerRadius;
            var up = Vector2.up;
            var down = Vector2.down;
            var left = Vector2.left;
            var right = Vector2.right;
            var zero = Vector2.zero;
            const float bIndent = BorderRelativeIndent;
            switch (_Side)
            {
                case EMazeMoveDirection.Left:
                    start = pos + left * bIndent + (left + down) * 0.5f + (_StartLimit ? up * cr + down * bIndent : zero);
                    end = pos + left * bIndent + (left + up) * 0.5f + (_EndLimit ? down * cr + up * bIndent : zero);
                    break;
                case EMazeMoveDirection.Up:
                    start = pos + up * bIndent + (up + left) * 0.5f + (_StartLimit ? right * cr + left * bIndent : zero);
                    end = pos + up * bIndent + (up + right) * 0.5f + (_EndLimit ? left * cr + right * bIndent : zero);
                    break;
                case EMazeMoveDirection.Right:
                    start = pos + right * bIndent + (right + down) * 0.5f + (_EndLimit ? up * cr + down * bIndent : zero);
                    end = pos + right * bIndent + (right + up) * 0.5f + (_StartLimit ? down * cr + up * bIndent : zero);
                    break;
                case EMazeMoveDirection.Down:
                    start = pos + down * bIndent + (down + left) * 0.5f + (_EndLimit ? right * cr + left * bIndent : zero);
                    end = pos + down * bIndent + (down + right) * 0.5f + (_StartLimit ? left * cr + right * bIndent : zero);
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Side);
            }
            start = CoordinateConverter.ToLocalMazeItemPosition(start);
            end = CoordinateConverter.ToLocalMazeItemPosition(end);
            return new Tuple<Vector2, Vector2>(start, end);
        }
        
        private Vector2 GetCornerCenter(
            V2Int _Point,
            bool _Right,
            bool _Up)
        {
            float cr = ViewSettings.CornerRadius;
            float xIndent = _Right ? -1f : 1f;
            float yIndent = _Up ? -1f : 1f;
            var crVec = cr * new Vector2(xIndent, yIndent);
            var center = _Point 
                         + ((_Right ? Vector2.right : Vector2.left)
                            + (_Up ? Vector2.up : Vector2.down)) * (0.5f + BorderRelativeIndent) + crVec;
            return CoordinateConverter.ToLocalMazeItemPosition(center);
        }
        
        private static Vector2 GetCornerAngles(bool _Right, bool _Up, bool _Inner)
        {
            if (_Right)
            {
                if (_Up)
                    return _Inner ? new Vector2(0, 90) : new Vector2(180, 270);
                return _Inner ? new Vector2(270, 360) : new Vector2(90, 180);
            }
            if (_Up)
                return _Inner ? new Vector2(90, 180) : new Vector2(270, 360);
            return _Inner ? new Vector2(180, 270) : new Vector2(0, 90);
        }
        
        #endregion
    }
}