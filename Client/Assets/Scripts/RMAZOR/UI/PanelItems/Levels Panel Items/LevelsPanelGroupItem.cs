using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Extensions;
using Common.Managers;
using Common.Ticker;
using Common.UI;
using Common.Utils;
using RMAZOR.Helpers;
using RMAZOR.Models;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.UI.PanelItems.Levels_Panel_Items
{
    public class LevelsPanelGroupItem : SimpleUiItemBase
    {
        [SerializeField] private LevelPanelGroupItemLevelItem
            level1Item,
            level2Item,
            level3Item,
            levelBonusItem;

        private Sprite
            m_ButtonDisabledSprite,
            m_ButtonEnabledSprite,
            m_ButtonSelectedSprite;

        private ILevelsLoader       LevelsLoader       { get; set; }
        private IRawLevelInfoGetter RawLevelInfoGetter { get; set; }
        private IModelGame          Model              { get; set; }

        public void Init(
            ILevelsLoader        _LevelsLoader,
            IRawLevelInfoGetter _RawLevelInfoGetter,
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
            LevelsLoader       = _LevelsLoader;
            RawLevelInfoGetter = _RawLevelInfoGetter;
            Model              = _Model;
            InitLevelItems(_StarEnabledSprite, _StarDisabledSprite);
            base.Init(_UiTicker, _AudioManager, _LocalizationManager);
            m_ButtonEnabledSprite  = _ButtonEnabledSprite;
            m_ButtonDisabledSprite = _ButtonDisabledSprite;
            m_ButtonSelectedSprite = _ButtonSelectedSprite;
        }

        public void UpdateItem(
            int              _LevelGroupIndex,
            int              _CurrentLevelGroupIndex,
            bool?            _Enabled,
            UnityAction<int> _Action)
        {
            bool canInteract = _Enabled.HasValue && _Enabled.Value;
            background.sprite = canInteract ? m_ButtonEnabledSprite : m_ButtonDisabledSprite;
            SetLevelItemButtonsInteractable(_LevelGroupIndex, _CurrentLevelGroupIndex, canInteract);
            ActivateLevelItems(true);
            SetButtonActions(_Action);
            SetLevelItemTitles(_LevelGroupIndex, !_Enabled.HasValue);
            SetLevelItemStars(_LevelGroupIndex);
        }

        private void InitLevelItems(Sprite _StarEnabledSprite, Sprite _StarDisabledSprite)
        {
            level1Item    .Init(_StarEnabledSprite, _StarDisabledSprite);
            level2Item    .Init(_StarEnabledSprite, _StarDisabledSprite);
            level3Item    .Init(_StarEnabledSprite, _StarDisabledSprite);
            levelBonusItem.Init(_StarEnabledSprite, _StarDisabledSprite);
        }

        private void SetLevelItemButtonsInteractable(
            int  _LevelGroupIndex,
            int  _CurrentLevelGroupIndex,
            bool _IsInteractableDefault)
        {
            if (_LevelGroupIndex < _CurrentLevelGroupIndex)
            {
                SetLevelItemButtonInteractable(level1Item, true, false);
                SetLevelItemButtonInteractable(level2Item, true, false);
                SetLevelItemButtonInteractable(level3Item, true, false);
                SetLevelItemButtonInteractable(levelBonusItem, true, false);
            }
            else if (_LevelGroupIndex == _CurrentLevelGroupIndex)
            {
                long firsLevelInCurrentGroupIdx = RmazorUtils.GetFirstLevelInGroupIndex(_LevelGroupIndex);
                string currentLevelType = (string) Model.LevelStaging.Arguments.GetSafe(
                    CommonInputCommandArg.KeyCurrentLevelType, out _);
                bool isCurrentLevelBonus = currentLevelType == CommonInputCommandArg.ParameterLevelTypeBonus;
                long currentLevelIndex = Model.LevelStaging.LevelIndex;
                bool levelGroupWasFinishedBefore = WasLevelGroupFinishedBefore(_LevelGroupIndex);
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
                SetLevelItemButtonInteractable(
                    levelBonusItem, 
                    levelGroupWasFinishedBefore || isCurrentLevelBonus,
                    isCurrentLevelBonus);
            }
            else
            {
                if (WasLevelGroupFinishedBefore(_LevelGroupIndex))
                {
                    SetLevelItemButtonInteractable(level1Item, true, false);
                    SetLevelItemButtonInteractable(level2Item, true, false);
                    SetLevelItemButtonInteractable(level3Item, true, false);
                    SetLevelItemButtonInteractable(levelBonusItem, true, false);
                }
                else
                {
                    SetLevelItemButtonInteractable(level1Item, _IsInteractableDefault, false);
                    SetLevelItemButtonInteractable(level2Item, false, false);
                    SetLevelItemButtonInteractable(level3Item, false, false);
                    SetLevelItemButtonInteractable(levelBonusItem, false, false);
                }
            }
        }

        private void SetLevelItemButtonInteractable(
            LevelPanelGroupItemLevelItem _Item,
            bool                         _Interactable,
            bool                         _IsCurrentLevel)
        {
            _Item.button.interactable = _IsCurrentLevel || _Interactable;
            _Item.button.image.sprite = _IsCurrentLevel ? m_ButtonSelectedSprite :
                _Interactable ? m_ButtonEnabledSprite : m_ButtonDisabledSprite;
        }
        
        private static bool WasLevelGroupFinishedBefore(int _LevelsGroupIndex)
        {
            long firsLevelInCurrentGroupIdx = RmazorUtils.GetFirstLevelInGroupIndex(_LevelsGroupIndex);
            int levelsInGroup = RmazorUtils.GetLevelsInGroup(_LevelsGroupIndex);
            long lastLevelInGroup = firsLevelInCurrentGroupIdx + levelsInGroup - 1;
            var levelsFinishedOnceIndicesList = SaveUtils.GetValue(SaveKeysRmazor.LevelsFinishedOnce);
            return levelsFinishedOnceIndicesList.Max() >= lastLevelInGroup;
        }

        private void ActivateLevelItems(bool _Active)
        {
            level1Item.gameObject    .SetActive(_Active);
            level2Item.gameObject    .SetActive(_Active);
            level3Item.gameObject    .SetActive(_Active);
            levelBonusItem.gameObject.SetActive(_Active);
        }

        private void SetLevelItemStars(int _LevelsGroupIndex)
        {
            var mainLevelsTimeRecordDict = SaveUtils.GetValue(
                SaveKeysRmazor.MainLevelTimeRecordsDict) ?? new Dictionary<long, float>();
            var bonusLevelsTimeRecordDict = SaveUtils.GetValue(
                SaveKeysRmazor.BonusLevelTimeRecordsDict) ?? new Dictionary<long, float>();
            SetStarsCountForLevel1(_LevelsGroupIndex, mainLevelsTimeRecordDict);
            SetStarsCountForLevel2(_LevelsGroupIndex, mainLevelsTimeRecordDict);
            SetStarsCountForLevel3(_LevelsGroupIndex, mainLevelsTimeRecordDict);
            SetStarsCountForBonusLevel(_LevelsGroupIndex, bonusLevelsTimeRecordDict);
        }

        private void SetStarsCountForLevel1(int _LevelsGroupIndex, Dictionary<long, float> _TimeRecordsDict)
        {
            long firsLevelInCurrentGroupIdx = RmazorUtils.GetFirstLevelInGroupIndex(_LevelsGroupIndex);
            long level1Idx = firsLevelInCurrentGroupIdx;
            float level1TimeRecord = _TimeRecordsDict.GetSafe(level1Idx, out bool containsKey);
            if (!containsKey)
                level1TimeRecord = float.PositiveInfinity;
            int level1StarsCount = GetStarsCountForLevel(level1Idx, false, level1TimeRecord);
            level1Item.SetStarsCount(level1StarsCount);
        }
        
        private void SetStarsCountForLevel2(int _LevelsGroupIndex, Dictionary<long, float> _TimeRecordsDict)
        {
            long firsLevelInCurrentGroupIdx = RmazorUtils.GetFirstLevelInGroupIndex(_LevelsGroupIndex);
            long level2Idx = firsLevelInCurrentGroupIdx + 1;
            float level2TimeRecord = _TimeRecordsDict.GetSafe(level2Idx, out bool containsKey);
            if (!containsKey)
                level2TimeRecord = float.PositiveInfinity;
            int level2StarsCount = GetStarsCountForLevel(level2Idx, false, level2TimeRecord);
            level2Item.SetStarsCount(level2StarsCount);
        }
        
        private void SetStarsCountForLevel3(int _LevelsGroupIndex, Dictionary<long, float> _TimeRecordsDict)
        {
            long firsLevelInCurrentGroupIdx = RmazorUtils.GetFirstLevelInGroupIndex(_LevelsGroupIndex);
            long level3Idx = firsLevelInCurrentGroupIdx + 2;
            float level3TimeRecord = _TimeRecordsDict.GetSafe(level3Idx, out bool containsKey);
            if (!containsKey)
                level3TimeRecord = float.PositiveInfinity;
            int level3StarsCount = GetStarsCountForLevel(level3Idx, false, level3TimeRecord);
            level3Item.SetStarsCount(level3StarsCount);
        }
        
        private void SetStarsCountForBonusLevel(int _LevelsGroupIndex, Dictionary<long, float> _TimeRecordsDict)
        {
            long levelBonusIdx = _LevelsGroupIndex - 1;
            float levelBonusTimeRecord = _TimeRecordsDict.GetSafe(levelBonusIdx, out bool containsKey);
            if (!containsKey)
                levelBonusTimeRecord = float.PositiveInfinity;
            int levelBonusStarsCount = GetStarsCountForLevel(levelBonusIdx, true, levelBonusTimeRecord);
            levelBonusItem.SetStarsCount(levelBonusStarsCount);
        }

        private int GetStarsCountForLevel(long _LevelIndex, bool _IsBonus, float _TimeRecord)
        {
            var args = new Dictionary<string, object>
            {
                {
                    CommonInputCommandArg.KeyCurrentLevelType,
                    _IsBonus
                        ? CommonInputCommandArg.ParameterLevelTypeBonus
                        : CommonInputCommandArg.ParameterLevelTypeMain
                }
            };
            string levelInfoRaw = LevelsLoader.GetLevelInfoRaw(CommonData.GameId, _LevelIndex, args);
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
            level1Item.button.onClick    .RemoveAllListeners();
            level2Item.button.onClick    .RemoveAllListeners();
            level3Item.button.onClick    .RemoveAllListeners();
            levelBonusItem.button.onClick.RemoveAllListeners();
            level1Item.button.onClick    .AddListener(() => _LoadLevelByIndexAction(0));
            level2Item.button.onClick    .AddListener(() => _LoadLevelByIndexAction(1));
            level3Item.button.onClick    .AddListener(() => _LoadLevelByIndexAction(2));
            levelBonusItem.button.onClick.AddListener(() => _LoadLevelByIndexAction(-1));
        }
        
        private void SetLevelItemTitles(int _LevelsGroupIndex, bool _IsUnknown)
        {
            if (_IsUnknown)
            {
                level1Item.title.text = "???";
                level2Item.title.text = "???";
                level3Item.title.text = "???";
                levelBonusItem.title.text = "???";
                return;
            }
            long firstLevelInGroup = RmazorUtils.GetFirstLevelInGroupIndex(_LevelsGroupIndex);
            level1Item.title.text     = (firstLevelInGroup + 1).ToString();
            level2Item.title.text     = (firstLevelInGroup + 2).ToString();
            level3Item.title.text     = (firstLevelInGroup + 3).ToString();
            levelBonusItem.title.text = "B";
        }
    }
}