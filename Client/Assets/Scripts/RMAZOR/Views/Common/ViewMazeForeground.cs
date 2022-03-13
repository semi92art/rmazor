using System.Collections.Generic;
using Common;
using Common.Managers;
using Common.Providers;
using RMAZOR.Models;
using UnityEngine;

namespace RMAZOR.Views.Common
{
    public interface IViewMazeForeground : IInit, IOnLevelStageChanged { }
    
    public class ViewMazeForeground : ViewMazeGroundBase, IViewMazeForeground
    {

        #region nonpublic members

        private IList<BackAndFrontColorsSetItem> m_BackAndFrontColorsSetItemsLight;
        private IList<BackAndFrontColorsSetItem> m_BackAndFrontColorsSetItemsDark;

        #endregion
        
        #region inject

        private IPrefabSetManager                PrefabSetManager        { get; }

        public ViewMazeForeground(
            IModelGame                              _Model,
            IColorProvider                          _ColorProvider,
            IPrefabSetManager                       _PrefabSetManager)
            : base(_Model, _ColorProvider)
        {
            PrefabSetManager        = _PrefabSetManager;
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
            const string set = "configs";
            var backgroundColorsSetLight = PrefabSetManager.GetObject<BackAndFrontColorsSetScriptableObject>
                (set, "back_and_front_colors_set_light");
            m_BackAndFrontColorsSetItemsLight = backgroundColorsSetLight.set;
            var backgroundColorsSetDark = PrefabSetManager.GetObject<BackAndFrontColorsSetScriptableObject>
                (set, "back_and_front_colors_set_dark");
            m_BackAndFrontColorsSetItemsDark = backgroundColorsSetDark.set;
        }
        
        protected override void SetColorsOnNewLevelOrNewTheme(long _LevelIndex, EColorTheme _Theme)
        {
            var color = GetMainColor(_LevelIndex, _Theme);
            ColorProvider.SetColor(ColorIds.Main, color);
        }

        private Color GetMainColor(long _LevelIndex, EColorTheme _Theme)
        {
            var colorsSet = _Theme == EColorTheme.Light
                ? m_BackAndFrontColorsSetItemsLight
                : m_BackAndFrontColorsSetItemsDark;
            int group = RazorMazeUtils.GetGroupIndex(_LevelIndex);
            int setItemIdx = group % colorsSet.Count;
            return colorsSet[setItemIdx].main;
        }
        
        

        #endregion
    }
}