using System.Collections.Generic;
using System.Linq;
using Common;
using mazing.common.Runtime;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Characters;
using RMAZOR.Views.Common;
using RMAZOR.Views.MazeItems;

namespace RMAZOR.Views.MazeItemGroups
{
    public interface IViewMazeHammersGroup :   
        IInit,
        IViewMazeItemGroup
    {
        void OnHammerShot(HammerShotEventArgs _Args);
    }
    
    public class ViewMazeHammersGroup : ViewMazeItemsGroupBase, IViewMazeHammersGroup
    {
        #region inject
        
        private IViewCharacter Character { get; }
        
        private ViewMazeHammersGroup(IViewMazeCommon _Common, IViewCharacter _Character) 
            : base(_Common)
        {
            Character = _Character;
        }

        #endregion

        #region api
        
        public override IEnumerable<EMazeItemType> Types => new[] {EMazeItemType.Hammer};

        public override void Init()
        {
            base.Init();
            var items = Common
                .GetItems(false, false)
                .Where(_Item => _Item is IViewMazeItemHammer)
                .Cast<IViewMazeItemHammer>();
            foreach (var item in items)
                item.GetViewCharacterInfo = () => Character.GetObjects();
        }

        public void OnHammerShot(HammerShotEventArgs _Args)
        {
            var item = (IViewMazeItemHammer)GetItems().Where(_Item => _Item.ActivatedInSpawnPool)
                .First(_Item => _Item.Props.Position == _Args.Info.StartPosition);
            item.OnHammerShot(_Args);
        }

        #endregion
    }
}