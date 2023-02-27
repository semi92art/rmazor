using Common;
using Common.Managers;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Utils;
using RMAZOR.Constants;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using UnityEngine;

namespace RMAZOR.Views.UI.Game_UI_Top_Buttons
{
    public interface IViewGameUiButtonDisableAds : IViewGameUiButton { }
    
    public class ViewGameUiButtonDisableAds : ViewGameUiButtonBase, IViewGameUiButtonDisableAds
    {
        #region nonpublic members

        protected override string PrefabName => "disable_ads_button";
        // protected override bool   CanShow
        // {
        //     get
        //     {
        //         var saveKeyValue = SaveUtils.GetValue(SaveKeysMazor.DisableAds);
        //         return !saveKeyValue.HasValue || !saveKeyValue.Value;
        //     }
        // }

        protected override bool CanShow => false;

        #endregion

        #region inject
        
        private ViewGameUiButtonDisableAds(
            IModelGame                  _Model,
            ICameraProvider             _CameraProvider,
            IViewInputCommandsProceeder _CommandsProceeder,
            IPrefabSetManager           _PrefabSetManager,
            IHapticsManager             _HapticsManager,
            IViewInputTouchProceeder    _TouchProceeder,
            IAnalyticsManager           _AnalyticsManager) 
            : base(
                _Model, 
                _CameraProvider, 
                _CommandsProceeder, 
                _PrefabSetManager, 
                _HapticsManager,
                _TouchProceeder, 
                _AnalyticsManager) { }

        #endregion

        #region nonpublic methods
        
        protected override Vector2 GetPosition(Camera _Camera)
        {
            var visibleBounds = GetVisibleBounds(_Camera);
            float xPos = visibleBounds.min.x + 1f;
            float yPos = visibleBounds.max.y - TopOffset;
            return new Vector2(xPos, yPos);
        }

        protected override void OnButtonPressed()
        {
            AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.DisableAdsMainButtonClick);
            CallCommand(EInputCommand.DisableAdsPanel);
        }

        #endregion
    }
}