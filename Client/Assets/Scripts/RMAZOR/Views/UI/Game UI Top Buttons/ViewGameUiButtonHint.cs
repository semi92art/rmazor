using Common.Managers;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using RMAZOR.Constants;
using RMAZOR.Models;
using RMAZOR.UI.Panels;
using RMAZOR.Views.InputConfigurators;
using UnityEngine;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Views.UI.Game_UI_Top_Buttons
{
    public interface IViewGameUiButtonHint : IViewGameUiButton
    {
        float StartDelayInSecs { get; }
    }
    
    public class ViewGameUiButtonHint : ViewGameUiButtonBase, IViewGameUiButtonHint
    {
        #region nonpublic members

        private Animator m_ButtonAnimator;

        protected override bool CanShow
        {
            get
            {
                if (Model.LevelStaging.LevelTime < StartDelayInSecs - 0.01f)
                    return false;
                string gameMode = (string) Model.LevelStaging.Arguments.GetSafe(KeyGameMode, out _);
                return gameMode == ParameterGameModePuzzles;
            }
        }
        protected override string PrefabName => "hint_button";

        #endregion

        #region inject
        
        private IHintDialogPanel HintDialogPanel { get; }
        
        private ViewGameUiButtonHint(
            IModelGame                  _Model,
            ICameraProvider             _CameraProvider,
            IViewInputCommandsProceeder _CommandsProceeder,
            IPrefabSetManager           _PrefabSetManager,
            IHapticsManager             _HapticsManager,
            IViewInputTouchProceeder    _TouchProceeder,
            IAnalyticsManager           _AnalyticsManager,
            IHintDialogPanel            _HintDialogPanel)
            : base(
                _Model,
                _CameraProvider,
                _CommandsProceeder, 
                _PrefabSetManager,
                _HapticsManager,
                _TouchProceeder,
                _AnalyticsManager)
        {
            HintDialogPanel = _HintDialogPanel;
        }

        #endregion

        #region api
        
        public float StartDelayInSecs => 10f;

        public override void ShowControls(bool _Show, bool _Instantly)
        {
            base.ShowControls(_Show, _Instantly);
            if (_Show)
                m_ButtonAnimator.SetTrigger(AnimKeys.Anim);
        }

        #endregion
        
        #region nonpublic methods
        
        protected override Vector2 GetPosition(Camera _Camera)
        {
            var visibleBounds = GetVisibleBounds(_Camera);
            float xPos = visibleBounds.max.x - 1f;
            float yPos = visibleBounds.max.y - 2f * TopOffset;
            return new Vector2(xPos, yPos);
        }

        protected override void OnButtonPressed()
        {
            AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.HintButtonClick);
            CallCommand(EInputCommand.HintPanel);
        }

        protected override void InitButton()
        {
            base.InitButton();
            m_ButtonAnimator = ButtonOnRaycast.GetCompItem<Animator>("animator");
            HintDialogPanel.OnClosePanelAction += () => ShowControls(false, true);
        }

        #endregion
    }
}