using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using Common.Extensions;
using Common.Helpers;
using RMAZOR.Models;
using RMAZOR.Views.Common.BackgroundIdleItems;
using UnityEngine.Events;

namespace RMAZOR.Views.Common.Additional_Background
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

    public class ViewMazeAdditionalBackgroundFake : InitBase, IViewMazeAdditionalBackground
    {
        public event UnityAction<List<PointsGroupArgs>> GroupsCollected;
        public void OnLevelStageChanged(LevelStageArgs _Args) { }
    }
    
    public class ViewMazeAdditionalBackground : InitBase, IViewMazeAdditionalBackground
    {
        #region inject

        private IModelGame                                       Model               { get; }
        private IViewMazeAdditionalBackgroundGeometryInitializer GeometryInitializer { get; }
        private IViewMazeAdditionalBackgroundDrawer              Drawer              { get; }
        private IViewMazeBackgroundIdleItems                     IdleItems           { get; }

        private ViewMazeAdditionalBackground(
            IModelGame                                       _Model,
            IViewMazeAdditionalBackgroundGeometryInitializer _GeometryInitializer,
            IViewMazeAdditionalBackgroundDrawer              _Drawer,
            IViewMazeBackgroundIdleItems                    _IdleItems)
        {
            Model               = _Model;
            GeometryInitializer = _GeometryInitializer;
            Drawer              = _Drawer;
            IdleItems           = _IdleItems;
        }

        #endregion

        #region api
        
        public event UnityAction<List<PointsGroupArgs>> GroupsCollected;

        public override void Init()
        {
            IdleItems.Init();
            GeometryInitializer.Init();
            Drawer.Init();
            base.Init();
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.LevelStage)
            {
                case ELevelStage.Loaded: OnLevelLoaded(_Args);  break;
            }
            IdleItems.OnLevelStageChanged(_Args);
        }

        #endregion

        #region nonpublic methods
        
        private void OnLevelLoaded(LevelStageArgs _Args)
        {
            object setBackgroundFromEditorArg = _Args.Args.GetSafe(
                CommonInputCommandArg.KeySetBackgroundFromEditor, out bool keyExist);
            if (keyExist && (bool)setBackgroundFromEditorArg)
                return;
            IdleItems.SetSpawnPool(_Args.LevelIndex);
            var info = Model.Data.Info;
            var groups = GeometryInitializer.GetGroups(info);
            GroupsCollected?.Invoke(groups);
            Drawer.Draw(groups, _Args.LevelIndex);
            Drawer.Appear(true);
        }
        
        #endregion
    }
}