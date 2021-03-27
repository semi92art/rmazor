using Entities;
using Games.RazorMaze.Models;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItem
    {
        ViewMazeItemProps Props { get; set; }
        void Init(ViewMazeItemProps _Props, V2Int _MazeSize);
        void SetLocalPosition(Vector2 _Position);
        void SetLocalScale(float _Scale);
        bool Equal(MazeItem _MazeItem);
    }
}