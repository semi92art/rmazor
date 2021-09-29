using System;
using Games.RazorMaze.Models.ProceedInfos;
using Games.RazorMaze.Views.MazeItems.Props;
using SpawnPools;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItem : ICloneable, ISpawnPoolItem, IOnLevelStageChanged
    {
        GameObject Object { get; }
        EAppearingState AppearingState { get; set; }
        EProceedingStage ProceedingStage { get; set; }
        ViewMazeItemProps Props { get; set; }
        void Init(ViewMazeItemProps _Props);
        void SetLocalPosition(Vector2 _Position);
        void SetLocalScale(float _Scale);
        bool Equal(IMazeItemProceedInfo _MazeItem);
    }
}