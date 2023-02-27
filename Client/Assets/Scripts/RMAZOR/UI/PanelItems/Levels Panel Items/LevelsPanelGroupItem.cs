using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using mazing.common.Runtime;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.Utils;
using RMAZOR.Helpers;
using RMAZOR.Models;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.UI.PanelItems.Levels_Panel_Items
{
    public class LevelsPanelGroupItem : SimpleUiItem
    {
        #region serialized fields
        
        [SerializeField] private TextMeshProUGUI titleText;

        [SerializeField] private LevelsPanelGroupItemLevelItem
            level1Item,
            level2Item,
            level3Item;

        #endregion

        #region nonpublic members
        
        private Sprite
            m_ButtonDisabledSprite,
            m_ButtonEnabledSprite,
            m_ButtonSelectedSprite;
        
        private ILevelsLoader       LevelsLoader       { get; set; }
        private IRawLevelInfoGetter RawLevelInfoGetter { get; set; }
        private IModelGame          Model              { get; set; }

        #endregion

        #region api
        
        public void Init(
            ILevelsLoader        _LevelsLoader,
            IRawLevelInfoGetter  _RawLevelInfoGetter,
            IModelGame           _Model,
            IUITicker            _UiTicker,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            Sprite               _ButtonEnabledSprite,
            Sprite               _ButtonDisabledSprite,
            Sprite               _ButtonSelectedSprite,
            Sprite               _StarEnabledSprite,
            Sprite               _StarDisabledSprite)
        {
            base.Init(_UiTicker, _AudioManager, _LocalizationManager);
            LevelsLoader           = _LevelsLoader;
            RawLevelInfoGetter     = _RawLevelInfoGetter;
            Model                  = _Model;
            m_ButtonEnabledSprite  = _ButtonEnabledSprite;
            m_ButtonDisabledSprite = _ButtonDisabledSprite;
            m_ButtonSelectedSprite = _ButtonSelectedSprite;
            InitLevelItems(_StarEnabledSprite, _StarDisabledSprite);
        }

        public void UpdateItem(
            int              _LevelGroupIndex,
            int              _CurrentLevelGroupIndex,
            bool?            _Enabled,
            UnityAction<int> _Action)
        {
            bool canInteract = _Enabled.HasValue && _Enabled.Value;
            background.sprite = canInteract ? m_ButtonEnabledSprite : m_ButtonDisabledSprite;
            SetLevelItemButtonsInteractable(_LevelGroupIndex, _CurrentLevelGroupIndex);
            ActivateLevelItems(true);
            SetButtonActions(_Action);
            SetLevelItemTitles(_LevelGroupIndex, !_Enabled.HasValue);
            SetLevelItemStars(_LevelGroupIndex);
        }

        #endregion

        #region nonpublic methods
        
        private void InitLevelItems(Sprite _StarEnabledSprite, Sprite _StarDisabledSprite)
        {
            var levelItems = GetLevelItems();
            foreach (var levelItem in levelItems)
                levelItem.Init(
                    Ticker,
                    AudioManager, 
                    LocalizationManager, 
                    _StarEnabledSprite, 
                    _StarDisabledSprite);
        }

        private void SetLevelItemButtonsInteractable(
            int  _LevelGroupIndex,
            int  _CurrentLevelGroupIndex)
        {
            if (_LevelGroupIndex < _CurrentLevelGroupIndex)
            {
                SetLevelItemButtonInteractable(level1Item, true, false);
                SetLevelItemButtonInteractable(level2Item, true, false);
                SetLevelItemButtonInteractable(level3Item, true, false);
            }
            else if (_LevelGroupIndex == _CurrentLevelGroupIndex)
            {
                long firsLevelInCurrentGroupIdx = RmazorUtils.GetFirstLevelInGroupIndex(_LevelGroupIndex);
                string currentLevelType = (string) Model.LevelStaging.Arguments.GetSafe(
                    ComInComArg.KeyCurrentLevelType, out _);
                bool isCurrentLevelBonus = currentLevelType == ComInComArg.ParameterLevelTypeBonus;
                long currentLevelIndex = Model.LevelStaging.LevelIndex;
                bool levelGroupWasFinishedBefore = RmazorUtils.WasLevelGroupFinishedBefore(_LevelGroupIndex);
                SetLevelItemButtonInteractable(
                    level1Item, 
                    levelGroupWasFinishedBefore || currentLevelIndex >= firsLevelInCurrentGroupIdx || isCurrentLevelBonus, 
                    currentLevelIndex == firsLevelInCurrentGroupIdx && !isCurrentLevelBonus);
                SetLevelItemButtonInteractable(
                    level2Item, 
                    levelGroupWasFinishedBefore || currentLevelIndex >= firsLevelInCurrentGroupIdx + 1 || isCurrentLevelBonus,
                    currentLevelIndex == firsLevelInCurrentGroupIdx + 1 && !isCurrentLevelBonus);
                SetLevelItemButtonInteractable(
                    level3Item, 
                    levelGroupWasFinishedBefore || currentLevelIndex >= firsLevelInCurrentGroupIdx + 2 || isCurrentLevelBonus, 
                    currentLevelIndex == firsLevelInCurrentGroupIdx + 2 && !isCurrentLevelBonus);
            }
            else
            {
                if (RmazorUtils.WasLevelGroupFinishedBefore(_LevelGroupIndex))
                {
                    SetLevelItemButtonInteractable(level1Item, true, false);
                    SetLevelItemButtonInteractable(level2Item, true, false);
                    SetLevelItemButtonInteractable(level3Item, true, false);
                }
                else
                {
                    bool firstLevelEnabled = RmazorUtils.WasLevelGroupFinishedBefore(_LevelGroupIndex - 1);
                    SetLevelItemButtonInteractable(level1Item, firstLevelEnabled, false);
                    SetLevelItemButtonInteractable(level2Item, false, false);
                    SetLevelItemButtonInteractable(level3Item, false, false);
                }
            }
        }

        private void SetLevelItemButtonInteractable(
            LevelsPanelGroupItemLevelItem _Item,
            bool                         _Interactable,
            bool                         _IsCurrentLevel)
        {
            _Item.SetInteractable(_IsCurrentLevel || _Interactable);
            _Item.SetButtonImageSprite(_IsCurrentLevel ? m_ButtonSelectedSprite :
                _Interactable ? m_ButtonEnabledSprite : m_ButtonDisabledSprite);
        }
        
        private void ActivateLevelItems(bool _Active)
        {
            level1Item.gameObject    .SetActive(_Active);
            level2Item.gameObject    .SetActive(_Active);
            level3Item.gameObject    .SetActive(_Active);
        }

        private void SetLevelItemStars(int _LevelsGroupIndex)
        {
            var mainLevelsTimeRecordDict = SaveUtils.GetValue(
                SaveKeysRmazor.MainLevelTimeRecordsDict) ?? new Dictionary<long, float>();
            var bonusLevelsTimeRecordDict = SaveUtils.GetValue(
                SaveKeysRmazor.BonusLevelTimeRecordsDict) ?? new Dictionary<long, float>();
            SetStarsCountAndBestTimeTextForLevelInGroup(_LevelsGroupIndex, 0, mainLevelsTimeRecordDict);
            SetStarsCountAndBestTimeTextForLevelInGroup(_LevelsGroupIndex, 1, mainLevelsTimeRecordDict);
            SetStarsCountAndBestTimeTextForLevelInGroup(_LevelsGroupIndex, 2, mainLevelsTimeRecordDict);
            SetStarsCountAndBestTimeTextForLevelInGroup(_LevelsGroupIndex, -1, bonusLevelsTimeRecordDict);
        }

        private void SetStarsCountAndBestTimeTextForLevelInGroup(
            int                     _LevelsGroupIndex,
            int                     _LevelIndexInGroup,
            Dictionary<long, float> _TimeRecordsDict)
        {
            long firsLevelInCurrentGroupIdx = RmazorUtils.GetFirstLevelInGroupIndex(_LevelsGroupIndex);
            long levelIdx = _LevelIndexInGroup == -1 ?
                _LevelsGroupIndex - 1 : firsLevelInCurrentGroupIdx + _LevelIndexInGroup;
            float bestTime = _TimeRecordsDict.GetSafe(levelIdx, out bool containsKey);
            if (!containsKey)
                bestTime = float.PositiveInfinity;
            int starsCount = GetStarsCountForLevel(levelIdx, ComInComArg.ParameterLevelTypeDefault, bestTime);
            var levelItem = _LevelIndexInGroup switch
            {
                0  => level1Item,
                1  => level2Item,
                2  => level3Item,
                _  => throw new SwitchExpressionException(_LevelIndexInGroup)
            };
            levelItem.SetStarsCount(starsCount);
            levelItem.SetBestTimeText(bestTime);
        }

        private int GetStarsCountForLevel(long _LevelIndex, string _LevelType, float _TimeRecord)
        {
            var args = new LevelInfoArgs
            {
                LevelType = _LevelType,
                LevelIndex = _LevelIndex
            };
            int levelsCount = LevelsLoader.GetLevelsCount(args);
            if (levelsCount <= _LevelIndex)
                return 0;
            string levelInfoRaw = null;
            try
            {
                levelInfoRaw = LevelsLoader.GetLevelInfoRaw(args);
            }
            catch
            {
                Dbg.LogError(_LevelIndex + " " + _LevelType);
            }
            
            (float time3Stars, float time2Stars, float time1Star) = RawLevelInfoGetter.GetStarTimeRecords(levelInfoRaw);
            if (_TimeRecord < time3Stars)
                return 3;
            if (_TimeRecord < time2Stars)
                return 2;
            if (_TimeRecord < time1Star)
                return 1;
            return 0;
        }

        private void SetButtonActions(UnityAction<int> _LoadLevelByIndexAction)
        {
            var levelItems = GetLevelItems();
            for (int i = 0; i < levelItems.Length; i++)
            {
                int i1 = i;
                levelItems[i].SetOnClickEvent(() => _LoadLevelByIndexAction(i1));
            }
        }
        
        private void SetLevelItemTitles(int _LevelsGroupIndex, bool _IsUnknown)
        {
            long firstLevelInGroup = RmazorUtils.GetFirstLevelInGroupIndex(_LevelsGroupIndex);
            titleText.text = LocalizationManager.GetTranslation("stage").FirstCharToUpper(
                CultureInfo.CurrentUICulture) + " " + _LevelsGroupIndex;
            string levelText = LocalizationManager.GetTranslation("level").FirstCharToUpper(
                CultureInfo.CurrentUICulture);
            var levelItems = GetLevelItems();
            for (int i = 0; i < levelItems.Length; i++)
                levelItems[i].SetTitle(_IsUnknown ? "???" : levelText + " " + (firstLevelInGroup + i + 1));
        }

        private LevelsPanelGroupItemLevelItem[] GetLevelItems()
        {
            return new[] {level1Item, level2Item, level3Item};
        }

        #endregion
    }
}