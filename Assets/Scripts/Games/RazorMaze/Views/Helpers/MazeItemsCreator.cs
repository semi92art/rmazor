using System.Collections.Generic;
using System.Linq;
using Entities;
using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.MazeItems;
using UnityEngine;

namespace Games.RazorMaze.Views.Helpers
{
    public interface IMazeItemsCreator
    {
        List<IViewMazeItem> CreateMazeItems(MazeInfo _Info);
    }
    
    public class MazeItemsCreator : IMazeItemsCreator
    {
        #region inject
        
        private IContainersGetter ContainersGetter { get; }
        private ICoordinateConverter CoordinateConverter { get; }
        private IViewMazeItemPath ViewMazeItemPath { get; }

        public MazeItemsCreator(
            IContainersGetter _ContainersGetter,
            ICoordinateConverter _CoordinateConverter, 
            IViewMazeItemPath _ViewMazeItemPath)
        {
            ContainersGetter = _ContainersGetter;
            CoordinateConverter = _CoordinateConverter;
            ViewMazeItemPath = _ViewMazeItemPath;

        }
        
        #endregion
        
        public List<IViewMazeItem> CreateMazeItems(MazeInfo _Info)
        {
            CoordinateConverter.Init(_Info.Size);
            
            var res = new List<IViewMazeItem>();
            foreach (var item in _Info.Path)
                AddPathItem(res, _Info, item);
            foreach (var item in _Info.MazeItems)
                AddMazeItem(res, _Info, item);
            // ViewMazeItemPath?.Object.SetActive(false);
            return res;
        }
        
        private void AddPathItem(
            ICollection<IViewMazeItem> _Items,
            MazeInfo _Info,
            V2Int _Position)
        {
            if (ViewMazeItemPath == null)
            {
                AddPathItemProt(_Items, _Info, _Position);
                return;
            }
            
            bool isStartNode = !_Items.Any();
            var props = new ViewMazeItemProps
            {
                IsNode = true,
                IsStartNode = isStartNode,
                Position = _Position
            };

            var item = (IViewMazeItemPath)ViewMazeItemPath.Clone();
            item.Init(props);
            _Items.Add(item);
        }
        
        private void AddPathItemProt(
            ICollection<IViewMazeItem> _Items,
            MazeInfo _Info,
            V2Int _Position) 
        {
            bool isStartNode = !_Items.Any();
            var tr = new GameObject("Path Item").transform;
            tr.SetParent(ContainersGetter.MazeItemsContainer);
            var item = tr.gameObject.AddComponent<ViewMazeItemProt>();
            var props = new ViewMazeItemProps
            {
                IsNode = true,
                IsStartNode = isStartNode,
                Position = _Position
            };
            item.MazeSize = _Info.Size; 
            item.Init(props);
            _Items.Add(item);
        }
        
        private void AddMazeItem(
            ICollection<IViewMazeItem> _Items,
            MazeInfo _Info,
            MazeItem _Item)
        {
            AddMazeItemProt(_Items, _Info, _Item);
            
            
            
        }

        private void AddMazeItemProt(
            ICollection<IViewMazeItem> _Items,
            MazeInfo _Info,
            MazeItem _Item)
        {
            var tr = new GameObject("Maze Item").transform;
            tr.SetParent(ContainersGetter.MazeItemsContainer);
            var item = tr.gameObject.AddComponent<ViewMazeItemProt>();
            var props = new ViewMazeItemProps
            {
                Type = _Item.Type,
                Position = _Item.Position,
                Path = _Item.Path,
                Directions = new List<V2Int>{_Item.Direction},
                IsNode = false,
                IsStartNode = false
            };
            item.MazeSize = _Info.Size;
            item.Init(props);
            _Items.Add(item);
        }
    }
}