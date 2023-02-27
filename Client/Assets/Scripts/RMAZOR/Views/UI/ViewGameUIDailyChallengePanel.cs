using Common.Constants;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Views.Controllers;
using RMAZOR.Views.Rotation;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Views.UI
{
    public interface IViewGameUIDailyChallengePanel
        : IInitViewUIItem, 
          IOnLevelStageChanged,
          IShowControls,
          ICharacterMoveFinished,
          IMazeRotationFinished
    {
        event UnityAction ChallengeFailed;
    }
    
    public class ViewGameUIDailyChallengePanel 
        : InitBase,
          IViewGameUIDailyChallengePanel
    {
        #region nonpublic members
        
        private GameObject     m_PanelObj;
        private SpriteRenderer m_IconChallenge;
        private SpriteRenderer m_IconSuccessOrFail;
        private Animator       m_IconSuccessOrFailAnimator;
        private TextMeshPro    m_Text;
        
        #endregion

        #region inject

        private IModelGame                         Model                         { get; }
        private IPrefabSetManager                  PrefabSetManager              { get; }
        private IContainersGetter                  ContainersGetter              { get; }
        private ICameraProvider                    CameraProvider                { get; }
        private ILocalizationManager               LocalizationManager           { get; }
        private IViewDailyChallengeTimeController  DailyChallengeTimeController  { get; }
        private IViewDailyChallengeStepsController DailyChallengeStepsController { get; }

        public ViewGameUIDailyChallengePanel(
            IModelGame                         _Model,
            IPrefabSetManager                  _PrefabSetManager,
            IContainersGetter                  _ContainersGetter,
            ICameraProvider                    _CameraProvider,
            ILocalizationManager               _LocalizationManager,
            IViewDailyChallengeTimeController  _DailyChallengeTimeController,
            IViewDailyChallengeStepsController _DailyChallengeStepsController)
        {
            Model                         = _Model;
            PrefabSetManager              = _PrefabSetManager;
            ContainersGetter              = _ContainersGetter;
            CameraProvider                = _CameraProvider;
            LocalizationManager           = _LocalizationManager;
            DailyChallengeTimeController  = _DailyChallengeTimeController;
            DailyChallengeStepsController = _DailyChallengeStepsController;
        }

        #endregion

        #region api

        public event UnityAction ChallengeFailed;

        public void Init(Vector4 _Offsets)
        {
            InitPanel();
            DailyChallengeTimeController .Init();
            DailyChallengeStepsController.Init();
            DailyChallengeTimeController .GetPanelObjects = GetPanelObjectsArgs;
            DailyChallengeStepsController.GetPanelObjects = GetPanelObjectsArgs;
            DailyChallengeTimeController .ChallengeFailed += ChallengeFailed;
            DailyChallengeStepsController.ChallengeFailed += ChallengeFailed;
            CameraProvider.ActiveCameraChanged  += OnActiveCameraChanged;
            LocalizationManager.LanguageChanged += OnLanguageChanged;
            base.Init();
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            DailyChallengeTimeController .OnLevelStageChanged(_Args);
            DailyChallengeStepsController.OnLevelStageChanged(_Args);
        }

        public void ShowControls(bool _Show, bool _Instantly)
        {
            string gameMode = (string) Model.LevelStaging.Arguments.GetSafe(KeyGameMode, out _);
            bool doShowDailyChallengeTimer = _Show && gameMode == ParameterGameModeDailyChallenge;
            Activate(doShowDailyChallengeTimer);
        }
        
        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            DailyChallengeStepsController.OnCharacterMoveFinished(_Args);
        }
        
        public void OnMazeRotationFinished(MazeRotationEventArgs _Args)
        {
            DailyChallengeStepsController.OnMazeRotationFinished(_Args);
        }

        #endregion

        #region nonpublic methods
        
        private void OnActiveCameraChanged(Camera _Camera)
        {
            var bounds = GraphicUtils.GetVisibleBounds(_Camera);
            var pos = new Vector2(bounds.center.x - 3f, bounds.max.y - 7f);
            m_PanelObj.transform.SetLocalPosXY(pos);
            m_PanelObj.transform.SetLocalScaleXY(Vector2.one * 1f);
        }

        private void OnLanguageChanged(ELanguage _Language)
        {
            m_Text.font = LocalizationManager.GetFont(ETextType.GameUI, _Language);
        }
        
        private void InitPanel()
        {
            const string prefabSetName = CommonPrefabSetNames.UiGame;
            var container = ContainersGetter.GetContainer(ContainerNamesCommon.GameUI);
            m_PanelObj = PrefabSetManager.InitPrefab(
                container, 
                prefabSetName, 
                "daily_challenge_panel");
            m_Text                      = m_PanelObj.GetCompItem<TextMeshPro>("text");
            m_IconChallenge             = m_PanelObj.GetCompItem<SpriteRenderer>("icon_challenge");
            m_IconSuccessOrFail         = m_PanelObj.GetCompItem<SpriteRenderer>("icon_success_or_fail");
            m_IconSuccessOrFailAnimator = m_PanelObj.GetCompItem<Animator>("icon_success_or_fail_animator");
            m_Text.font = LocalizationManager.GetFont(ETextType.GameUI);
            Activate(false);
        }
        
        private void Activate(bool _Active)
        {
            m_IconChallenge.enabled             = _Active;
            m_IconSuccessOrFail.enabled         = _Active;
            m_IconSuccessOrFailAnimator.enabled = _Active;
            m_Text.enabled                      = _Active;
        }

        private ViewGameDailyChallengePanelObjectsArgs GetPanelObjectsArgs()
        {
            return new ViewGameDailyChallengePanelObjectsArgs
            {
                IconChallenge             = m_IconChallenge,
                IconSuccessOrFail         = m_IconSuccessOrFail,
                IconSuccessOrFailAnimator = m_IconSuccessOrFailAnimator,
                Text                      = m_Text,
                PanelObject               = m_PanelObj
            };
        }

        #endregion
    }
}