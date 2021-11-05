﻿using System;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ProceedInfos;

namespace Games.RazorMaze.Views.MazeItems.Props
{
    [Serializable]
    public class ViewMazeItemProps
    {
        public bool          IsNode;
        public bool          IsStartNode;
        public EMazeItemType Type;
        public V2Int         Position;
        public List<V2Int>   Path       = new List<V2Int>();
        public List<V2Int>   Directions = new List<V2Int>{V2Int.zero};
        public V2Int         Pair;
        public bool          IsMoneyItem;

        public bool Equals(IMazeItemProceedInfo _Info)
        {
            if (_Info.Type != Type)
                return false;
            if (_Info.StartPosition != Position)
                return false;
            if (_Info.Path.Count != Path.Count)
                return false;
            if (_Info.Path.Where((_Pos, _Index) => _Pos != Path[_Index]).Any())
                return false;
            if (Directions.Any() && _Info.Direction != Directions.First())
                return false;
            return true;
        }
    }
}