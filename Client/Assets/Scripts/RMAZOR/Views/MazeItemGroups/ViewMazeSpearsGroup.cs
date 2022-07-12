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
    public interface IViewMazeSpearsGroup :   
        IInit,
        IViewMazeItemGroup
    {
        void OnSpearAppear(SpearEventArgs _Args);
        void OnSpearShot(SpearEventArgs _Args);
    }
    
    public class ViewMazeSpearsGroup : ViewMazeItemsGroupBase, IViewMazeSpearsGroup
    {
        #region constants

        private const int MaxSpearsOnLevel = 3;

        #endregion
        
        #region nonpublic members

        private int  m_SpearAppearIndex;
        private int  m_SpearShotIndex;
        private bool m_SpearsInitialized;
        
        private readonly List<IViewMazeItemSpear> m_Spears = new List<IViewMazeItemSpear>();

        #endregion
        
        #region inject

        private IViewMazeItemSpear Spear     { get; }
        private IViewCharacter     Character { get; }

        private ViewMazeSpearsGroup(
            IViewMazeCommon    _Common,
            IViewMazeItemSpear _Spear,
            IViewCharacter     _Character) 
            : base(_Common)
        {
            Spear = _Spear;
            Character = _Character;
        }

        #endregion

        #region api
        
        public override IEnumerable<EMazeItemType> Types => new[] {EMazeItemType.Spear};

        public void OnSpearAppear(SpearEventArgs _Args)
        {
            m_Spears[m_SpearAppearIndex++].OnSpearAppear(_Args);
        }

        public void OnSpearShot(SpearEventArgs _Args)
        {
            m_Spears[m_SpearShotIndex++].OnSpearShot(_Args);
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.LevelStage != ELevelStage.Loaded) 
                return;
            m_SpearAppearIndex = 0;
            m_SpearShotIndex = 0;
            if (m_SpearsInitialized) 
                return;
            InitializeSpears();
            m_SpearsInitialized = true;
        }

        public override IEnumerable<IViewMazeItem> GetActiveItems()
        {
            return m_Spears;
        }

        #endregion

        #region nonpublic methods

        private void InitializeSpears()
        {
            for (int i = 0; i < MaxSpearsOnLevel; i++)
            {
                var spear = (IViewMazeItemSpear) Spear.Clone();
                spear.GetViewCharacterInfo = () => Character.GetObjects();
                var props = new ViewMazeItemProps {Type = EMazeItemType.Spear, Args = new List<string> { $"pos={i % 3}"}};
                spear.UpdateState(props);
                m_Spears.Add(spear);
            }
        }

        #endregion
    }
}