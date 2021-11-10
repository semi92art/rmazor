using System;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using DI.Extensions;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.Utils;
using Shapes;
using Ticker;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItemMovingBlock : IViewMazeItem
    {
        void OnMoveStarted(MazeItemMoveEventArgs _Args);
        void OnMoving(MazeItemMoveEventArgs _Args);
        void OnMoveFinished(MazeItemMoveEventArgs _Args);
    }
    
    public abstract class ViewMazeItemMovingBase : ViewMazeItemBase, IViewMazeItemMovingBlock
    {
        #region nonpublic members
        
        protected static AudioClipArgs AudioClipArgsBlockDrop => new AudioClipArgs("block_drop", EAudioClipType.Sound);
        
        #endregion
        
        #region shapes

        protected          Polyline   m_PathPolyLine;
        protected readonly List<Line> m_PathLines = new List<Line>();
        protected readonly List<Disc> m_PathJoints = new List<Disc>();

        #endregion

        #region constructor

        protected ViewMazeItemMovingBase(
            ViewSettings _ViewSettings,
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
                _ColorProvider) { }

        #endregion

        #region api

        public override bool ActivatedInSpawnPool
        {
            get => base.ActivatedInSpawnPool;
            set
            {
                if (m_PathPolyLine.IsNotNull())
                    m_PathPolyLine.enabled = false;
                foreach (var pathJoint in m_PathJoints)
                    pathJoint.enabled = false;
                base.ActivatedInSpawnPool = value;
            }
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            base.OnLevelStageChanged(_Args);

            if (_Args.Stage == ELevelStage.Loaded || _Args.Stage == ELevelStage.ReadyToStart)
            {
                Object.transform.localPosition = CoordinateConverter.ToLocalMazeItemPosition(Props.Position);
            }
        }

        public abstract void OnMoveStarted(MazeItemMoveEventArgs _Args);
        public abstract void OnMoving(MazeItemMoveEventArgs _Args);

        public virtual void OnMoveFinished(MazeItemMoveEventArgs _Args)
        {
            Managers.AudioManager.PlayClip(AudioClipArgsBlockDrop);
        }

        #endregion
        
        #region nonpublic methods

        protected override void UpdateShape()
        {
            InitWallBlockMovingPaths();
        }
        
        protected virtual void InitWallBlockMovingPaths()
        {
            InitWallBlockMovingPathsCore();
        }

        protected void InitWallBlockMovingPathsCore()
        {
            if (m_PathPolyLine.IsNotNull())
                m_PathPolyLine.gameObject.DestroySafe();
            foreach (var line in m_PathLines)
                line.gameObject.DestroySafe();
            m_PathLines.Clear();
            foreach (var joint in m_PathJoints)
                joint.gameObject.DestroySafe();
            m_PathJoints.Clear();
            var points = Props.Path
                .Select(_P => CoordinateConverter.ToLocalMazeItemPosition(_P))
                .ToList();
            var containier = ContainersGetter.GetContainer(ContainerNames.MazeItems);
            m_PathPolyLine = CreatePolyline(points, containier);
            for (int i = 0; i < points.Count; i++)
            {
                m_PathJoints.Add( CreateJoint(points[i], containier));
                if (i == points.Count - 1)
                    continue;
                m_PathLines.Add(CreateLine(points[i], points[i + 1], containier));
            }
            m_PathPolyLine.enabled = false;
            foreach (var pathLine in m_PathLines)
                pathLine.enabled = false;
            foreach (var joint in m_PathJoints)
                joint.enabled = false;
        }

        private Polyline CreatePolyline(IReadOnlyCollection<Vector2> _Points, Transform _Container)
        {
            var polyLineGo = new GameObject(ObjectName + " PolyLine");
            polyLineGo.SetParent(_Container);
            polyLineGo.transform.SetLocalPosXY(Vector2.zero);
            var polyLine = polyLineGo.AddComponent<Polyline>();
            polyLine.Thickness = ViewSettings.LineWidth * CoordinateConverter.Scale;
            polyLine.Color = GetPolyLineColor();
            polyLine.SetPoints(_Points);
            polyLine.Closed = false;
            polyLine.SortingOrder = SortingOrders.PathLine;
            polyLine.Joins = PolylineJoins.Round;
            return polyLine;
        }

        private Disc CreateJoint(Vector2 _Point, Transform _Container)
        {
            var jointGo = new GameObject(ObjectName +  " Joint");
            jointGo.SetParent(_Container);
            jointGo.transform.SetLocalPosXY(_Point);
            var joint = jointGo.AddComponent<Disc>();
            joint.Color = GetJointColor();
            joint.Radius = ViewSettings.LineWidth * CoordinateConverter.Scale * 2f;
            joint.Type = DiscType.Disc;
            joint.SortingOrder = SortingOrders.PathJoint;
            return joint;
        }

        private Line CreateLine(Vector2 _Start, Vector2 _End, Transform _Container)
        {
            var lineGo = new GameObject(ObjectName + " Line");
            lineGo.SetParent(_Container);
            lineGo.transform.SetLocalPosXY(Vector2.zero);
            var line = lineGo.AddComponent<Line>();
            (line.Start, line.End) = (_Start, _End);
            line.Color = GetLineColor();
            line.Dashed = true;
            line.Thickness = ViewSettings.LineWidth * CoordinateConverter.Scale;
            line.DashSize = 2f;
            line.DashType = DashType.Rounded;
            return line;
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var sets = base.GetAppearSets(_Appear);
            if (m_PathPolyLine.IsNotNull())
                sets.Add(new Component[] { m_PathPolyLine}, GetPolyLineColor);
            if (m_PathLines.Any())
                sets.Add(m_PathLines, GetLineColor);
            if (m_PathJoints.Any())
                sets.Add(m_PathJoints, GetJointColor);
            return sets;
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId != ColorIds.Main) 
                return;
            m_PathPolyLine.Color = GetPolyLineColor();
            foreach (var line in m_PathLines)
                line.Color = GetLineColor();
            foreach (var joint in m_PathJoints)
                joint.Color = GetJointColor();
        }

        private Color GetPolyLineColor()
        {
            return GetMainColor().SetA(0.2f);
        }
        
        private Color GetLineColor()
        {
            return GetMainColor().SetA(0.7f);
        }

        private Color GetJointColor()
        {
            return GetMainColor();
        }

        private Color GetMainColor()
        {
            return ColorProvider.GetColor(ColorIds.Main);
        }

        

        #endregion
    }
}