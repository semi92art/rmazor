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

        #endregion
        
        #region inject

        private RemoteProperties  RemoteProperties { get; }
        private IPrefabSetManager PrefabSetManager { get; }

        public ViewMazeForeground(
            RemoteProperties _RemoteProperties,
            IModelGame        _Model,
            IColorProvider    _ColorProvider,
            IPrefabSetManager _PrefabSetManager)
            : base(_Model, _ColorProvider)
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
            const string set = "configs";
            m_BackAndFrontColorsSetItemsLight = RemoteProperties.BackAndFrontColorsSet;
            if (m_BackAndFrontColorsSetItemsLight == null)
            {
                var backgroundColorsSetLight = PrefabSetManager.GetObject<BackAndFrontColorsSetScriptableObject>
                    (set, "back_and_front_colors_set_light");
                m_BackAndFrontColorsSetItemsLight = backgroundColorsSetLight.set;
            }
        }
        
        protected override void SetColorsOnNewLevelOrNewTheme(long _LevelIndex, EColorTheme _Theme)
        {
            var color = GetMainColor(_LevelIndex, _Theme);
            ColorProvider.SetColor(ColorIds.Main, color);
        }

        private Color GetMainColor(long _LevelIndex, EColorTheme _Theme)
        {
            var colorsSet = m_BackAndFrontColorsSetItemsLight;
            int group = RazorMazeUtils.GetGroupIndex(_LevelIndex);
            int setItemIdx = group % colorsSet.Count;
            return colorsSet[setItemIdx].main;
        }
        
        

        #endregion
    }
}