using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Extensions;
using Common.Managers;
using mazing.common.Runtime;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using RMAZOR.Models;
using UnityEngine;

namespace RMAZOR.Views.Common
{
    public interface IViewMazeForeground : IInit, IOnLevelStageChanged { }
    
    public class ViewMazeForeground : ViewMazeGroundBase, IViewMazeForeground
    {
        #region nonpublic members

        private IList<AdditionalColorsProps> m_BackAndFrontColorsSetItemsLight;

        #endregion
        
        #region inject

        private IRemotePropertiesRmazor RemoteProperties { get; }
        private IPrefabSetManager       PrefabSetManager { get; }

        private ViewMazeForeground(
            IRemotePropertiesRmazor _RemoteProperties,
            IColorProvider          _ColorProvider,
            IPrefabSetManager       _PrefabSetManager)
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
            var backgroundColorsSetLight = PrefabSetManager.GetObject<AdditionalColorsSetScriptableObject>
                ("configs", "additional_colors_set");
            m_BackAndFrontColorsSetItemsLight = backgroundColorsSetLight.set
                .Where(_Item => _Item.inUse)
                .ToList();
        }
        
        protected override void SetColorsOnNewLevel(LevelStageArgs _Args)
        {
            var color = GetMainColor(_Args);
            ColorProvider.SetColor(ColorIds.Main, color);
        }

        private Color GetMainColor(LevelStageArgs _Args)
        {
            string levelType = (string) _Args.Args.GetSafe(CommonInputCommandArg.KeyNextLevelType, out _);
            bool isBonusLevel = levelType == CommonInputCommandArg.ParameterLevelTypeBonus;
            int levelIndex = (int)_Args.LevelIndex;
            int setItemIdx = isBonusLevel ? levelIndex : RmazorUtils.GetLevelsGroupIndex(levelIndex) - 1;
            var colorsSet = m_BackAndFrontColorsSetItemsLight;
            setItemIdx %= colorsSet.Count;
            return colorsSet[setItemIdx].main;
        }

        #endregion
    }
}