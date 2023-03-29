using System;
using System.Collections.Generic;
using Common.Managers.PlatformGameServices;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Constants;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Customoze_Character_Panel_Items
{
    public class CustomizeCharacterPanelCharacterItemArgsFull
        : CustomizeCharacterPanelCharacterItemArgsFullBase
    {
        public Sprite CharacterIconSprite { get; set; }
        public string NameLocalizationKey { get; set; }
    }
    
    public class CustomizeCharacterPanelItemCustomCharacter : CustomizeCharacterPanelItemBase
    {
        #region serialized fields

        [SerializeField] private Image           characterIcon;
        [SerializeField] private TextMeshProUGUI characterName;

        #endregion

        #region nonpublic members

        protected override SaveKey<List<string>> IdsOfBoughtItemsSaveKey => SaveKeysRmazor.IdsOfBoughtCharacters;
        
        private IViewInputCommandsProceeder CommandsProceeder { get; set; }

        #endregion

        #region api
        
        public CustomizeCharacterPanelCharacterItemArgsFull CharacterItemArgsFull { get; private set; }

        public void Init(
            IUITicker                                    _UITicker,
            IAudioManager                                _AudioManager,
            ILocalizationManager                         _LocalizationManager,
            IScoreManager                                _ScoreManager,
            IAnalyticsManager                            _AnalyticsManager,
            Func<int>                                    _GetCharacterLevel,
            IViewInputCommandsProceeder                  _CommandsProceeder,
            CustomizeCharacterPanelCharacterItemArgsFull _CharacterItemArgs,
            UnityAction                                  _OpenShopPanel)
        {
            CharacterItemArgsFull = _CharacterItemArgs;
            CommandsProceeder     = _CommandsProceeder;
            base.Init(
                _UITicker, 
                _AudioManager, 
                _LocalizationManager,
                _ScoreManager,
                _AnalyticsManager,
                _CharacterItemArgs,
                _GetCharacterLevel,
                _OpenShopPanel);
        }

        public override void UpdateState()
        {
            base.UpdateState();
            UpdateCharacterIconAndName();
        }

        #endregion

        #region nonpublic methods
        
        protected override void SetThisItem()
        {
            var args = new Dictionary<string, object>
            {
                {ComInComArg.KeyCharacterId, CharacterItemArgsFull.Id}
            };
            CommandsProceeder.RaiseCommand(EInputCommand.SelectCharacter, args);
            SaveUtils.PutValue(SaveKeysRmazor.CharacterIdV2, CharacterItemArgsFull.Id);
        }

        private void UpdateCharacterIconAndName()
        {
            characterIcon.sprite = CharacterItemArgsFull.CharacterIconSprite;
            characterName.text = LocalizationManager.GetTranslation(CharacterItemArgsFull.NameLocalizationKey);
            
        }

        protected override void BuyForGameMoney()
        {
            PlayButtonClickSound();
            var args = new Dictionary<string, object>
            {{AnalyticIdsRmazor.ParameterCharacterId, CharacterItemArgsFull.Id}};
            AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.BuyCharacterForGameMoneyButtonClick, args);
            base.BuyForGameMoney();
        }

        protected override void LocalizeTextObjectsOnInit()
        {
            var locInfo = new LocTextInfo(characterName, ETextType.MenuUI_H1,
                CharacterItemArgsFull.NameLocalizationKey);
            LocalizationManager.AddLocalization(locInfo);
            base.LocalizeTextObjectsOnInit();
        }

        #endregion
    }
}