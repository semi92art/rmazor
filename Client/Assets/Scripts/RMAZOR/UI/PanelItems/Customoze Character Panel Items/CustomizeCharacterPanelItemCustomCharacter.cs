using System;
using System.Collections.Generic;
using Common.Managers.PlatformGameServices;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Managers.IAP;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Constants;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using UnityEngine;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Customoze_Character_Panel_Items
{
    public class CustomizeCharacterPanelCharacterItemArgsFull
        : CustomizeCharacterPanelCharacterItemArgsFullBase
    {
        public Sprite                                   CharacterIconSprite { get; set; }
        public int                                      CharacterId         { get; set; }
    }
    
    public class CustomizeCharacterPanelItemCustomCharacter : CustomizeCharacterPanelItemBase
    {
        #region serialized fields

        [SerializeField] private Image characterIcon;

        #endregion

        #region nonpublic members

        protected override int InternalId => CharacterItemArgsFull.CharacterId;
        
        private IViewInputCommandsProceeder CommandsProceeder { get; set; }

        #endregion

        #region api
        
        public CustomizeCharacterPanelCharacterItemArgsFull CharacterItemArgsFull { get; private set; }

        public void Init(
            IUITicker                                    _UITicker,
            IAudioManager                                _AudioManager,
            ILocalizationManager                         _LocalizationManager,
            IShopManager                                 _ShopManager,
            IScoreManager                                _ScoreManager,
            IAnalyticsManager                            _AnalyticsManager,
            Func<int>                                    _GetCharacterLevel,
            IViewInputCommandsProceeder                  _CommandsProceeder,
            CustomizeCharacterPanelCharacterItemArgsFull _CharacterItemArgs)
        {
            CharacterItemArgsFull = _CharacterItemArgs;
            CommandsProceeder     = _CommandsProceeder;
            base.Init(
                _UITicker, 
                _AudioManager, 
                _LocalizationManager,
                _ShopManager,
                _ScoreManager,
                _AnalyticsManager,
                _CharacterItemArgs.AccessConditionArgs, 
                _CharacterItemArgs.CoastArgs,
                _CharacterItemArgs.HasReceiptArgs,
                _GetCharacterLevel);
        }

        public override void UpdateState()
        {
            base.UpdateState();
            UpdateCharacterIcon();
        }

        #endregion

        #region nonpublic methods
        
        protected override void SetThisItem()
        {
            var args = new Dictionary<string, object>
            {
                {ComInComArg.KeyCharacterId, CharacterItemArgsFull.CharacterId}
            };
            CommandsProceeder.RaiseCommand(EInputCommand.SelectCharacter, args);
            SaveUtils.PutValue(SaveKeysRmazor.CharacterId, CharacterItemArgsFull.CharacterId);
        }

        private void UpdateCharacterIcon()
        {
            characterIcon.sprite = CharacterItemArgsFull.CharacterIconSprite;
        }

        protected override void UpdateAccessState()
        {
            base.UpdateAccessState();
            bool accessibleForUse = IsItemAccessibleForUse();
            if (!accessibleForUse)
                return;
            var iconRtr = characterIcon.rectTransform;
            iconRtr.anchorMin        = Vector2.one * 0.5f;
            iconRtr.anchorMax        = Vector2.one * 0.5f;
            iconRtr.anchoredPosition = Vector2.zero;
            iconRtr.pivot            = Vector2.one * 0.5f;
            iconRtr.sizeDelta        = Vector2.one * 78f;
        }

        protected override void OnBuyForGameMoneyButtonClick()
        {
            PlayButtonClickSound();
            var args = new Dictionary<string, object>
            {{AnalyticIdsRmazor.ParameterCharacterId, CharacterItemArgsFull.CharacterId}};
            AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.BuyCharacterForGameMoneyButtonClick, args);
            base.OnBuyForGameMoneyButtonClick();
        }

        protected override void OnBuyForRealMoneyButtonClick()
        {
            PlayButtonClickSound();
            var args = new Dictionary<string, object>
                {{AnalyticIdsRmazor.ParameterCharacterId, CharacterItemArgsFull.CharacterId}};
            AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.BuyCharacterForRealMoneyButtonClick, args);
            base.OnBuyForRealMoneyButtonClick();
        }

        #endregion
    }
}