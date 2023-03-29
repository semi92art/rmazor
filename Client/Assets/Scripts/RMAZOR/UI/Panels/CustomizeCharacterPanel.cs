using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Managers.PlatformGameServices;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.UI.DialogViewers;
using mazing.common.Runtime.Utils;
using RMAZOR.Constants;
using RMAZOR.Managers;
using RMAZOR.UI.PanelItems.Customoze_Character_Panel_Items;
using RMAZOR.UI.Panels.ShopPanels;
using RMAZOR.UI.Utils;
using RMAZOR.Views.Common;
using RMAZOR.Views.InputConfigurators;
using StansAssets.Foundation.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static RMAZOR.Models.ComInComArg;
using Object = UnityEngine.Object;

namespace RMAZOR.UI.Panels
{
    public interface ICustomizeCharacterPanel : IDialogPanel
    {
        event UnityAction<int> BadgesNumberChanged;
        int                    GetBadgesCount();
    }
    
    public class CustomizeCharacterPanelFake : DialogPanelFake, ICustomizeCharacterPanel
    {
        public event UnityAction<int> BadgesNumberChanged;

        public int GetBadgesCount() => default;
    }
    
    public class CustomizeCharacterPanel : DialogPanelBase, ICustomizeCharacterPanel
    {
        #region nonpublic members

        private RawImage        m_CharacterIcon;
        private TextMeshProUGUI m_CharacterLevelText;
        
        private TextMeshProUGUI m_BankMoneyText;
        private Button          m_ButtonAddMoney;
        
        private Button          m_ButtonClose;

        private ScrollRect    m_CustomCharactersScrollRect, m_CustomColorSetsScrollRect;
        private RectTransform m_CustomCharactersContent,    m_CustomColorSetsContent;

        private TabPanelView m_TabPanelView;
        
        private List<CustomizeCharacterPanelItemCustomCharacter> m_CustomCharacterItemsList;
        private List<CustomizeCharacterPanelItemCustomColorSet>  m_CustomColorSetItemsList;

        protected override string PrefabName => "customize_character_panel";

        #endregion

        #region inject

        private IDialogViewersController DialogViewersController { get; }
        private IShopDialogPanel         ShopDialogPanel         { get; }

        private CustomizeCharacterPanel(
            IManagersGetter             _Managers,
            IUITicker                   _Ticker,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IViewTimePauser             _TimePauser,
            IViewInputCommandsProceeder _CommandsProceeder,
            IShopDialogPanel            _ShopDialogPanel,
            IDialogViewersController    _DialogViewersController) 
            : base(
                _Managers,
                _Ticker, 
                _CameraProvider, 
                _ColorProvider,
                _TimePauser,
                _CommandsProceeder)
        {
            DialogViewersController = _DialogViewersController;
            ShopDialogPanel         = _ShopDialogPanel;
        }

        #endregion

        #region api

        public event UnityAction<int> BadgesNumberChanged;
        
        public override int DialogViewerId => DialogViewerIdsCommon.FullscreenCommon;
        
        public int GetBadgesCount()
        {
            return m_TabPanelView.GetBadges().Count(_B => _B.Number > 0);
        }

        public override void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanel(_Container, _OnClose);
            LoadCustomCharactersArgsList();
            LoadCustomCharacterColorSetsArgsList();
            InitPanelItems();
            AssignCurrentItems();
            m_TabPanelView.BadgesChanged += OnTabPanelBadgesChanged;
            InitTabView();
            Managers.ScoreManager.GameSaved -= OnGameSaved;
            Managers.ScoreManager.GameSaved += OnGameSaved;
        }

        #endregion

        #region nonpublic methods
        
        protected override void OnDialogStartAppearing()
        {
            base.OnDialogStartAppearing();
            UpdateState();
        }
        
        private void OnTabPanelBadgesChanged(List<Badge> _Badges)
        {
            int badgesNum = _Badges.Count(_B => _B.Number > 0);
            BadgesNumberChanged?.Invoke(badgesNum);
        }

        private void OnGameSaved(SavedGameEventArgs _Args)
        {
            var savedGame = (SavedGameV2) _Args.SavedGame;
            int moneyCount = Convert.ToInt32(savedGame.Arguments[KeyMoneyCount]);
            m_BankMoneyText.text = moneyCount.ToString("N0");
        }
        
        private void InitPanelItems()
        {
            InitPanelItemsCustomCharacters();
            InitPanelItemsCustomColorSets();
        }

        private void InitPanelItemsCustomCharacters()
        {
            m_CustomCharactersContent.Clear();
            var characterPanelCharacterItemTemplateGo = Managers.PrefabSetManager
                .GetPrefab(CommonPrefabSetNames.Views, "character_panel_character_item");
            var argsList = LoadCustomCharactersArgsList();
            m_CustomCharacterItemsList = argsList.Select(_Args =>
            {
                var go = Object.Instantiate(characterPanelCharacterItemTemplateGo, m_CustomCharactersContent);
                var viewComponent = go.GetComponent<CustomizeCharacterPanelItemCustomCharacter>();
                viewComponent.Init(
                    Ticker,
                    Managers.AudioManager,
                    Managers.LocalizationManager,
                    Managers.ScoreManager,
                    Managers.AnalyticsManager,
                    GetCharacterLevel,
                    CommandsProceeder,
                    _Args,
                    OnShopButtonClick);
                viewComponent.Selected += _Id =>
                {
                    foreach (var viewComp in m_CustomCharacterItemsList
                        .Where(_Item => _Item.CharacterItemArgsFull.Id != _Id))
                    {
                        viewComp.Select(false);
                    }
                };
                viewComponent.BoughtForGameMoney += UpdateState;
                return viewComponent;
            }).ToList();
            Object.Destroy(characterPanelCharacterItemTemplateGo);
        }

        private void InitPanelItemsCustomColorSets()
        {
            m_CustomColorSetsContent.Clear();
            var characterPanelColorSetItemTemplateGo = Managers.PrefabSetManager
                .GetPrefab(CommonPrefabSetNames.Views, "character_panel_color_set_item");
            var argsList = LoadCustomCharacterColorSetsArgsList();
            m_CustomColorSetItemsList = argsList.Select(_Args =>
            {
                var go = Object.Instantiate(characterPanelColorSetItemTemplateGo, m_CustomColorSetsContent);
                var viewComponent = go.GetComponent<CustomizeCharacterPanelItemCustomColorSet>();
                viewComponent.Init(
                    Ticker,
                    Managers.AudioManager,
                    Managers.LocalizationManager,
                    Managers.ScoreManager,
                    Managers.AnalyticsManager,
                    GetCharacterLevel,
                    ColorProvider,
                    _Args,
                    OnShopButtonClick);
                viewComponent.Selected += _Id =>
                {
                    foreach (var viewComp in m_CustomColorSetItemsList
                        .Where(_Item => _Item.ColorSetItemArgsFull.ColorSetId != _Id))
                    {
                        viewComp.Select(false);
                    }
                };
                return viewComponent;
            }).ToList();
            Object.Destroy(characterPanelColorSetItemTemplateGo);
        }
        
        private List<CustomizeCharacterPanelCharacterItemArgsFull> LoadCustomCharactersArgsList()
        {
            var scrObj = Managers.PrefabSetManager.GetObject<CustomCharacterAssetItemsSetScriptableObject>(
                CommonPrefabSetNames.Configs, "custom_characters_set");
            return scrObj.set
                .Where(_Item => _Item.inUse)
                .Select(_Item =>
            {
                var itemOut = GetPanelItemArgsFullFromAssetItem
                    <CustomizeCharacterPanelCharacterItemArgsFull, CustomCharactersAssetItem>(_Item);
                itemOut.CharacterIconSprite = _Item.icon;
                itemOut.NameLocalizationKey = $"char_name_{_Item.id}";
                return itemOut;
            }).ToList();
        }

        private List<CustomizeCharacterPanelColorSetItemArgsFull> LoadCustomCharacterColorSetsArgsList()
        {
            var scrObj = Managers.PrefabSetManager.GetObject<CustomCharacterColorsSetAssetItemsSetScriptableObject>(
                CommonPrefabSetNames.Configs, "custom_character_colors_set_set");
            return scrObj.set
                .Where(_Item => _Item.inUse)
                .Select(_Item =>
            {
                var itemOut = GetPanelItemArgsFullFromAssetItem
                    <CustomizeCharacterPanelColorSetItemArgsFull, CustomCharacterColorsSetAssetItem>(_Item);
                itemOut.ColorSetId          = _Item.id;
                itemOut.NameLocalizationKey = $"color_{_Item.id}";
                itemOut.Color               = _Item.color1;
                return itemOut;
            }).ToList();
        }
        
        private T1 GetPanelItemArgsFullFromAssetItem<T1, T2>(T2 _CustomCharactersAssetItem) 
            where T1 : CustomizeCharacterPanelCharacterItemArgsFullBase, new()
            where T2 : CustomCharactersAssetItemBase
        {
            return new T1
            {
                Id = _CustomCharactersAssetItem.id,
                CoastArgs = new CustomizeCharacterItemCoastArgs
                {
                    GameMoneyCoast = () => _CustomCharactersAssetItem.gameMoneyCoast
                },
                AccessConditionArgs = new CustomizeCharacterItemAccessConditionArgs
                {
                    CharacterLevel = _CustomCharactersAssetItem.characterLevelToOpen
                },
            };
        }

        private void UpdateState()
        {
            m_CharacterLevelText.text = GetCharacterLevel().ToString();
            m_BankMoneyText.text = GetBankMoneyCount().ToString("N0");
            foreach (var customCharacterItem in m_CustomCharacterItemsList)
                customCharacterItem.UpdateState();
            foreach (var customColorSet in m_CustomColorSetItemsList)
                customColorSet.UpdateState();
            SelectCurrentPanelItems();
        }

        private void SelectCurrentPanelItems()
        {
            SelectCurrentCharacterItem();
            SelectCurrentColorSetItem();
        }

        private void SelectCurrentCharacterItem()
        {
            string charIdCached = SaveUtils.GetValue(SaveKeysRmazor.CharacterIdV2);
            if (string.IsNullOrEmpty(charIdCached))
            {
                charIdCached = "1";
                SaveUtils.PutValue(SaveKeysRmazor.CharacterIdV2, charIdCached);
            }
            foreach (var characterItem in m_CustomCharacterItemsList)
            {
                string itecCharId = characterItem.CharacterItemArgsFull.Id;
                characterItem.Select(itecCharId == charIdCached);
            }
        }

        private void SelectCurrentColorSetItem()
        {
            string colorSetIdCached = SaveUtils.GetValue(SaveKeysRmazor.CharacterColorSetIdV2);
            if (string.IsNullOrEmpty(colorSetIdCached))
            {
                colorSetIdCached = "yellow";
                SaveUtils.PutValue(SaveKeysRmazor.CharacterColorSetIdV2, colorSetIdCached);
            }
            foreach (var customColorSet in m_CustomColorSetItemsList)
            {
                string itemColorSetId = customColorSet.ColorSetItemArgsFull.ColorSetId;
                customColorSet.Select(itemColorSetId == colorSetIdCached);
            }
        }
        
        private void AssignCurrentItems()
        {
            Cor.Run(Cor.WaitNextFrame(() =>
            {
                AssignCurrentCharacter();
                AssignCurrentColorSet();
            }));
        }

        private void InitTabView()
        {
            var tabPropsDict = new Dictionary<int, TabPanelItemArgs>
            {
                {
                    0, new TabPanelItemArgs
                    {
                        LocalizationKey = "characters",
                        OnClick = () =>
                        {
                            m_CustomCharactersScrollRect.SetGoActive(true);
                            m_CustomColorSetsScrollRect.SetGoActive(false);
                        }
                    }
                },
                {
                    1, new TabPanelItemArgs
                    {
                        LocalizationKey = "color_sets",
                        OnClick = () =>
                        {
                            m_CustomCharactersScrollRect.SetGoActive(false);
                            m_CustomColorSetsScrollRect.SetGoActive(true);
                        }
                    }
                },
            };
            var (t, am, lm) = 
                (Ticker, Managers.AudioManager, Managers.LocalizationManager);
            m_TabPanelView.Init(t, am, lm, tabPropsDict, "customize_character");
        }

        private void AssignCurrentCharacter()
        {
            string charIdCached = SaveUtils.GetValue(SaveKeysRmazor.CharacterIdV2);
            if (string.IsNullOrEmpty(charIdCached))
            {
                charIdCached = CommonDataRmazor.CharacterIdDefault;
                SaveUtils.PutValue(SaveKeysRmazor.CharacterIdV2, charIdCached);
            }
            m_CustomCharacterItemsList.FirstOrDefault(_Item =>
                    _Item.CharacterItemArgsFull.Id == charIdCached)!.Select(true);
        }

        private void AssignCurrentColorSet()
        {
            string colorSetIdCached = SaveUtils.GetValue(SaveKeysRmazor.CharacterColorSetIdV2);
            if (string.IsNullOrEmpty(colorSetIdCached))
            {
                colorSetIdCached = CommonDataRmazor.ColorSetIdDefault;
                SaveUtils.PutValue(SaveKeysRmazor.CharacterColorSetIdV2, colorSetIdCached);
            }
            m_CustomColorSetItemsList.FirstOrDefault(_Item =>
                _Item.ColorSetItemArgsFull.ColorSetId == colorSetIdCached)!.Select(true);
        }
        
        protected override void GetPrefabContentObjects(GameObject _Go)
        {
            m_CharacterIcon              = _Go.GetCompItem<RawImage>("character_icon");
            m_CharacterLevelText         = _Go.GetCompItem<TextMeshProUGUI>("character_level_text");
            m_ButtonAddMoney             = _Go.GetCompItem<Button>("add_money_button");
            m_BankMoneyText              = _Go.GetCompItem<TextMeshProUGUI>("bank_money_text");
            m_ButtonClose                = _Go.GetCompItem<Button>("button_close");
            m_CustomCharactersScrollRect = _Go.GetCompItem<ScrollRect>("custom_characters_scroll_rect");
            m_CustomColorSetsScrollRect  = _Go.GetCompItem<ScrollRect>("custom_color_sets_scroll_rect");
            m_CustomCharactersContent    = _Go.GetCompItem<RectTransform>("custom_characters_content");
            m_CustomColorSetsContent     = _Go.GetCompItem<RectTransform>("custom_color_sets_content");
            m_TabPanelView               = _Go.GetCompItem<TabPanelView>("tab_view");
        }

        protected override void LocalizeTextObjectsOnLoad()
        {
            var locTextInfos = new[]
            {
                new LocTextInfo(m_CharacterLevelText,    ETextType.MenuUI_H1, "empty_key",
                    _T => GetCharacterLevel().ToString(), ETextLocalizationType.OnlyText), 
                new LocTextInfo(m_BankMoneyText,         ETextType.MenuUI_H1, "empty_key",
                    _T => GetBankMoneyCount().ToString("N0")), 
            };
            foreach (var locTextInfo in locTextInfos)
                Managers.LocalizationManager.AddLocalization(locTextInfo);
        }

        protected override void SubscribeButtonEvents()
        {
            m_ButtonClose   .onClick.AddListener(OnButtonCloseClick);
            m_ButtonAddMoney.onClick.AddListener(OnAddMoneyButtonClick);
        }

        private void OnButtonCloseClick()
        {
            OnClose();
        }

        private void OnAddMoneyButtonClick()
        {
            PlayButtonClickSound();
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.CustomizeCharacterAddMoneyButtonClick);
            var dv = DialogViewersController.GetViewer(ShopDialogPanel.DialogViewerId);
            dv.Show(ShopDialogPanel, _AdditionalCameraEffectsAction: AdditionalCameraEffectsActionDefaultCoroutine);
        }
        
        private void OnShopButtonClick()
        {
            PlayButtonClickSound();
            var dv = DialogViewersController.GetViewer(ShopDialogPanel.DialogViewerId);
            dv.Show(ShopDialogPanel, 100f, _AdditionalCameraEffectsAction: AdditionalCameraEffectsActionDefaultCoroutine);
        }
        
        private int GetBankMoneyCount()
        {
            var savedGame = Managers.ScoreManager.GetSavedGame(MazorCommonData.SavedGameFileName);
            object bankMoneyArg = savedGame.Arguments.GetSafe(KeyMoneyCount, out bool keyExist);
            if (keyExist)
                return Convert.ToInt32(bankMoneyArg);
            bankMoneyArg = 0L;
            savedGame.Arguments.SetSafe(KeyMoneyCount, bankMoneyArg);
            Managers.ScoreManager.SaveGame(savedGame);
            return Convert.ToInt32(bankMoneyArg);
        }

        private int GetCharacterLevel()
        {
            int totalXpGot = MainMenuUtils.GetTotalXpGot(Managers.ScoreManager);
            return RmazorUtils.GetCharacterLevel(totalXpGot, out _, out _);
        }
        
        private void AdditionalCameraEffectsActionDefaultCoroutine(bool _Appear, float _Time)
        {
            Cor.Run(MainMenuUtils.SubPanelsAdditionalCameraEffectsActionCoroutine(_Appear, _Time,
                CameraProvider, Ticker));
        }

        #endregion
    }
}