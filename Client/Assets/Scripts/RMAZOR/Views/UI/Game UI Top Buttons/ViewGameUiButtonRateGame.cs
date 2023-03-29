﻿using Common.Managers;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Managers.IAP;
using mazing.common.Runtime.Utils;
using RMAZOR.Constants;
using RMAZOR.Models;
using RMAZOR.UI.Panels;
using RMAZOR.Views.InputConfigurators;
using UnityEngine;

namespace RMAZOR.Views.UI.Game_UI_Top_Buttons
{
    public interface IViewGameUiButtonRateGame : IViewGameUiButton { }
    
    public class ViewGameUiButtonRateGame : ViewGameUiButtonBase, IViewGameUiButtonRateGame
    {
        #region nonpublic members
        
        protected override bool CanShow => true;
        
        protected override string PrefabName => "rate_game_button";

        #endregion

        #region inject
        
        private IShopManager               ShopManager     { get; }
        private IDailyGiftPanel            DailyGiftPanel  { get; }

        private ViewGameUiButtonRateGame(
            IModelGame                  _Model,
            ICameraProvider             _CameraProvider,
            IViewInputCommandsProceeder _CommandsProceeder,
            IPrefabSetManager           _PrefabSetManager,
            IHapticsManager             _HapticsManager,
            IViewInputTouchProceeder    _TouchProceeder,
            IAnalyticsManager           _AnalyticsManager,
            IShopManager                _ShopManager,
            IDailyGiftPanel             _DailyGiftPanel)
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