using System;
using System.Collections.Generic;
using System.Globalization;
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
using RMAZOR.Managers;
using RMAZOR.UI.PanelItems.Customoze_Character_Panel_Items;
using RMAZOR.UI.Panels.ShopPanels;
using RMAZOR.UI.Utils;
using RMAZOR.Views.Common;
using RMAZOR.Views.InputConfigurators;
using StansAssets.Foundation.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static RMAZOR.Models.ComInComArg;
using Object = UnityEngine.Object;

namespace RMAZOR.UI.Panels
{
    public interface ICustomizeCharacterPanel : IDialogPanel { }
    
    public class CustomizeCharacterPanelFake : DialogPanelFake, ICustomizeCharacterPanel { }
    
    public class CustomizeCharacterPanel : DialogPanelBase, ICustomizeCharacterPanel
    {
        #region nonpublic members

        private RawImage        m_CharacterIcon;
        private TextMeshProUGUI m_CharacterLevelText;
        
        private TextMeshProUGUI m_BankMoneyText;
        private Button          m_ButtonAddMoney;
        
        private TextMeshProUGUI m_CustomCharactersTitle, m_CustomColorSetsTitle;
        private Button          m_ButtonClose;

        private ScrollRect    m_CustomCharactersScrollRect, m_CustomColorSetsScrollRect;
        private RectTransform m_CustomCharactersContent,    m_CustomColorSetsContent;
        
        private List<CustomizeCharacterPanelItemCustomCharacter> m_CustomCharacterItemsList;
        private List<CustomizeCharacterPanelItemCustomColorSet>  m_CustomColorSetItemsList;

        protected override string PrefabName => "customize_character_panel";

        #endregion

        #region inject
        
        private IShopDialogPanel         ShopDialogPanel         { get; }
        private IDialogViewersController DialogViewersController { get; }

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
            ShopDialogPanel         = _ShopDialogPanel;
            DialogViewersController = _DialogViewersController;
        }

        #endregion

        #region api

        public override int DialogViewerId => DialogViewerIdsCommon.FullscreenCommon;

        public override void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanel(_Container, _OnClose);
            LoadCustomCharactersArgsList();
            LoadCustomCharacterColorSetsArgsList();
            InitPanelItems();
            AssignCurrentItems();
            Managers.ScoreManager.GameSaved -= OnGameSaved;
            Managers.ScoreManager.GameSaved += OnGameSaved;
        }
        
        protected override void OnDialogStartAppearing()
        {
            base.OnDialogStartAppearing();
            UpdateState();
        }

        #endregion

        #region nonpublic methods
        
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
                    Managers.ShopManager,
                    Managers.ScoreManager,
                    Managers.AnalyticsManager,
                    GetCharacterLevel,
                    CommandsProceeder,
                    _Args);
                viewComponent.Selected += _Id =>
                {
                    foreach (var viewComp in m_CustomCharacterItemsList
                        .Where(_Item => _Item.CharacterItemArgsFull.CharacterId != _Id))
                    {
                        viewComp.Select(false);
                    }
                };
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
                    Managers.ShopManager,
                    Managers.ScoreManager,
                    Managers.AnalyticsManager,
                    GetCharacterLevel,
                    ColorProvider,
                    _Args);
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
            return scrObj.set.Select(_Item =>
            {
                var itemOut = GetPanelItemArgsFullFromAssetItem
                    <CustomizeCharacterPanelCharacterItemArgsFull, CustomCharactersAssetItem>(_Item);
                itemOut.CharacterId         = _Item.characterId;
                itemOut.CharacterIconSprite = _Item.icon;
                return itemOut;
            }).ToList();
        }

        private List<CustomizeCharacterPanelColorSetItemArgsFull> LoadCustomCharacterColorSetsArgsList()
        {
            var scrObj = Managers.PrefabSetManager.GetObject<CustomCharacterColorsSetAssetItemsSetScriptableObject>(
                CommonPrefabSetNames.Configs, "custom_character_colors_set_set");
            return scrObj.set.Select(_Item =>
            {
                var itemOut = GetPanelItemArgsFullFromAssetItem
                    <CustomizeCharacterPanelColorSetItemArgsFull, CustomCharacterColorsSetAssetItem>(_Item);
                itemOut.ColorSetId = _Item.colorSetId;
                itemOut.Color1     = _Item.color1;
                itemOut.Color2     = _Item.color2;
                return itemOut;
            }).ToList();
        }
        
        private T1 GetPanelItemArgsFullFromAssetItem<T1, T2>(T2 _CustomCharactersAssetItem) 
            where T1 : CustomizeCharacterPanelCharacterItemArgsFullBase, new()
            where T2 : CustomCharactersAssetItemBase
        {
            return new T1
            {
                CoastArgs = new CustomizeCharacterItemCoastArgs
                {
                    GameMoneyCoast = () => _CustomCharactersAssetItem.gameMoneyCoast,
                    ShopItemArgs = Managers.ShopManager.GetItemInfo(_CustomCharactersAssetItem.purchaseKey),
                    PurchaseKey = _CustomCharactersAssetItem.purchaseKey
                },
                AccessConditionArgs = new CustomizeCharacterItemAccessConditionArgs
                {
                    CharacterLevel = _CustomCharactersAssetItem.characterLevelToOpen
                },
                HasReceiptArgs = new CustomizeCharacterItemHasReceiptArgs
                {
                    HasReceipt = Managers.ShopManager.GetItemInfo(_CustomCharactersAssetItem.purchaseKey).HasReceipt
                }
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
            int charIdCached = SaveUtils.GetValue(SaveKeysRmazor.CharacterId);
            if (charIdCached == default)
            {
                charIdCached = 1;
                SaveUtils.PutValue(SaveKeysRmazor.CharacterId, charIdCached);
            }
            foreach (var characterItem in m_CustomCharacterItemsList)
            {
                int itecCharId = characterItem.CharacterItemArgsFull.CharacterId;
                characterItem.Select(itecCharId == charIdCached);
            }
        }

        private void SelectCurrentColorSetItem()
        {
            int colorSetIdCached = SaveUtils.GetValue(SaveKeysRmazor.CharacterColorSetId);
            if (colorSetIdCached == default)
            {
                colorSetIdCached = 1;
                SaveUtils.PutValue(SaveKeysRmazor.CharacterColorSetId, colorSetIdCached);
            }
            foreach (var customColorSet in m_CustomColorSetItemsList)
            {
                int itemColorSetId = customColorSet.ColorSetItemArgsFull.ColorSetId;
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

        private void AssignCurrentCharacter()
        {
            int characterId = SaveUtils.GetValue(SaveKeysRmazor.CharacterColorSetId);
            if (characterId == default)
                characterId = 1;
            m_CustomCharacterItemsList.FirstOrDefault(_Item =>
                _Item.CharacterItemArgsFull.CharacterId == characterId)!.Select(true);
        }

        private void AssignCurrentColorSet()
        {
            int colorSetId = SaveUtils.GetValue(SaveKeysRmazor.CharacterColorSetId);
            if (colorSetId == default)
                colorSetId = 1;
            m_CustomColorSetItemsList.FirstOrDefault(_Item =>
                _Item.ColorSetItemArgsFull.ColorSetId == colorSetId)!.Select(true);
        }
        
        protected override void GetPrefabContentObjects(GameObject _Go)
        {
            m_CharacterIcon              = _Go.GetCompItem<RawImage>("character_icon");
            m_CharacterLevelText         = _Go.GetCompItem<TextMeshProUGUI>("character_level_text");
            m_ButtonAddMoney             = _Go.GetCompItem<Button>("add_money_button");
            m_BankMoneyText              = _Go.GetCompItem<TextMeshProUGUI>("bank_money_text");
            m_ButtonClose                = _Go.GetCompItem<Button>("button_close");
            m_CustomCharactersTitle      = _Go.GetCompItem<TextMeshProUGUI>("custom_characters_title");
            m_CustomColorSetsTitle       = _Go.GetCompItem<TextMeshProUGUI>("custom_color_sets_title");
            m_CustomCharactersScrollRect = _Go.GetCompItem<ScrollRect>("custom_characters_scroll_rect");
            m_CustomColorSetsScrollRect  = _Go.GetCompItem<ScrollRect>("custom_color_sets_scroll_rect");
            m_CustomCharactersContent    = _Go.GetCompItem<RectTransform>("custom_characters_content");
            m_CustomColorSetsContent     = _Go.GetCompItem<RectTransform>("custom_color_sets_content");
        }

        protected override void LocalizeTextObjectsOnLoad()
        {
            static string TextFormula(string _Text) => _Text.ToUpper(CultureInfo.CurrentUICulture);
            var locTextInfos = new[]
            {
                new LocTextInfo(m_CustomCharactersTitle, ETextType.MenuUI, "characters", TextFormula),
                new LocTextInfo(m_CustomColorSetsTitle,  ETextType.MenuUI, "color_sets", TextFormula),
                new LocTextInfo(m_CharacterLevelText,    ETextType.MenuUI, "empty_key",
                    _T => GetCharacterLevel().ToString(), ETextLocalizationType.OnlyText), 
                new LocTextInfo(m_BankMoneyText,         ETextType.MenuUI, "empty_key",
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
            var dv = DialogViewersController.GetViewer(ShopDialogPanel.DialogViewerId);
            dv.Show(ShopDialogPanel, _AdditionalCameraEffectsAction: AdditionalCameraEffectsActionDefaultCoroutine);
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