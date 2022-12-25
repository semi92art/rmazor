using System;
using Common;
using mazing.common.Runtime.SpawnPools;
using RMAZOR.Models.ProceedInfos;
using RMAZOR.Views.Common;
using RMAZOR.Views.MazeItems.Props;
using UnityEngine;

namespace RMAZOR.Views.MazeItems
{
    public interface IViewMazeItem :
        ICloneable,
        ISpawnPoolItem, 
        IOnLevelStageChanged,
        IAppear
    {
        GameObject        Object          { get; }
        Component[]       Renderers       { get; }
        EProceedingStage  ProceedingStage { get; set; }
        ViewMazeItemProps Props           { get; set; }
        void              UpdateState(ViewMazeItemProps _Props);
        void              SetLocalPosition(Vector2      _Position);
        void              SetLocalScale(float           _Scale);
        bool              Equal(IMazeItemProceedInfo    _MazeItem);
    }
}