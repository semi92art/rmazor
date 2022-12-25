using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Common;
using Common.Extensions;
using Common.Managers;
using Common.UI;
using Common.Utils;
using mazing.common.Runtime;
using mazing.common.Runtime.Enums;
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
    public class LevelsPanelGroupItem : SimpleUiItemBase
    {
        #region serialized fields
        
        [SerializeField] private TextMeshProUGUI titleText;
        
        [SerializeField] private LevelPanelGroupItemLevelItem
            level1Item,
            level2Item,
            level3Item,
            levelBonusItem;

        #endregion

        #region nonpublic members
        
        private Sprite
            m_ButtonDisabledSprite,
            m_ButtonEnabledSprite,
            m_ButtonSelectedSprite;
        
        private readonly CultureInfo m_FloatValueCultureInfo = new CultureInfo("en-US");

        private ILevelsLoader       LevelsLoader       { get; set; }
        private IRawLevelInfoGetter RawLevelInfoGetter { get; set; }
        private IModelGame          Model              { get; set; }
        private IFontProvider       FontProvider       { get; set; }

        #endregion

        #region api
        
        public void Init(
            ILevelsLoader        _LevelsLoader,
            IRawLevelInfoGetter  _RawLevelInfoGetter,
            IModelGame           _Model,
            IUITicker            _UiTicker,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            IFontProvider        _FontProvider,
            Sprite               _ButtonEnabledSprite,
            Sprite               _ButtonDisabledSprite,
            Sprite               _ButtonSelectedSprite,
            Sprite               _StarEnabledSprite,
            Sprite               _StarDisabledSprite)
        {
            LevelsLoader       = _LevelsLoader;
            RawLevelInfoGetter = _RawLevelInfoGetter;
            Model              = _Model;
            FontProvider       = _FontProvider;
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
            level1Item    .Init(_StarEnabledSprite, _StarDisabledSprite);
            level2Item    .Init(_StarEnabledSprite, _StarDisabledSprite);
            level3Item    .Init(_StarEnabledSprite, _StarDisabledSprite);
            levelBonusItem.Init(_StarEnabledSprite, _StarDisabledSprite);
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
                SetLevelItemButtonInteractable(levelBonusItem, true, false);
            }
            else if (_LevelGroupIndex == _CurrentLevelGroupIndex)
            {
                long firsLevelInCurrentGroupIdx = RmazorUtils.GetFirstLevelInGroupIndex(_LevelGroupIndex);
                string currentLevelType = (string) Model.LevelStaging.Arguments.GetSafe(
                    CommonInputCommandArg.KeyCurrentLevelType, out _);
                bool isCurrentLevelBonus = currentLevelType == CommonInputCommandArg.ParameterLevelTypeBonus;
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
                SetLevelItemButtonInteractable(
                    levelBonusItem, 
                    levelGroupWasFinishedBefore || isCurrentLevelBonus,
                    isCurrentLevelBonus);
            }
            else
            {
                if (RmazorUtils.WasLevelGroupFinishedBefore(_LevelGroupIndex))
                {
                    SetLevelItemButtonInteractable(level1Item, true, false);
                    SetLevelItemButtonInteractable(level2Item, true, false);
                    SetLevelItemButtonInteractable(level3Item, true, false);
                    SetLevelItemButtonInteractable(levelBonusItem, true, false);
                }
                else
                {
                    bool firstLevelEnabled = RmazorUtils.WasLevelGroupFinishedBefore(_LevelGroupIndex - 1);
                    SetLevelItemButtonInteractable(level1Item, firstLevelEnabled, false);
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
            int starsCount = GetStarsCountForLevel(levelIdx, false, bestTime);
            var levelItem = _LevelIndexInGroup switch
            {
                0  => level1Item,
                1  => level2Item,
                2  => level3Item,
                -1 => levelBonusItem,
                _  => throw new SwitchExpressionException(_LevelIndexInGroup)
            };
            levelItem.SetStarsCount(starsCount);
            levelItem.bestTimeText.font =
                FontProvider.GetFont(ETextType.MenuUI, LocalizationManager.GetCurrentLanguage());
            levelItem.bestTimeText.text = float.IsInfinity(bestTime) ?
                "-" : bestTime.ToString("F3", m_FloatValueCultureInfo) + "s";
        }

        private int GetStarsCountForLevel(long _LevelIndex, bool _IsBonus, float _TimeRecord)
        {
            int levelsCount = LevelsLoader.GetLevelsCount(CommonData.GameId, _IsBonus);
            if (levelsCount <= _LevelIndex)
                return 0;
            string levelInfoRaw = null;
            try
            {
                levelInfoRaw = LevelsLoader.GetLevelInfoRaw(CommonData.GameId, _LevelIndex, _IsBonus);
            }
            catch
            {
                Dbg.LogError(_LevelIndex + " " + _IsBonus);
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
            var onClickEvents = new[]
            {
                level1Item.button.onClick,
                level2Item.button.onClick,
                level3Item.button.onClick,
                levelBonusItem.button.onClick
            };
            foreach (var onClickEvent in onClickEvents)
                onClickEvent.RemoveAllListeners();
            level1Item.button.onClick    .AddListener(() => _LoadLevelByIndexAction(0));
            level2Item.button.onClick    .AddListener(() => _LoadLevelByIndexAction(1));
            level3Item.button.onClick    .AddListener(() => _LoadLevelByIndexAction(2));
            levelBonusItem.button.onClick.AddListener(() => _LoadLevelByIndexAction(-1));
        }
        
        private void SetLevelItemTitles(int _LevelsGroupIndex, bool _IsUnknown)
        {
            var font = FontProvider.GetFont(ETextType.MenuUI, LocalizationManager.GetCurrentLanguage());
            level1Item.title.font     = font;
            level2Item.title.font     = font;
            level3Item.title.font     = font;
            levelBonusItem.title.font = font;
            titleText.font = font;
            if (_IsUnknown)
            {
                const string questionSigns = "???";
                titleText.text            = questionSigns;
                level1Item.title.text     = questionSigns;
                level2Item.title.text     = questionSigns;
                level3Item.title.text     = questionSigns;
                levelBonusItem.title.text = questionSigns;
                return;
            }
            long firstLevelInGroup = RmazorUtils.GetFirstLevelInGroupIndex(_LevelsGroupIndex);
            titleText.text = LocalizationManager.GetTranslation("stage").FirstCharToUpper(
                CultureInfo.CurrentUICulture) + " " + _LevelsGroupIndex;
            string levelText = LocalizationManager.GetTranslation("level").FirstCharToUpper(
                CultureInfo.CurrentUICulture);
            level1Item.title.text     = levelText + " " + (firstLevelInGroup + 1);
            level2Item.title.text     = levelText + " " + (firstLevelInGroup + 2);
            level3Item.title.text     = levelText + " " + (firstLevelInGroup + 3);
            levelBonusItem.title.text = levelText + " " + $"E{_LevelsGroupIndex}";
        }

        #endregion
    }
}