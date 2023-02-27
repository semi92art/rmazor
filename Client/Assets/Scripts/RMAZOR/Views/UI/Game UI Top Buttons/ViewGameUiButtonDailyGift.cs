using Common.Managers;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using RMAZOR.Constants;
using RMAZOR.Models;
using RMAZOR.UI.Panels;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using UnityEngine;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Views.UI.Game_UI_Top_Buttons
{
    public interface IViewGameUiButtonDailyGift : IViewGameUiButton
    {
        bool CanShowDailyGiftPanel { get; }
    }
    
    public class ViewGameUiButtonDailyGift : ViewGameUiButtonBase, IViewGameUiButtonDailyGift
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
        
        // protected override bool CanShow => CanShowDailyGiftPanel 
        //                                    && (Model.LevelStaging.LevelIndex > 0 || IsNextLevelBonus);

        protected override bool   CanShow    => false;
        protected override string PrefabName => "daily_gift_button";

        #endregion

        #region inject
        
        private IDailyGiftPanel DailyGiftPanel { get; }
        
        private ViewGameUiButtonDailyGift(
            IModelGame                  _Model,
            ICameraProvider             _CameraProvider,
            IViewInputCommandsProceeder _CommandsProceeder,
            IPrefabSetManager           _PrefabSetManager,
            IHapticsManager             _HapticsManager,
            IViewInputTouchProceeder    _TouchProceeder,
            IAnalyticsManager           _AnalyticsManager,
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
            DailyGiftPanel = _DailyGiftPanel;
        }

        #endregion

        #region api

        public bool CanShowDailyGiftPanel => DailyGiftPanel.IsDailyGiftAvailableToday;

        #endregion

        #region nonpublic methods

        protected override void InitButton()
        {
            DailyGiftPanel.OnClose += () => ShowControls(false, true);
            base.InitButton();
            var renderer1 = ButtonOnRaycast.GetCompItem<SpriteRenderer>("sprite_2");
            renderer1.sortingOrder = SortingOrders.GameUI;
            Renderers.Add(renderer1);
            
        }
        
        protected override Vector2 GetPosition(Camera _Camera)
        {
            var visibleBounds = GetVisibleBounds(_Camera);
            float xPos = visibleBounds.min.x + 6f;
            float yPos = visibleBounds.max.y - TopOffset;
            return new Vector2(xPos, yPos);
        }

        protected override void OnButtonPressed()
        {
            AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.DailyGiftButtonClick);
            CallCommand(EInputCommand.DailyGiftPanel);
        }

        #endregion
    }
}