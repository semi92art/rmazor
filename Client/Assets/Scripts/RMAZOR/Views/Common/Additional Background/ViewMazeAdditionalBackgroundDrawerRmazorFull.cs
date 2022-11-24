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
using Common.Utils;
using RMAZOR.Helpers;
using RMAZOR.Models;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Common.Additional_Background
{
    public interface IViewMazeAdditionalBackgroundDrawerRmazorFull
        : IViewMazeAdditionalBackgroundDrawer { }
    
    public class ViewMazeAdditionalBackgroundDrawerRmazorFull 
        : ViewMazeAdditionalBackgroundDrawerRmazorBase,
          IViewMazeAdditionalBackgroundDrawerRmazorFull
    {
        #region constants

        private const int   BordersPoolCount      = 500;
        private const int   CornersPoolCount      = 500;
        private const int   CornersDashSize       = 4;
        private const float Corners2Alpha         = 0.5f;
        private const int   TextureRenderersCount = 3;
        
        #endregion
        
        #region nonpublic members
        
        private static readonly int StencilRefId = Shader.PropertyToID("_StencilRef");
        
        private readonly RendererSpawnPool<SpriteRenderer> m_TextureRenderers = 
            new RendererSpawnPool<SpriteRenderer>();

        private readonly BehavioursSpawnPool<Line> m_Borders  = new BehavioursSpawnPool<Line>();
        private readonly BehavioursSpawnPool<Line> m_Borders2 = new BehavioursSpawnPool<Line>();
        private readonly BehavioursSpawnPool<Disc> m_Corners  = new BehavioursSpawnPool<Disc>();
        private readonly BehavioursSpawnPool<Disc> m_Corners2 = new BehavioursSpawnPool<Disc>();

        private SpriteRenderer              m_TextureRendererBack;
        private AdditionalBackgroundInfoSet m_AdditionalBackgroundInfoSet;
        private string                      m_AdditionalBackgroundAssetNameSuffix;
        
        private float BorderScaleCoefficient => ViewSettings.additionalBackgroundType == 2 ? 1.5f : 0.5f;

        #endregion

        #region inject

        private IPrefabSetManager                    PrefabSetManager            { get; }
        private IViewMazeBackgroundTextureController BackgroundTextureController { get; }

        private ViewMazeAdditionalBackgroundDrawerRmazorFull(
            ViewSettings                         _ViewSettings,
            IColorProvider                       _ColorProvider,
            IContainersGetter                    _ContainersGetter,
            ICoordinateConverter                 _CoordinateConverter,
            IPrefabSetManager                    _PrefabSetManager,
            IRendererAppearTransitioner          _Transitioner,
            IViewMazeBackgroundTextureController _BackgroundTextureController) 
            : base (
                _ViewSettings,
                _CoordinateConverter,
                _ContainersGetter,
                _ColorProvider,
                _Transitioner)
        {
            PrefabSetManager            = _PrefabSetManager;
            BackgroundTextureController = _BackgroundTextureController;
        }

        #endregion

        #region api
        
        public override void Init()
        {
            CommonDataRmazor.AdditionalBackgroundDrawer = this;
            InitPools();
            InitTextures();
            base.Init();
        }
        
        public override void Draw(List<PointsGroupArgs> _Groups, long _LevelIndex)
        {
            DeactivatePools();
            m_TextureRendererBack.enabled = true;
            foreach (var group in _Groups)
                DrawBordersForGroup(group);
            foreach (var group in _Groups)
                DrawCornersForGroup(group);
            foreach (var group in _Groups)
                DrawTextureForGroup(group);
        }

        public override void Appear(bool _Appear)
        {
            AppearingState = _Appear ? EAppearingState.Appearing : EAppearingState.Dissapearing;
            var mainCol = ColorProvider.GetColor(ColorIds.Main);
            var back1Col = ColorProvider.GetColor(ColorIds.Background1)
                .SetA(ViewSettings.additionalBackgroundAlpha);
            var dict = new Dictionary<IEnumerable<Component>, Func<Color>>
            {
                {m_Borders.GetAllActiveItems(),  () => mainCol},
                {m_Corners.GetAllActiveItems(),  () => mainCol},
                {new[] {m_TextureRendererBack},  () => back1Col},
                {m_TextureRenderers.GetAllActiveItems(), () => mainCol}
            };
            switch (ViewSettings.additionalBackgroundType)
            {
                case 1:
                    dict.Add(m_Corners.GetAllActiveItems(),  () => mainCol);
                    break;
                case 2:
                    dict.Add(m_Corners.GetAllActiveItems(),  () => mainCol.SetA(Corners2Alpha));
                    dict.Add(m_Corners2.GetAllActiveItems(),  () => mainCol);
                    dict.Add(m_Borders2.GetAllActiveItems(), () => mainCol);
                    break;
            }
            Transitioner.DoAppearTransition(_Appear, dict,
                ViewSettings.betweenLevelTransitionTime,
                () => AppearingState = _Appear ? EAppearingState.Appeared : EAppearingState.Dissapeared);
        }

        public void SetAdditionalBackgroundSprite(string _Name)
        {
            m_TextureRenderers    .DeactivateAll();
            string additBackTextureName = _Name + "_" + m_AdditionalBackgroundAssetNameSuffix;
            var renderer = m_TextureRenderers.FirstInactive;
            m_TextureRenderers.Activate(renderer);
            renderer.sprite = m_AdditionalBackgroundInfoSet
                .FirstOrDefault(_Item => _Item.name == additBackTextureName)
                ?.sprite;
        }

        public override void Disable()
        {
            base.Disable();
            DeactivatePools();
            m_TextureRendererBack.enabled = false;
        }

        #endregion

        #region nonpublic members
        
        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            if (_ColorId == ColorIds.Background1)
                m_TextureRendererBack.color = _Color.SetA(ViewSettings.additionalBackgroundAlpha);
            if (_ColorId != ColorIds.Main)
                return;
            m_TextureRenderers.GetAllActiveItems().ForEach(_R => _R.color = _Color.SetA(_R.color.a));
            var activeBorders = m_Borders.GetAllActiveItems();
            foreach (var border in activeBorders)
                border.Color = _Color.SetA(border.Color.a);

            var activeCorners = m_Corners.GetAllActiveItems();
            foreach (var corner in activeCorners)
                corner.Color = _Color.SetA(corner.Color.a);
            if (ViewSettings.additionalBackgroundType != 2)
                return;
            var activeBorders2 = m_Borders2.GetAllActiveItems();
            foreach (var border in activeBorders2)
                border.Color = _Color.SetA(border.Color.a);
            var activeCorners2 = m_Corners2.GetAllActiveItems();
            foreach (var corner in activeCorners2)
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
                var border = go.AddComponent<Line>()
                    .SetSortingOrder(SortingOrders.AdditionalBackgroundBorder)
                    .SetEndCaps(LineEndCap.Round);
                m_Borders.Add(border);
                if (ViewSettings.additionalBackgroundType != 2)
                    continue;
                var go2 = new GameObject("Additional Background Border Dashed");
                go2.SetParent(Container);
                var border2 = go2.AddComponent<Line>()
                    .SetEndCaps(LineEndCap.Round)
                    .SetDashed(true)
                    .SetDashSnap(DashSnapping.Off)
                    .SetDashType(DashType.Angled)
                    .SetDashSpace(DashSpace.FixedCount)
                    .SetDashSize(8f)
                    .SetDashShapeModifier(1f)
                    .SetSortingOrder(SortingOrders.AdditionalBackgroundBorder);
                m_Borders2.Add(border2);

            }
            m_Borders.DeactivateAll(true);
            if (ViewSettings.additionalBackgroundType == 2)
                m_Borders2.DeactivateAll(true);
        }

        private void InitCornersPool()
        {
            for (int i = 0; i < CornersPoolCount; i++)
            {
                var go = new GameObject("Additional Background Corner Dashed");
                go.SetParent(Container);
                var corner = go.AddComponent<Disc>();
                corner.SetSortingOrder(SortingOrders.AdditionalBackgroundCorner)
                    .SetType(DiscType.Arc)
                    .SetArcEndCaps(ArcEndCap.Round);
                m_Corners.Add(corner);
                if (ViewSettings.additionalBackgroundType != 2)
                    continue;
                var go2 = new GameObject("Additional Background Corner Background");
                go2.SetParent(Container);
                var corner2 = go2.AddComponent<Disc>();
                corner2.SetSortingOrder(SortingOrders.AdditionalBackgroundCorner)
                    .SetType(DiscType.Arc)
                    .SetArcEndCaps(ArcEndCap.Round)
                    .SetDashed(true)
                    .SetDashType(DashType.Angled)
                    .SetDashSpace(DashSpace.FixedCount)
                    .SetDashSize(CornersDashSize)
                    .SetDashSnap(DashSnapping.Tiling)
                    .SetDashShapeModifier(1f);
                m_Corners2.Add(corner2);
            }
            m_Corners.DeactivateAll(true);
            if (ViewSettings.additionalBackgroundType == 2)
                m_Corners2.DeactivateAll(true);
        }

        private void InitTextures()
        {
            var rendBack = Container.AddComponentOnNewChild<SpriteRenderer>(
                "Additional Background Back", out _);
            rendBack.sprite = PrefabSetManager.GetObject<Sprite>(
                "icons", "icon_square_100x100");
            rendBack.material = PrefabSetManager.InitObject<Material>(
                "materials", "additional_background_back");
            rendBack.sortingOrder = SortingOrders.AdditionalBackgroundTexture2;
            m_TextureRendererBack = rendBack;
            
            var go = PrefabSetManager.InitPrefab(
                Container,
                "background",
                "additional_background");
            for (int i = 0; i < TextureRenderersCount; i++)
            {
                var renderer = go.GetCompItem<SpriteRenderer>("renderer");
                renderer.sortingOrder = SortingOrders.AdditionalBackgroundTexture;
                renderer.drawMode = SpriteDrawMode.Tiled;
                renderer.tileMode = SpriteTileMode.Continuous;
                m_TextureRenderers.Add(renderer);
            }
            float ratio = GraphicUtils.AspectRatio;
            if (ratio > 0.7f)       m_AdditionalBackgroundAssetNameSuffix = "high_ratio";
            else if (ratio > 0.54f) m_AdditionalBackgroundAssetNameSuffix = "middle_ratio";
            else                    m_AdditionalBackgroundAssetNameSuffix = "low_ratio";
            m_AdditionalBackgroundInfoSet = PrefabSetManager
                .GetObject<AdditionalBackgroundInfoSetScriptableObject>(
                    "background",
                    "additional_background_set",
                    EPrefabSource.Bundle).set;
        }
        
        private void DeactivatePools()
        {
            TextureRendererMasksPool  .DeactivateAll();
            m_TextureRenderers    .DeactivateAll();
            m_Borders             .DeactivateAll();
            m_Corners             .DeactivateAll();
            if (ViewSettings.additionalBackgroundType != 2)
                return;
            m_Borders2            .DeactivateAll();
            m_Corners2            .DeactivateAll();
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
                DrawBorder(point, EDirection.Left, point == first, point == last);
            (first, last) = (rightBorderPoints.First(), rightBorderPoints.Last());
            foreach (var point in rightBorderPoints)
                DrawBorder(point, EDirection.Right, point == last, point == first);
            (first, last) = (bottomBorderPoints.First(), bottomBorderPoints.Last());
            foreach (var point in bottomBorderPoints)
                DrawBorder(point, EDirection.Down, point == last, point == first);
            (first, last) = (topBorderPoints.First(), topBorderPoints.Last());
            foreach (var point in topBorderPoints)
                DrawBorder(point, EDirection.Up, point == first, point == last);
        }

        private void DrawCornersForGroup(PointsGroupArgs _Group)
        {
            int minX = _Group.Points.Min(_P => _P.X);
            int minY = _Group.Points.Min(_P => _P.Y);
            int maxX = _Group.Points.Max(_P => _P.X);
            int maxY = _Group.Points.Max(_P => _P.Y);
            DrawCorner(new V2Int(minX, minY), false, false, true);
            DrawCorner(new V2Int(minX, maxY), false, true, true);
            DrawCorner(new V2Int(maxX, maxY), true, true, true);
            DrawCorner(new V2Int(maxX, minY), true, false, true);
        }

        private void DrawTextureForGroup(PointsGroupArgs _Group)
        {
            int minX = _Group.Points.Min(_P => _P.X);
            int minY = _Group.Points.Min(_P => _P.Y);
            int maxX = _Group.Points.Max(_P => _P.X);
            int maxY = _Group.Points.Max(_P => _P.Y);
            float scale = CoordinateConverter.Scale;
            float width = scale * (maxX - minX + 2f * (0.5f + BorderRelativeIndent));
            float height = scale * (maxY - minY + 2f * (0.5f + BorderRelativeIndent));
            var backColorArgs = BackgroundTextureController.GetBackgroundColorArgs();
            string additBackTextureName = backColorArgs.AdditionalInfo.additionalBackgroundName;
            additBackTextureName += "_" + m_AdditionalBackgroundAssetNameSuffix;
            var renderer = m_TextureRenderers.FirstInactive;
            m_TextureRenderers.Activate(renderer);
            renderer.sprite = m_AdditionalBackgroundInfoSet
                .FirstOrDefault(_Item => _Item.name == additBackTextureName)
                ?.sprite;
            renderer.transform.localScale = Vector3.one;
            var centerRaw = new Vector2(minX + (maxX - minX) * 0.5f, minY + (maxY - minY) * 0.5f);
            var center = CoordinateConverter.ToLocalMazeItemPosition(centerRaw);
            renderer.transform.SetLocalPosXY(center);
            renderer.sharedMaterial.SetFloat(StencilRefId, _Group.GroupIndex);
            renderer.size = new Vector2(width, height);
            m_TextureRendererBack.transform.SetLocalPosXY(center)
                .SetLocalScaleXY(new Vector2(width, height));
            DrawMaskForGroup(_Group);
            DrawNetLines(_Group);
        }

        private void DrawBorder(V2Int _Position, EDirection _Side, bool _StartLimit, bool _EndLimit)
        {
            var borderPos = ContainersGetter.GetContainer(ContainerNames.MazeItems).transform.position;
            var border = m_Borders.FirstInactive;
            border.SetThickness(ViewSettings.LineThickness * BorderScaleCoefficient * CoordinateConverter.Scale)
                .transform.SetPosXY(borderPos)
                .gameObject.name = _Side + " " + "border";
            (border.Start, border.End) = GetBorderPoints(border, _Position, _Side, _StartLimit, _EndLimit, false);
            m_Borders.Activate(border);
            if (ViewSettings.additionalBackgroundType != 2)
                return;
            var border2 = m_Borders2.FirstInactive;
            border2.SetThickness(ViewSettings.LineThickness * CoordinateConverter.Scale)
                .transform.SetPosXY(borderPos)
                .gameObject.name = _Side + " " + "border" + " " + "dashed";
            (border2.Start, border2.End) = GetBorderPoints(border2, _Position, _Side, _StartLimit, _EndLimit, true);
            m_Borders2.Activate(border2);
        }
        
        private void DrawCorner(
            V2Int _Position,
            bool _Right,
            bool _Up,
            bool _Inner)
        {
            var angles = Mathf.Deg2Rad * GetCornerAngles(_Right, _Up, _Inner);
            var position = ContainersGetter.GetContainer(ContainerNames.MazeItems).transform.position;
            float radius = ViewSettings.LineThickness * BorderScaleCoefficient
                                                     * CoordinateConverter.Scale;
            var corner = m_Corners.FirstInactive;
            corner.SetRadius(radius)
                .SetThickness(ViewSettings.LineThickness * CornerScaleCoefficient * CoordinateConverter.Scale)
                .SetAngRadiansStart(angles.x)
                .SetAngRadiansEnd(angles.y)
                .transform.SetPosXY(position)
                .PlusLocalPosXY(GetCornerCenter(_Position, _Right, _Up));
            m_Corners.Activate(corner);
            if (ViewSettings.additionalBackgroundType != 2)
                return;
            var corner2 = m_Corners2.FirstInactive;
            corner2.SetRadius(radius)
                .SetThickness(ViewSettings.LineThickness * CornerScaleCoefficient * CoordinateConverter.Scale)
                .SetAngRadiansStart(angles.x)
                .SetAngRadiansEnd(angles.y)
                .transform.SetPosXY(position)
                .PlusLocalPosXY(GetCornerCenter(_Position, _Right, _Up));
            m_Corners2.Activate(corner2);
        }
        
        private Tuple<Vector2, Vector2> GetBorderPoints(
            Line _Border,
            V2Int _Point,
            EDirection _Side, 
            bool _StartLimit, 
            bool _EndLimit,
            bool _Second)
        {
            Vector2 start, end;
            Vector2 pos = _Point;
            float cr = ViewSettings.LineThickness * BorderScaleCoefficient;
            Vector2 up, down, left, right, zero;
            (up, down, left, right, zero) = (Vector2.up, Vector2.down, Vector2.left, Vector2.right, Vector2.zero);
            const float bIndent = BorderRelativeIndent;
            switch (_Side)
            {
                case EDirection.Left:
                    start = pos + left * bIndent + (left + down) * 0.5f + (_StartLimit ? up * cr + down * bIndent : zero);
                    end = pos + left * bIndent + (left + up) * 0.5f + (_EndLimit ? down * cr + up * bIndent : zero);
                    break;
                case EDirection.Up:
                    start = pos + up * bIndent + (up + left) * 0.5f + (_StartLimit ? right * cr + left * bIndent : zero);
                    end = pos + up * bIndent + (up + right) * 0.5f + (_EndLimit ? left * cr + right * bIndent : zero);
                    break;
                case EDirection.Right:
                    start = pos + right * bIndent + (right + down) * 0.5f + (_EndLimit ? up * cr + down * bIndent : zero);
                    end = pos + right * bIndent + (right + up) * 0.5f + (_StartLimit ? down * cr + up * bIndent : zero);
                    break;
                case EDirection.Down:
                    start = pos + down * bIndent + (down + left) * 0.5f + (_EndLimit ? right * cr + left * bIndent : zero);
                    end = pos + down * bIndent + (down + right) * 0.5f + (_StartLimit ? left * cr + right * bIndent : zero);
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Side);
            }
            start = CoordinateConverter.ToLocalMazeItemPosition(start);
            end = CoordinateConverter.ToLocalMazeItemPosition(end);
            if (!_Second) 
                return new Tuple<Vector2, Vector2>(start, end);
            float thickness = CoordinateConverter.Scale * ViewSettings.LineThickness * BorderScaleCoefficient * 0.5f;
            var tr = _Border.transform;
            Vector3 startGlobal = tr.TransformPoint(new Vector3(start.x, start.y, 0f));
            Vector3 endGlobal = tr.TransformPoint(new Vector3(end.x, end.y, 0f));
            switch (_Side)
            {
                case EDirection.Left:
                    startGlobal += Vector3.right * thickness;
                    endGlobal += Vector3.right * thickness;
                    break;
                case EDirection.Up:
                    startGlobal += Vector3.down * thickness;
                    endGlobal += Vector3.down * thickness;
                    break;
                case EDirection.Right:
                    startGlobal += Vector3.left * thickness;
                    endGlobal += Vector3.left * thickness;
                    break;
                case EDirection.Down:
                    startGlobal += Vector3.up * thickness;
                    endGlobal += Vector3.up * thickness;
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Side);
            }
            start = tr.InverseTransformPoint(startGlobal);
            end = tr.InverseTransformPoint(endGlobal);
            return new Tuple<Vector2, Vector2>(start, end);
        }

        private Vector2 GetCornerCenter(
            V2Int _Point,
            bool _Right,
            bool _Up)
        {
            float cr = ViewSettings.LineThickness * BorderScaleCoefficient;
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