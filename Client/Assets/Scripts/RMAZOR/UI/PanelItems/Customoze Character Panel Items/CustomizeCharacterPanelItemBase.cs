using System;
using System.Collections.Generic;
using Common;
using Common.Managers.PlatformGameServices;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
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
    
    public class CustomizeCharacterItemCoastArgs
    {
        public Func<int>    GameMoneyCoast          { get; set; }
    }
    
    public abstract class CustomizeCharacterPanelCharacterItemArgsFullBase
    {
        public string                                    Id                  { get; set; }
        public CustomizeCharacterItemAccessConditionArgs AccessConditionArgs { get; set; }
        public CustomizeCharacterItemCoastArgs           CoastArgs           { get; set; }
    }
    
    public abstract class CustomizeCharacterPanelItemBase : SimpleUiItem
    {
        #region serialized fields

        [SerializeField] private Button button;

        [SerializeField] private Button          buyForGameMoneyButton;
        [SerializeField] private TextMeshProUGUI buyForGameMoneyButtonText;

        [SerializeField] private Image           priceBackground;
        [SerializeField] private TextMeshProUGUI priceText;
        
        [SerializeField] private Image           gameMoneyIcon;
        [SerializeField] private Image           checkMarkIcon;
        
        #endregion

        #region nonpublic members
        
        protected abstract SaveKey<List<string>> IdsOfBoughtItemsSaveKey { get; }

        private CustomizeCharacterPanelCharacterItemArgsFullBase m_Args;
        
        private Func<int>   m_GetCharacterLevel;
        private UnityAction m_OpenShopPanel;
        
        private   IScoreManager     ScoreManager     { get; set; }
        protected IAnalyticsManager AnalyticsManager { get; set; }

        #endregion

        #region api

        public event UnityAction<string> Selected;

        public event UnityAction BoughtForGameMoney;

        public void Select(bool _Selected)
        {
            priceBackground.enabled = !_Selected;
            priceText.enabled       = !_Selected;
            gameMoneyIcon.enabled   = !_Selected;
            checkMarkIcon.enabled   = _Selected;
            if (!_Selected)
                return;
            SetThisItem();
            Selected?.Invoke(m_Args.Id);
        }

        public virtual void UpdateState()
        {
            UpdateAccessState();
        }

        #endregion

        #region nonpublic methods

        protected abstract void SetThisItem();
        
        protected void Init(
            IUITicker                                        _UITicker,
            IAudioManager                                    _AudioManager,
            ILocalizationManager                             _LocalizationManager,
            IScoreManager                                    _ScoreManager,
            IAnalyticsManager                                _AnalyticsManager,
            CustomizeCharacterPanelCharacterItemArgsFullBase _FullArgs,
            Func<int>                                        _GetCharacterLevel,
            UnityAction                                      _OpenShopPanel)
        {
            base.Init(_UITicker, _AudioManager, _LocalizationManager);
            ScoreManager          = _ScoreManager;
            AnalyticsManager      = _AnalyticsManager;
            m_Args                = _FullArgs;
            m_GetCharacterLevel   = _GetCharacterLevel;
            m_OpenShopPanel       = _OpenShopPanel;
            LocalizeTextObjectsOnInit();
            SubscribeButtonEvents();
        }
        
        private void UpdateAccessState()
        {
            bool accessibleForUse      = IsItemAccessibleForUse();
            buyForGameMoneyButton.SetGoActive(!accessibleForUse);
            button.interactable = accessibleForUse;
        }

        protected virtual void LocalizeTextObjectsOnInit()
        {
            var locTextInfos = new []
            {
                new LocTextInfo(buyForGameMoneyButtonText, ETextType.MenuUI_H1, "buy"),
                new LocTextInfo(priceText, ETextType.MenuUI_H3, "empty_key",
                    _T => m_Args.CoastArgs.GameMoneyCoast().ToString()),
            };
            foreach (var locTextInfo in locTextInfos)
                LocalizationManager.AddLocalization(locTextInfo);
        }

        private void SubscribeButtonEvents()
        {
            button               .SetOnClick(OnMainButtonClick);
            buyForGameMoneyButton.SetOnClick(OnBuyForGameMoneyButtonClick);
        }
        
        private void OnMainButtonClick()
        {
            PlayButtonClickSound();
            Select(true);
        }

        private void OnBuyForGameMoneyButtonClick()
        {
            IsItemAccessibleForPurchase(out bool accessibleForGameMoneyPurchase);
            if (accessibleForGameMoneyPurchase)
                BuyForGameMoney();
            else
                m_OpenShopPanel?.Invoke();
        }

        protected virtual void BuyForGameMoney()
        {
            var savedGame = ScoreManager.GetSavedGame(MazorCommonData.SavedGameFileName);
            object bankMoneyCountArg = savedGame.Arguments.GetSafe(ComInComArg.KeyMoneyCount, out _);
            long money = Convert.ToInt64(bankMoneyCountArg);
            money -= m_Args.CoastArgs.GameMoneyCoast();
            savedGame.Arguments.SetSafe(ComInComArg.KeyMoneyCount, money);
            ScoreManager.SaveGame(savedGame);
            SetItemAsAvailableForUse();
            BoughtForGameMoney?.Invoke();
        }

        private void SetItemAsAvailableForUse()
        {
            var idsOfBoughtItems = SaveUtils.GetValue(IdsOfBoughtItemsSaveKey) ?? new List<string>();
            if (!idsOfBoughtItems.Contains(m_Args.Id))
                idsOfBoughtItems.Add(m_Args.Id);
            SaveUtils.PutValue(IdsOfBoughtItemsSaveKey, idsOfBoughtItems);
            UpdateAccessState();
        }

        private bool IsItemAccessibleForPurchase(out bool _AccessibleForGameMoneyPurchase)
        {
            var savedGame = ScoreManager.GetSavedGame(MazorCommonData.SavedGameFileName);
            object bankMoneyCountArg = savedGame.Arguments.GetSafe(ComInComArg.KeyMoneyCount, out _);
            long money = Convert.ToInt64(bankMoneyCountArg);
            _AccessibleForGameMoneyPurchase = money >= m_Args.CoastArgs.GameMoneyCoast();
            return m_GetCharacterLevel() >= m_Args.AccessConditionArgs.CharacterLevel;
        }

        private bool IsItemAccessibleForUse()
        {
            bool fullCustomizationUnlocked = SaveUtils.GetValue(SaveKeysRmazor.FullCustomizationUnlocked);
            if (fullCustomizationUnlocked)
                return true;
            var idsOfBoughtItems = SaveUtils.GetValue(IdsOfBoughtItemsSaveKey) ?? new List<string>();
            bool isItemAvailableCached = idsOfBoughtItems.Contains(m_Args.Id);
            return isItemAvailableCached || m_Args.CoastArgs.GameMoneyCoast() == 0;
        }

        #endregion
    }
}