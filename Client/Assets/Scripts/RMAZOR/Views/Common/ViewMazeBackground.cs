using System.Runtime.CompilerServices;
using Common;
using Common.Providers;
using RMAZOR.Models;
using RMAZOR.Views.Common.CongratulationItems;
using UnityEngine;

namespace RMAZOR.Views.Common
{
    public interface IViewMazeBackground : IInit, IOnLevelStageChanged { }
    
    public class ViewMazeBackground : ViewMazeGroundBase, IViewMazeBackground
    {
        #region inject

        private IViewMazeBackgroundIdleItems         IdleItems         { get; }
        private IViewMazeBackgroundCongradItems      CongratItems      { get; }
        private IViewMazeBackgroundTextureController TextureController { get; }

        public ViewMazeBackground(
            IModelGame                           _Model,
            IColorProvider                       _ColorProvider,
            IViewMazeBackgroundIdleItems         _IdleItems,
            IViewMazeBackgroundCongradItems      _CongratItems,
            IViewMazeBackgroundTextureController _TextureController) 
            : base(_Model, _ColorProvider)
        {
            IdleItems         = _IdleItems;
            CongratItems      = _CongratItems;
            TextureController = _TextureController;
        }
        
        #endregion
        
        #region api

        public override void Init()
        {
            IdleItems        .Init();
            CongratItems     .Init();
            TextureController.Init();
            base.Init();
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            IdleItems        .OnLevelStageChanged(_Args);
            CongratItems     .OnLevelStageChanged(_Args);
            TextureController.OnLevelStageChanged(_Args);
        }

        #endregion
    }
}