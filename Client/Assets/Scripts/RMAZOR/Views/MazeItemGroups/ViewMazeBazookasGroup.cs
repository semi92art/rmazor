using System.Collections.Generic;
using Common;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Characters;
using RMAZOR.Views.Common;
using RMAZOR.Views.MazeItems;
using RMAZOR.Views.MazeItems.Props;

namespace RMAZOR.Views.MazeItemGroups
{
    public interface IViewMazeBazookasGroup :   
        IInit,
        IViewMazeItemGroup
    {
        void OnBazookaAppear(BazookaEventArgs _Args);
        void OnBazookaShot(BazookaEventArgs _Args);
    }
    
    public class ViewMazeBazookasGroup : ViewMazeItemsGroupBase, IViewMazeBazookasGroup
    {
        #region nonpublic members

        private bool m_BazookaInitialized;

        #endregion
        
        #region inject

        private IModelGame           Model     { get; }
        private IViewMazeItemBazooka Bazooka   { get; }
        private IViewCharacter       Character { get; }

        public ViewMazeBazookasGroup(
            IModelGame           _Model,
            IViewMazeCommon      _Common, 
            IViewMazeItemBazooka _Bazooka,
            IViewCharacter       _Character) 
            : base(_Common)
        {
            Model     = _Model;
            Bazooka   = _Bazooka;
            Character = _Character;
        }

        #endregion

        #region api
        
        public override IEnumerable<EMazeItemType> Types => new[] {EMazeItemType.Bazooka};

        public void OnBazookaAppear(BazookaEventArgs _Args)
        {
            Bazooka.OnBazookaAppear(_Args);
        }

        public void OnBazookaShot(BazookaEventArgs _Args)
        {
            Bazooka.OnBazookaShot(_Args);
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage != ELevelStage.Loaded || m_BazookaInitialized) 
                return;
            Bazooka.GetViewCharacterInfo = () =>
            {
                var transform = Character.Transform;
                var colliders = Character.Colliders;
                var info = new ViewCharacterInfo(transform, colliders);
                return info;
            };
            Model.PathItemsProceeder.AllPathsProceededEvent += Bazooka.OnAllPathProceed;
            var props = new ViewMazeItemProps {Type = EMazeItemType.Bazooka};
            Bazooka.Init(props);
        }

        public override IEnumerable<IViewMazeItem> GetActiveItems()
        {
            return new[] {Bazooka};
        }

        #endregion
    }
}