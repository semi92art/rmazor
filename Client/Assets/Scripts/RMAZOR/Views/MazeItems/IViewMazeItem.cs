using System;
using RMAZOR.Models.ProceedInfos;
using RMAZOR.Views.Common;
using RMAZOR.Views.MazeItems.Props;
using SpawnPools;
using UnityEngine;

namespace RMAZOR.Views.MazeItems
{
    public interface IViewMazeItem : ICloneable, ISpawnPoolItem, IOnLevelStageChanged, IAppear
    {
        GameObject Object { get; }
        Component[] Shapes { get; }
        EProceedingStage ProceedingStage { get; set; }
        ViewMazeItemProps Props { get; set; }
        void Init(ViewMazeItemProps _Props);
        void SetLocalPosition(Vector2 _Position);
        void SetLocalScale(float _Scale);
        bool Equal(IMazeItemProceedInfo _MazeItem);
    }
}