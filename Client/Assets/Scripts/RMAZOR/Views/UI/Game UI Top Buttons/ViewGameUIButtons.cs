using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Constants;
using mazing.common.Runtime;
using mazing.common.Runtime.Managers.IAP;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using UnityEngine;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Views.UI.Game_UI_Top_Buttons
{
    public interface IViewGameUIButtons 
        : IInitViewUIItem, 
          IViewUIGetRenderers, 
          IShowControls,
          IOnLevelStageChanged { }
    
    public class ViewGameUIButtons : IViewGameUIButtons
    {
        #region nonpublic members

        private object[] m_ProceedersCached;
        
        #endregion

        #region inject

        private IViewGameUiButtonSettings   ButtonSettings   { get; }
        private IViewGameUiButtonDisableAds ButtonDisableAds { get; }
        private IViewGameUiButtonHome       ButtonHome       { get; }
        private IViewGameUiButtonDailyGift  ButtonDailyGift  { get; }
        private IViewGameUiButtonRateGame   ButtonRateGame   { get; }
        private IViewGameUiButtonShop       ButtonShop       { get; }
        private IViewGameUiButtonLevels     ButtonLevels     { get; }
        private IViewGameUiButtonHint       ButtonHint       { get; }
        private IShopManager                ShopManager      { get; }
        private IViewGameTicker             ViewGameTicker   { get; }

        private ViewGameUIButtons(
            IViewGameUiButtonSettings   _ButtonSettings,
            IViewGameUiButtonDisableAds _ButtonDisableAds,
            IViewGameUiButtonHome       _ButtonHome,
            IViewGameUiButtonDailyGift  _ButtonDailyGift,
            IViewGameUiButtonRateGame   _ButtonRateGame,
            IViewGameUiButtonShop       _ButtonShop,
            IViewGameUiButtonLevels     _ButtonLevels,
            IViewGameUiButtonHint       _ButtonHint,
            IShopManager                _ShopManager,
            IViewGameTicker             _ViewGameTicker)
        {
            ButtonSettings          = _ButtonSettings;
            ButtonDisableAds        = _ButtonDisableAds;
            ButtonHome              = _ButtonHome;
            ButtonDailyGift         = _ButtonDailyGift;
            ButtonRateGame          = _ButtonRateGame;
            ButtonShop              = _ButtonShop;
            ButtonLevels            = _ButtonLevels;
            ButtonHint              = _ButtonHint;
            ShopManager             = _ShopManager;
            ViewGameTicker          = _ViewGameTicker;
        }

        #endregion

        #region api
        
        public void Init(Vector4 _Offsets)
        {
            CacheProceeders();
            var iInits = GetInterfaceOfProceeders<IInit>();
            foreach (var iInit in iInits)
                iInit.Init();
            ShopManager.AddPurchaseAction(
                PurchaseKeys.NoAds,
                () =>
                {
                    ButtonDisableAds.ShowControls(false, true);
                    ButtonShop.ShowControls(true, true);
                });
        }
        
        public void ShowControls(bool _Show, bool _Instantly)
        {
            var iShowControlsItems = GetInterfaceOfProceeders<IShowControls>()
                .Except(new[] {ButtonHint});
            foreach (var item in iShowControlsItems)
                item.ShowControls(_Show, _Instantly);
            
        }
        
        public IEnumerable<Component> GetRenderers()
        {
            return GetInterfaceOfProceeders<IViewUIGetRenderers>()
                .SelectMany(_R => _R.GetRenderers());
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            ShowOrHideHintIfPuzzleLevelsOnLevelStart(_Args);
        }

        #endregion

        #region nonpublic methods

        private void ShowOrHideHintIfPuzzleLevelsOnLevelStart(LevelStageArgs _Args)
        {
            if (_Args.LevelStage == ELevelStage.None
                || (string) _Args.Arguments[KeyGameMode] != ParameterGameModePuzzles
                || _Args.Arguments.ContainsKey(KeyShowPuzzleLevelHint))
            {
                return;
            }
            switch (_Args.LevelStage)
            {
                case ELevelStage.Loaded:
                    ButtonHint.ShowControls(false, true);
                    break;
                case ELevelStage.StartedOrContinued when _Args.PreviousStage == ELevelStage.ReadyToStart:
                    Cor.Run(ShowHintButtonAfterSomeTimeCoroutine(ButtonHint.StartDelayInSecs));
                    break;
            }
        }

        private IEnumerator ShowHintButtonAfterSomeTimeCoroutine(float _Seconds)
        {
            yield return Cor.Delay(_Seconds, ViewGameTicker);
            ButtonHint.ShowControls(true, true);
        }
        
        private T[] GetInterfaceOfProceeders<T>() where T : class
        {
            return Array.ConvertAll(m_ProceedersCached, _Item => _Item as T);
        }

        private void CacheProceeders()
        {
            m_ProceedersCached = new object[]
            {
                ButtonSettings,
                ButtonDisableAds,
                ButtonHome,
                ButtonDailyGift,
                ButtonRateGame,
                ButtonShop,
                ButtonLevels,
                ButtonHint
            };
        }

        #endregion
    }
}