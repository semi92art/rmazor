using System.Collections.Generic;
using Common;
using Common.Entities;
using Common.Helpers;
using RMAZOR.Models;
using UnityEngine;
using UnityEngine.Events;
using Vector2 = System.Numerics.Vector2;

namespace RMAZOR.Views.Common
{
    public class PointsGroupArgs
    {
        public List<V2Int>       Points     { get; }
        public List<List<V2Int>> Holes      { get; }
        public int               GroupIndex { get; }

        public PointsGroupArgs(
            List<V2Int>       _Points, 
            List<List<V2Int>> _Holes,
            int               _GroupIndex)
        {
            Points     = _Points;
            Holes      = _Holes;
            GroupIndex = _GroupIndex;
        }
    }

    public interface IViewMazeAdditionalBackground : IOnLevelStageChanged, IInit
    {
        event UnityAction<List<PointsGroupArgs>> GroupsCollected;
    }
    
    public class ViewMazeAdditionalBackground : InitBase, IViewMazeAdditionalBackground
    {
        #region inject

        private IModelGame                                       Model               { get; }
        private IViewMazeAdditionalBackgroundGeometryInitializer GeometryInitializer { get; }
        private IViewMazeAdditionalBackgroundDrawer              Drawer              { get; }


        public ViewMazeAdditionalBackground(
            IModelGame                                       _Model,
            IViewMazeAdditionalBackgroundGeometryInitializer _GeometryInitializer,
            IViewMazeAdditionalBackgroundDrawer              _Drawer)
        {
            Model               = _Model;
            GeometryInitializer = _GeometryInitializer;
            Drawer              = _Drawer;
        }

        #endregion

        #region api
        
        public event UnityAction<List<PointsGroupArgs>> GroupsCollected;

        public override void Init()
        {
            GeometryInitializer.Init();
            Drawer.Init();
            base.Init();
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:             OnLevelLoaded();        break;
                case ELevelStage.ReadyToUnloadLevel: OnLevelReadyToUnload(); break;
            }
        }

        #endregion

        #region nonpublic methods
        
        private void OnLevelLoaded()
        {
            var info = Model.Data.Info;
            var groups = GeometryInitializer.GetGroups(info);
            GroupsCollected?.Invoke(groups);
            Drawer.Draw(groups);
            Drawer.Appear(true);
        }

        private void OnLevelReadyToUnload()
        {
            Drawer.Appear(false);
        }

        #endregion
    }
}