using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.MazeItems;
using RMAZOR.Views.MazeItems.Props;

namespace RMAZOR.Views.Helpers.MazeItemsCreators
{
    public class MazeItemsCreatorInEditor : MazeItemsCreatorProt
    {
        protected override void AddMazeItemProt(
            ICollection<IViewMazeItem> _Items, 
            V2Int _MazeSize,
            ViewMazeItemProps _Props)
        {
            AddMazeItemProtCore(_Items, _MazeSize, _Props);
        }

        public override List<IViewMazeItem> CreateMazeItems(MazeInfo _Info)
        {
            var res = new List<IViewMazeItem>();
            foreach (var item in _Info.PathItems)
                AddPathItem(res, _Info, item);
            var mazeItems = _Info.MazeItems.ToList();
            var trapReactMazeItems = mazeItems
                .Where(_Item => _Item.Type == EMazeItemType.TrapReact)
                .ToList();
            if (trapReactMazeItems.Any())
            {
                var groups = trapReactMazeItems.GroupBy(_Item => new
                {
                    _Item.Position
                });
                foreach (var g in groups
                    .Where(_G => _G.Count() > 1))
                {
                    var first = g.First();
                    foreach (var item in g.Except(new[] {first}))
                    {
                        first.Directions = first.Directions.Concat(item.Directions).ToList();
                        mazeItems.Remove(item);
                    }
                }
            }
            foreach (var item in mazeItems)
                AddMazeItem(res, _Info, item);
            return res;
        }
    }
}