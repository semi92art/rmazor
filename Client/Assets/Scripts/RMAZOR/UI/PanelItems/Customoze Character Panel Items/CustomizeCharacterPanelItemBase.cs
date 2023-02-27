using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common;
using Common.Managers.PlatformGameServices;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Managers.IAP;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Customoze_Character_Panel_Items
{ 
    public class CustomizeCharacterItemAccessConditionArgs
    {
        public int CharacterLevel { get; set; }
    }

    public class CustomizeCharacterItemHasReceiptArgs
    {
        public bool HasReceipt { get; set; }
    }

    public class CustomizeCharacterItemCoastArgs
    {
        public Func<int>    GameMoneyCoast          { get; set; }
        public ShopItemArgs ShopItemArgs            { get; set; }
        public int          PurchaseKey             { get; set; }
    }
    
    public class CustomizeCharacterPanelCharacterItemArgsFullBase
    {
        public CustomizeCharacterItemAccessConditionArgs AccessConditionArgs { get; set; }
        public CustomizeCharacterItemCoastArgs           CoastArgs           { get; set; }
        public CustomizeCharacterItemHasReceiptArgs      HasReceiptArgs      { get; set; }
    }
    
    public abstract class CustomizeCharacterPanelItemBase : SimpleUiItem
    {
        #region serialized fields

        [SerializeField] private Button button;

        [SerializeField] private Button          buyForGameMoneyButton;
        [SerializeField] private TextMeshProUGUI buyForGameMoneyButtonText;
        [SerializeField] private Image           buyForGameMoneyButtonBackground;
        [SerializeField] private Image           gameMoneyIcon;

        [SerializeField] private Image           orBackground;
        [SerializeField] private TextMeshProUGUI orText;
        
        [SerializeField] private Button          buyForRealMoneyButton;
        [SerializeField] private TextMeshProUGUI buyForRealMoneyButtonText;
        [SerializeField] private Image           buyForRealMoneyButtonBackground;

        [SerializeField] private Image           lockIcon;
        [SerializeField] private Image           characterLevelBackground;
        [SerializeField] private TextMeshProUGUI characterLevelText;

        [SerializeField] private   Image    checkMarkIcon;
        [SerializeField] protected Animator loadingAnim;
        
        #endregion

        #region nonpublic members

        private CustomizeCharacterItemAccessConditionArgs m_AccessConditionArgs;
        private CustomizeCharacterItemCoastArgs           m_ItemCoastArgs;
        private CustomizeCharacterItemHasReceiptArgs      m_HasReceiptArgs;
        
        private Func<int> m_GetCharacterLevel;
        
        protected abstract int InternalId { get; }

        private   IShopManager      ShopManager      { get; set; }
        private   IScoreManager     ScoreManager     { get; set; }
        protected IAnalyticsManager AnalyticsManager { get; set; }

        #endregion

        #region api

        public event UnityAction<int> Selected;

        public void Select(bool _Selected)
        {
            checkMarkIcon.enabled = _Selected;
            if (!_Selected)
                return;
            SetThisItem();
            Selected?.Invoke(InternalId);
        }

        public virtual void UpdateState()
        {
            UpdateAccessState();
        }

        #endregion

        #region nonpublic methods

        protected abstract void SetThisItem();
        
        protected void Init(
            IUITicker                                 _UITicker,
            IAudioManager                             _AudioManager,
            ILocalizationManager                      _LocalizationManager,
            IShopManager                              _ShopManager,
            IScoreManager                             _ScoreManager,
            IAnalyticsManager                         _AnalyticsManager,
            CustomizeCharacterItemAccessConditionArgs _AccessConditionArgs,
            CustomizeCharacterItemCoastArgs           _ItemCoastArgs,
            CustomizeCharacterItemHasReceiptArgs      _HasReceiptArgs,
            Func<int>                                 _GetCharacterLevel)
        {
            base.Init(_UITicker, _AudioManager, _LocalizationManager);
            ShopManager           = _ShopManager;
            ScoreManager          = _ScoreManager;
            AnalyticsManager      = _AnalyticsManager;
            m_AccessConditionArgs = _AccessConditionArgs;
            m_ItemCoastArgs       = _ItemCoastArgs;
            m_HasReceiptArgs      = _HasReceiptArgs;
            m_GetCharacterLevel   = _GetCharacterLevel;
            LocalizeTextObjectsOnInit();
            SubscribeButtonEvents();
            AddPurchaseActionToShopManager();
            ProceedLoadingRealPriceInfo();
        }
        
        protected virtual void UpdateAccessState()
        {
            bool accessibleForUse      = IsItemAccessibleForUse();
            bool accessibleForPurchase = IsItemAccessibleForPurchase(out bool accessibleForGameMoneyPurchase);
            buyForGameMoneyButton.interactable = accessibleForPurchase && accessibleForGameMoneyPurchase;
            buyForRealMoneyButton.interactable = accessibleForPurchase;
            buyForGameMoneyButton.SetGoActive(!accessibleForUse);
            buyForRealMoneyButton.SetGoActive(!accessibleForUse);
            orText      .enabled = !accessibleForUse;
            orBackground.enabled = !accessibleForUse;
            
            button.interactable  = accessibleForUse;
            
            orText.color                    = GetAccessibleColor(accessibleForPurchase);
            buyForGameMoneyButtonText.color = GetAccessibleColor(accessibleForPurchase && accessibleForGameMoneyPurchase);
            buyForRealMoneyButtonText.color = GetAccessibleColor(accessibleForPurchase);
            gameMoneyIcon.color = GetAccessibleColor(accessibleForPurchase && accessibleForGameMoneyPurchase);
            lockIcon.enabled = characterLevelBackground.enabled = characterLevelText.enabled = !accessibleForPurchase;
        }

        private void LocalizeTextObjectsOnInit()
        {
            const string emptyLocKey = "empty_key";
            var locTextInfos = new []
            {
                new LocTextInfo(buyForGameMoneyButtonText, ETextType.MenuUI, emptyLocKey,
                    _T => m_ItemCoastArgs.GameMoneyCoast().ToString()),
                new LocTextInfo(buyForRealMoneyButtonText, ETextType.MenuUI, emptyLocKey,
                    _TextLocalizationType: ETextLocalizationType.OnlyFont),
                new LocTextInfo(characterLevelText, ETextType.MenuUI, emptyLocKey,
                    _T => m_AccessConditionArgs.CharacterLevel.ToString(), 
                    ETextLocalizationType.OnlyText), 
                new LocTextInfo(orText, ETextType.MenuUI, "or",
                    _T => _T.ToUpper(CultureInfo.CurrentUICulture)), 
            };
            foreach (var locTextInfo in locTextInfos)
                LocalizationManager.AddLocalization(locTextInfo);
        }

        private void SubscribeButtonEvents()
        {
            button               .SetOnClick(OnMainButtonClick);
            buyForGameMoneyButton.SetOnClick(OnBuyForGameMoneyButtonClick);
            buyForRealMoneyButton.SetOnClick(OnBuyForRealMoneyButtonClick);
        }

        private void AddPurchaseActionToShopManager()
        {
            ShopManager.AddPurchaseAction(m_ItemCoastArgs.PurchaseKey, SetItemAsAvailableForUse);
        }
        
        private void ProceedLoadingRealPriceInfo()
        {
            IndicateLoading(true);
            var shopItemArgs = m_ItemCoastArgs.ShopItemArgs;
            Cor.Run(Cor.WaitWhile(
                () => shopItemArgs.Result() == EShopProductResult.Pending,
                () =>
                {
                    IndicateLoading(false);
                    buyForRealMoneyButtonText.text = shopItemArgs.LocalizedPriceString;
                }));
        }

        private void OnMainButtonClick()
        {
            PlayButtonClickSound();
            Select(true);
        }

        protected virtual void OnBuyForGameMoneyButtonClick()
        {
            var savedGame = ScoreManager.GetSavedGame(MazorCommonData.SavedGameFileName);
            object bankMoneyCountArg = savedGame.Arguments.GetSafe(ComInComArg.KeyMoneyCount, out _);
            long money = Convert.ToInt64(bankMoneyCountArg);
            money -= m_ItemCoastArgs.GameMoneyCoast();
            savedGame.Arguments.SetSafe(ComInComArg.KeyMoneyCount, money);
            ScoreManager.SaveGame(savedGame);
            SetItemAsAvailableForUse();
        }

        protected virtual void OnBuyForRealMoneyButtonClick()
        {
            ShopManager.Purchase(m_ItemCoastArgs.PurchaseKey);
        }

        private void SetItemAsAvailableForUse()
        {
            var boughtPurchaseIds = SaveUtils.GetValue(SaveKeysMazor.BoughtPurchaseIds) ??
                                    new List<int>();
            boughtPurchaseIds.Add(m_ItemCoastArgs.PurchaseKey);
            boughtPurchaseIds = boughtPurchaseIds.Distinct().ToList();
            SaveUtils.PutValue(SaveKeysMazor.BoughtPurchaseIds, boughtPurchaseIds);
            UpdateAccessState();
        }

        private bool IsItemAccessibleForPurchase(out bool _AccessibleForGameMoneyPurchase)
        {
            var savedGame = ScoreManager.GetSavedGame(MazorCommonData.SavedGameFileName);
            object bankMoneyCountArg = savedGame.Arguments.GetSafe(ComInComArg.KeyMoneyCount, out _);
            long money = Convert.ToInt64(bankMoneyCountArg);
            _AccessibleForGameMoneyPurchase = money >= m_ItemCoastArgs.GameMoneyCoast();
            return m_GetCharacterLevel() >= m_AccessConditionArgs.CharacterLevel;
        }

        protected bool IsItemAccessibleForUse()
        {
            bool isItemAvailableCached = (SaveUtils.GetValue(SaveKeysMazor.BoughtPurchaseIds) ??
                                         new List<int>()).Contains(m_ItemCoastArgs.PurchaseKey);
            return m_HasReceiptArgs.HasReceipt || isItemAvailableCached || m_ItemCoastArgs.GameMoneyCoast() == 0;
        }

        private static Color GetAccessibleColor(bool _ItemOpened)
        {
            return _ItemOpened ? Color.white : Color.gray;
        }
        
        private void IndicateLoading(bool _Indicate)
        {
            buyForGameMoneyButtonText.SetGoActive(!_Indicate);
            loadingAnim.SetGoActive(_Indicate);
            loadingAnim.enabled = _Indicate;
        }

        #endregion
    }
}