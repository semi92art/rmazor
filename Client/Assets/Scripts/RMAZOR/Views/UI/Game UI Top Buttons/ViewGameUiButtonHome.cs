using Common.Managers;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Managers;
using RMAZOR.Constants;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using UnityEngine;

namespace RMAZOR.Views.UI.Game_UI_Top_Buttons
{
    public interface IViewGameUiButtonHome : IViewGameUiButton { }
    
    public class ViewGameUiButtonHome : ViewGameUiButtonBase, IViewGameUiButtonHome
    {
        #region nonpublic members

        protected override bool   CanShow    => true;
        protected override string PrefabName => "home_button";

        #endregion

        #region inject
        

        private ViewGameUiButtonHome(
            IModelGame                  _Model,
            ICameraProvider             _CameraProvider,
            IViewInputCommandsProceeder _CommandsProceeder,
            IPrefabSetManager           _PrefabSetManager,
            IHapticsManager             _HapticsManager,
            IViewInputTouchProceeder    _TouchProceeder,
            IAnalyticsManager           _AnalyticsManager) 
            : base(_Model, 
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
            float xPos = visibleBounds.max.x - 6f;
            float yPos = visibleBounds.max.y - TopOffset;
            return new Vector2(xPos, yPos);
        }

        protected override void OnButtonPressed()
        {
            AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.HomeButtonClick);
            CommandsProceeder.RaiseCommand(EInputCommand.ConfirmGoToMainMenuPanel, null);
        }

        #endregion
    }
}