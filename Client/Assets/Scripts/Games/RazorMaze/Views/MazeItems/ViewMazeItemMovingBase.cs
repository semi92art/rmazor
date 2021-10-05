using System;
using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.Utils;
using Shapes;
using Ticker;
using UnityEngine;
using Utils;

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
        #region constants

        protected const string SoundClipNameBlockDrop = "block_drop";
        
        #endregion
        
        #region shapes
        
        protected readonly List<Polyline> m_PathLines = new List<Polyline>();
        protected readonly List<Disc> m_PathJoints = new List<Disc>();

        #endregion

        #region constructor

        protected ViewMazeItemMovingBase(
            ViewSettings _ViewSettings,
            IModelGame _Model,
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IGameTicker _GameTicker,
            IViewAppearTransitioner _Transitioner,
            IManagersGetter _Managers) 
            : base(
                _ViewSettings, 
                _Model,
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker,
                _Transitioner,
                _Managers) { }

        #endregion

        #region api

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            base.OnLevelStageChanged(_Args);

            if (_Args.Stage == ELevelStage.Loaded || _Args.Stage == ELevelStage.ReadyToStartOrContinue)
            {
                Object.transform.localPosition = CoordinateConverter.ToLocalMazeItemPosition(Props.Position);
            }
        }
        
        public virtual void OnMoveStarted(MazeItemMoveEventArgs _Args) { }
        public abstract void OnMoving(MazeItemMoveEventArgs _Args);

        public virtual void OnMoveFinished(MazeItemMoveEventArgs _Args)
        {
            Managers.Notify(_SM => _SM.PlayClip(SoundClipNameBlockDrop));
        }

        #endregion
        
        #region nonpublic methods

        protected virtual void InitWallBlockMovingPaths()
        {
            m_PathLines.Clear();
            m_PathJoints.Clear();
            
            var points = Props.Path
                .Select(_P => CoordinateConverter.ToLocalMazeItemPosition(_P))
                .ToList();

            var go = new GameObject("Line");
            go.SetParent(ContainersGetter.MazeItemsContainer);
            go.transform.SetLocalPosXY(Vector2.zero);
            var line = go.AddComponent<Polyline>();
            line.Thickness = ViewSettings.LineWidth * CoordinateConverter.GetScale();
            line.Color = DrawingUtils.ColorLines.SetA(0.5f);
            line.SetPoints(points);
            line.Closed = false;
            line.SortingOrder = DrawingUtils.GetPathLineSortingOrder();
            m_PathLines.Add(line);
            
            foreach (var point in points)
            {
                var go1 = new GameObject("Joint");
                go1.SetParent(ContainersGetter.MazeItemsContainer);
                var joint = go1.AddComponent<Disc>();
                go1.transform.SetLocalPosXY(point);
                joint.Color = DrawingUtils.ColorLines;
                joint.Radius = ViewSettings.LineWidth * CoordinateConverter.GetScale() * 2f;
                joint.Type = DiscType.Disc;
                joint.SortingOrder = DrawingUtils.GetPathLineJointSortingOrder();
                m_PathJoints.Add(joint);
            }
                
            foreach (var pathLine in m_PathLines)
                pathLine.enabled = false;
            foreach (var joint in m_PathJoints)
                joint.enabled = false;
        }

        protected override void SetShape()
        {
            InitWallBlockMovingPaths();
        }

        protected override void Appear(bool _Appear)
        {
            base.Appear(_Appear);
            
            Coroutines.Run(Coroutines.WaitWhile(
                () => !Initialized,
                () =>
                {
                    var sets = new Dictionary<object[], Func<Color>>();
                    if (m_PathLines.Any())
                        sets.Add(m_PathLines.Cast<object>().ToArray(), () => DrawingUtils.ColorLines.SetA(0.5f));
                    if (m_PathJoints.Any())
                        sets.Add(m_PathJoints.Cast<object>().ToArray(), () => DrawingUtils.ColorLines);
                    
                    Transitioner.DoAppearTransitionSimple(
                        _Appear,
                        GameTicker,
                        sets,
                        Props.Path.First());
                }));
        }
        
        #endregion
    }
}