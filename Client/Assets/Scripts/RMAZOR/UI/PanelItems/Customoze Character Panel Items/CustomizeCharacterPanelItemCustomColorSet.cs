using System;
using System.Collections.Generic;
using Common;
using Common.Managers.PlatformGameServices;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Managers.IAP;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Constants;
using UnityEngine;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Customoze_Character_Panel_Items
{
    public class CustomizeCharacterPanelColorSetItemArgsFull
        : CustomizeCharacterPanelCharacterItemArgsFullBase
    {
        public int   ColorSetId { get; set; }
        public Color Color1     { get; set; }
        public Color Color2     { get; set; }
    }
    
    public class CustomizeCharacterPanelItemCustomColorSet : CustomizeCharacterPanelItemBase
    {
        #region serialized fields

        [SerializeField] private Image colorIconsBackground;
        [SerializeField] private Image color1Icon, color2Icon;

        #endregion

        #region nonpublic members

        protected override int InternalId => ColorSetItemArgsFull.ColorSetId;
        
        private IColorProvider ColorProvider { get; set; }

        #endregion

        #region api
        
        public CustomizeCharacterPanelColorSetItemArgsFull ColorSetItemArgsFull { get; private set; }

        public void Init(
            IUITicker                                   _UITicker,
            IAudioManager                               _AudioManager,
            ILocalizationManager                        _LocalizationManager,
            IShopManager                                _ShopManager,
            IScoreManager                               _ScoreManager,
            IAnalyticsManager                           _AnalyticsManager,
            Func<int>                                   _GetCharacterLevel,
            IColorProvider                              _ColorProvider,
            CustomizeCharacterPanelColorSetItemArgsFull _ColorSetItemArgsFull)
        {
            ColorSetItemArgsFull = _ColorSetItemArgsFull;
            ColorProvider        = _ColorProvider;
            base.Init(
                _UITicker, 
                _AudioManager, 
                _LocalizationManager,
                _ShopManager,
                _ScoreManager,
                _AnalyticsManager,
                _ColorSetItemArgsFull.AccessConditionArgs,
                _ColorSetItemArgsFull.CoastArgs, 
                _ColorSetItemArgsFull.HasReceiptArgs,
                _GetCharacterLevel);
        }

        public override void UpdateState()
        {
            base.UpdateState();
            UpdateColorIcons();
        }

        #endregion

        #region nonpublic methods
        
        protected override void SetThisItem()
        {
            ColorProvider.SetColor(ColorIds.Character,  ColorSetItemArgsFull.Color1);
            ColorProvider.SetColor(ColorIds.Character2, ColorSetItemArgsFull.Color2);
            SaveUtils.PutValue(SaveKeysRmazor.CharacterColorSetId, ColorSetItemArgsFull.ColorSetId);
        }

        private void UpdateColorIcons()
        {
            color1Icon.color = ColorSetItemArgsFull.Color1;
            color2Icon.color = ColorSetItemArgsFull.Color2;
        }

        protected override void UpdateAccessState()
        {
            base.UpdateAccessState();
            bool accessibleForUse = IsItemAccessibleForUse();
            if (!accessibleForUse)
                return;
            var iconRtr = colorIconsBackground.rectTransform;
            iconRtr.anchorMin        = Vector2.one * 0.5f;
            iconRtr.anchorMax        = Vector2.one * 0.5f;
            iconRtr.anchoredPosition = Vector2.zero;
            iconRtr.pivot            = Vector2.one * 0.5f;
            iconRtr.sizeDelta        = Vector2.one * 78f;
        }
        
        protected override void OnBuyForGameMoneyButtonClick()
        {
            var args = new Dictionary<string, object>
                {{AnalyticIdsRmazor.ParameterColorSetId, ColorSetItemArgsFull.ColorSetId}};
            AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.BuyColorSetForGameMoneyButtonClick, args);
            base.OnBuyForGameMoneyButtonClick();
        }

        protected override void OnBuyForRealMoneyButtonClick()
        {
            var args = new Dictionary<string, object>
                {{AnalyticIdsRmazor.ParameterColorSetId, ColorSetItemArgsFull.ColorSetId}};
            AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.BuyColorSetForRealMoneyButtonClick, args);
            base.OnBuyForRealMoneyButtonClick();
        }
        
        #endregion
    }
}