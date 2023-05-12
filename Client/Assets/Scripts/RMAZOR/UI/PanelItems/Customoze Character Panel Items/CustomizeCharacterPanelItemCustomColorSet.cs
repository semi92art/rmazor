using System;
using System.Collections.Generic;
using System.Globalization;
using Common;
using Common.Managers.PlatformGameServices;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Constants;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Customoze_Character_Panel_Items
{
    public class CustomizeCharacterPanelColorSetItemArgsFull
        : CustomizeCharacterPanelCharacterItemArgsFullBase
    {
        public string ColorSetId          { get; set; }
        public string NameLocalizationKey { get; set; }
        public Color  Color               { get; set; }
    }
    
    public class CustomizeCharacterPanelItemCustomColorSet : CustomizeCharacterPanelItemBase
    {
        #region serialized fields

        [SerializeField] private Image           color1Icon;
        [SerializeField] private TextMeshProUGUI colorSetName;

        #endregion

        #region nonpublic members

        protected override SaveKey<List<string>> IdsOfBoughtItemsSaveKey => SaveKeysRmazor.IdsOfBoughtColorSets;
        
        private IColorProvider ColorProvider { get; set; }

        #endregion

        #region api
        
        public CustomizeCharacterPanelColorSetItemArgsFull ColorSetItemArgsFull { get; private set; }

        public void Init(
            IUITicker                                   _UITicker,
            IAudioManager                               _AudioManager,
            ILocalizationManager                        _LocalizationManager,
            IScoreManager                               _ScoreManager,
            IAnalyticsManager                           _AnalyticsManager,
            Func<int>                                   _GetCharacterLevel,
            IColorProvider                              _ColorProvider,
            CustomizeCharacterPanelColorSetItemArgsFull _ColorSetItemArgsFull,
            UnityAction                                 _OpenShopPanel)
        {
            ColorSetItemArgsFull = _ColorSetItemArgsFull;
            ColorProvider        = _ColorProvider;
            base.Init(
                _UITicker, 
                _AudioManager, 
                _LocalizationManager,
                _ScoreManager,
                _AnalyticsManager,
                _ColorSetItemArgsFull,
                _GetCharacterLevel,
                _OpenShopPanel);
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
            ColorProvider.SetColor(ColorIds.Character,  ColorSetItemArgsFull.Color);
            ColorProvider.SetColor(ColorIds.Character2, Color.black);
            SaveUtils.PutValue(SaveKeysRmazor.CharacterColorSetIdV2, ColorSetItemArgsFull.ColorSetId);
        }

        private void UpdateColorIcons()
        {
            color1Icon.color = SetContrast(ColorSetItemArgsFull.Color, 1.5f);
        }
        
        private static Color SetContrast(Color color, float contrastValue)
        {
            // Convert color to HSV
            Color.RGBToHSV(color, out float h, out float s, out float v);

            // Calculate new value for V based on contrastValue
            float newV = (v > 0.5f) ? v + ((1.0f - v) * contrastValue) : v - (v * contrastValue);

            // Clamp newV value between 0 and 1
            newV = Mathf.Clamp01(newV);

            // Convert HSV back to RGB
            Color newColor = Color.HSVToRGB(h, s, newV);

            return newColor;
        }

        protected override void BuyForGameMoney()
        {
            var args = new Dictionary<string, object>
                {{AnalyticIdsRmazor.ParameterColorSetId, ColorSetItemArgsFull.ColorSetId}};
            AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.BuyColorSetForGameMoneyButtonClick, args);
            base.BuyForGameMoney();
        }
        
        protected override void LocalizeTextObjectsOnInit()
        {
            var locInfo = new LocTextInfo(colorSetName, ETextType.MenuUI_H1,
                ColorSetItemArgsFull.NameLocalizationKey, 
                _T => _T.ToUpper(CultureInfo.CurrentUICulture));
            LocalizationManager.AddLocalization(locInfo);
            base.LocalizeTextObjectsOnInit();
        }

        #endregion
    }
}