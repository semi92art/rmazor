using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Extensions;
using Common.Managers;
using Common.Providers;
using UnityEngine;

namespace RMAZOR.Views.Common
{
    public interface IViewMazeForeground : IInit, IOnLevelStageChanged { }
    
    public class ViewMazeForeground : ViewMazeGroundBase, IViewMazeForeground
    {
        #region nonpublic members

        private IList<BackAndFrontColorsSetItem> m_BackAndFrontColorsSetItemsLight;

        #endregion
        
        #region inject

        private RemoteProperties  RemoteProperties { get; }
        private IPrefabSetManager PrefabSetManager { get; }

        public ViewMazeForeground(
            RemoteProperties _RemoteProperties,
            IColorProvider    _ColorProvider,
            IPrefabSetManager _PrefabSetManager)
            : base(_ColorProvider)
        {
            RemoteProperties = _RemoteProperties;
            PrefabSetManager = _PrefabSetManager;
        }
        
        #endregion

        #region api

        public override void Init()
        {
            LoadSets();
            ColorProvider.AddIgnorableForThemeSwitchColor(ColorIds.Main);
            base.Init();
        }
        
        #endregion
        
        #region nonpublic methods

        private void LoadSets()
        {
            m_BackAndFrontColorsSetItemsLight = RemoteProperties.BackAndFrontColorsSet
                .Where(_Item => _Item.inUse)
                .ToList();
            if (!m_BackAndFrontColorsSetItemsLight.NullOrEmpty())
                return;
            var backgroundColorsSetLight = PrefabSetManager.GetObject<BackAndFrontColorsSetScriptableObject>
                ("configs", "back_and_front_colors_set_light");
            m_BackAndFrontColorsSetItemsLight = backgroundColorsSetLight.set
                .Where(_Item => _Item.inUse)
                .ToList();
        }
        
        protected override void SetColorsOnNewLevel(long _LevelIndex)
        {
            var color = GetMainColor(_LevelIndex);
            ColorProvider.SetColor(ColorIds.Main, color);
        }

        private Color GetMainColor(long _LevelIndex)
        {
            var colorsSet = m_BackAndFrontColorsSetItemsLight;
            int group = RmazorUtils.GetGroupIndex(_LevelIndex);
            int setItemIdx = group % colorsSet.Count;
            return colorsSet[setItemIdx].main;
        }
        
        

        #endregion
    }
}