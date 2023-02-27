using Common.Managers;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Managers.IAP;
using mazing.common.Runtime.Utils;
using RMAZOR.Constants;
using RMAZOR.Models;
using RMAZOR.UI.Panels;
using RMAZOR.Views.InputConfigurators;
using UnityEngine;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Views.UI.Game_UI_Top_Buttons
{
    public interface IViewGameUiButtonRateGame : IViewGameUiButton { }
    
    public class ViewGameUiButtonRateGame : ViewGameUiButtonBase, IViewGameUiButtonRateGame
    {
        #region nonpublic members
        
        // private bool IsNextLevelBonus
        // {
        //     get
        //     {
        //         string nextLevelType = (string) Model.LevelStaging.Arguments.GetSafe(KeyNextLevelType, out _);
        //         return nextLevelType == ParameterLevelTypeBonus;
        //     }
        // }

        // protected override bool CanShow =>
        //     !ButtonDailyGift.CanShowDailyGiftPanel
        //     && ((Model.LevelStaging.LevelIndex > 8 && !IsNextLevelBonus)
        //         || (Model.LevelStaging.LevelIndex > 2 && IsNextLevelBonus));
        
        protected override bool CanShow => false;

        protected override string PrefabName => "rate_game_button";

        #endregion

        #region inject
        
        private IShopManager               ShopManager     { get; }
        private IDailyGiftPanel            DailyGiftPanel  { get; }
        private IViewGameUiButtonDailyGift ButtonDailyGift { get; }

        private ViewGameUiButtonRateGame(
            IModelGame                  _Model,
            ICameraProvider             _CameraProvider,
            IViewInputCommandsProceeder _CommandsProceeder,
            IPrefabSetManager           _PrefabSetManager,
            IHapticsManager             _HapticsManager,
            IViewInputTouchProceeder    _TouchProceeder,
            IAnalyticsManager           _AnalyticsManager,
            IShopManager                _ShopManager,
            IDailyGiftPanel             _DailyGiftPanel,
            IViewGameUiButtonDailyGift  _ButtonDailyGift)
            : base(
                _Model,
                _CameraProvider,
                _CommandsProceeder, 
                _PrefabSetManager, 
                _HapticsManager, 
                _TouchProceeder, 
                _AnalyticsManager)
        {
            ShopManager     = _ShopManager;
            DailyGiftPanel  = _DailyGiftPanel;
            ButtonDailyGift = _ButtonDailyGift;
        }

        #endregion

        #region nonpublic methods

        protected override void InitButton()
        {
            DailyGiftPanel.OnClose += () => ShowControls(CanShow, true);
            base.InitButton();
        }

        protected override Vector2 GetPosition(Camera _Camera)
        {
            var visibleBounds = GraphicUtils.GetVisibleBounds(_Camera);
            float xPos = visibleBounds.min.x + 6f;
            float yPos = visibleBounds.max.y - TopOffset;
            return new Vector2(xPos, yPos);
        }

        protected override void OnButtonPressed()
        {
            AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.RateGameMainButtonClick);
            ShopManager.RateGame();
            SaveUtils.PutValue(SaveKeysCommon.GameWasRated, true);
        }

        #endregion
    }
}