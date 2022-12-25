using System.Collections.Generic;
using Common;
using mazing.common.Runtime;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Common;
using RMAZOR.Views.MazeItems;

namespace RMAZOR.Views.MazeItemGroups
{
    public interface IViewMazeDiodesGroup
        : IInit,
          IViewMazeItemGroup
    {
        void OnDiodeBlock(DiodeEventArgs _Args);
        void OnDiodePass(DiodeEventArgs  _Args);
    }
    
    public class ViewMazeDiodesGroup : ViewMazeItemsGroupBase, IViewMazeDiodesGroup
    {
        public ViewMazeDiodesGroup(IViewMazeCommon _Common) : base(_Common) { }

        public override IEnumerable<EMazeItemType> Types => new[] {EMazeItemType.Diode};

        public void OnDiodeBlock(DiodeEventArgs _Args)
        {
            var item = Common.GetItem<IViewMazeItemDiode>(_Args.Info);
            item.OnDiodeBlock(_Args);
        }

        public void OnDiodePass(DiodeEventArgs _Args)
        {
            var item = Common.GetItem<IViewMazeItemDiode>(_Args.Info);
            item.OnDiodePass(_Args);
        }
    }
}