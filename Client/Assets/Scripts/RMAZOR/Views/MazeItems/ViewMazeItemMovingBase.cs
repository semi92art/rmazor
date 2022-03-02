using System;
using System.Collections.Generic;
using System.Linq;
using Common.Constants;
using Common.Entities;
using Common.Enums;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Views.Common;
using RMAZOR.Views.Helpers;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.MazeItems
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

        private static AudioClipArgs AudioClipArgsBlockDrop =>
            new AudioClipArgs("block_drop", EAudioClipType.GameSound);
        
        #endregion
        
        #region shapes

        private readonly List<Line> m_PathLines  = new List<Line>();
        private readonly List<Disc> m_PathJoints = new List<Disc>();

        #endregion

        #region constructor

        protected ViewMazeItemMovingBase(
            ViewSettings                  _ViewSettings,
            IModelGame                    _Model,
            IMazeCoordinateConverter      _CoordinateConverter,
            IContainersGetter             _ContainersGetter,
            IViewGameTicker               _GameTicker,
            IViewBetweenLevelTransitioner _Transitioner,
            IManagersGetter               _Managers,
            IColorProvider                _ColorProvider,
            IViewInputCommandsProceeder   _CommandsProceeder)
            : base(
                _ViewSettings,
                _Model,
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker,
                _Transitioner,
                _Managers,
                _ColorProvider,
                _CommandsProceeder) { }

        #endregion

        #region api

        public override bool ActivatedInSpawnPool
        {
            get => base.ActivatedInSpawnPool;
            set
            {
                foreach (var pathJoint in m_PathJoints)
                    pathJoint.enabled = false;
                foreach (var pathLine in m_PathLines)
                    pathLine.enabled = false;
                base.ActivatedInSpawnPool = value;
            }
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            base.OnLevelStageChanged(_Args);
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                case ELevelStage.ReadyToStart when _Args.PreviousStage == ELevelStage.Loaded:
                    var pos = CoordinateConverter.ToLocalMazeItemPosition(Props.Position);
                    SetLocalPosition(pos);
                    break;
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
            for (int i = 0; i < points.Count; i++)
            {
                m_PathJoints.Add( CreateJoint(points[i], containier));
                if (i == points.Count - 1)
                    continue;
                m_PathLines.Add(CreateLine(points[i], points[i + 1], containier));
            }
            foreach (var pathLine in m_PathLines)
                pathLine.enabled = false;
            foreach (var joint in m_PathJoints)
                joint.enabled = false;
        }

        private Disc CreateJoint(Vector2 _Point, Transform _Container)
        {
            var jointGo = new GameObject(ObjectName +  " Joint");
            jointGo.SetParent(_Container);
            jointGo.transform.SetLocalPosXY(_Point);
            var joint = jointGo.AddComponent<Disc>();
            joint.Radius = ViewSettings.LineWidth * CoordinateConverter.Scale * 0.5f;
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
            line.Dashed = true;
            line.Thickness = ViewSettings.LineWidth * CoordinateConverter.Scale * 0.5f;
            line.DashSize = 1f;
            line.DashSpacing = line.DashSize * 2f;
            line.DashType = DashType.Rounded;
            return line;
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var sets = base.GetAppearSets(_Appear);
            var col = ColorProvider.GetColor(ColorIds.Main);
            if (m_PathLines.Any())
                sets.Add(m_PathLines, () => col);
            if (m_PathJoints.Any())
                sets.Add(m_PathJoints, () => col);
            return sets;
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId != ColorIds.Main) 
                return;
            foreach (var line in m_PathLines.Where(_Line => _Line.IsNotNull()))
                line.Color = _Color.SetA(0.7f);
            foreach (var joint in m_PathJoints.Where(_Joint => _Joint.IsNotNull()))
                joint.Color = _Color;
        }

        #endregion
    }
}