using System;
using Games.RazorMaze.Models;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItem : ICloneable
    {
        GameObject Object { get; }
        bool Active { get; set; }
        ViewMazeItemProps Props { get; set; }
        void Init(ViewMazeItemProps _Props);
        void SetLocalPosition(Vector2 _Position);
        void SetLocalScale(float _Scale);
        bool Equal(MazeItem _MazeItem);
    }
}