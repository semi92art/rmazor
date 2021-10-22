using System.Collections.Generic;
using System.Linq;
using Constants;
using DI.Extensions;
using Exceptions;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.InputConfigurators;
using Games.RazorMaze.Views.Utils;
using Shapes;
using Ticker;
using TMPro;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.UI
{
    public interface IViewUIGameControls : IInit, IOnLevelStageChanged
    {
        void OnMazeItemMoveStarted(MazeItemMoveEventArgs _Args);
        void OnMazeItemMoveFinished(MazeItemMoveEventArgs _Args);

    }
    
    public class ViewUIGameControls : ViewUIGameControlsBase
    {
        #region nonpublic members

        private static int AnimKeyCheckMarkPass => AnimKeys.Anim;
        private static int AnimKeyChekMarkSet => AnimKeys.Anim2;
        private static int AnimKeyCheckMarkIdle => AnimKeys.Stop;
        private static int AnimKeyCongratsAnim => AnimKeys.Anim;
        private static int AnimKeyCongratsIdle => AnimKeys.Stop;

        private ButtonOnRaycast m_RotateClockwiseButton;
        private ButtonOnRaycast m_RotateCounterClockwiseButton;
        private ButtonOnRaycast m_ShopButton;
        private ButtonOnRaycast m_SettingsButton;
        private TextMeshPro m_LevelText;
        private TextMeshPro m_CompletedText;
        private TextMeshPro m_CongratsText;
        private Animator m_CongratsAnim;
        private readonly List<object> m_Renderers = new List<object>();
        private readonly List<Animator> m_CheckMarks = new List<Animator>();
        private bool m_ButtonsInitialized;

        #endregion
        
        #region inject

        private ViewSettings ViewSettings { get; }
        private IViewUIPrompts Prompts { get; }
        private IModelGame Model { get; }
        private IContainersGetter ContainersGetter { get; }
        private IMazeCoordinateConverter CoordinateConverter { get; }
        private IViewAppearTransitioner AppearTransitioner { get; }
        private IGameTicker GameTicker { get; }
        private ILevelsLoader LevelsLoader { get; }
        private ILocalizationManager LocalizationManager { get; }

        public ViewUIGameControls(
            ViewSettings _ViewSettings,
            IViewUIPrompts _Prompts,
            IModelGame _Model,
            IContainersGetter _ContainersGetter,
            IMazeCoordinateConverter _CoordinateConverter,
            IViewInputConfigurator _InputConfigurator,
            IViewAppearTransitioner _AppearTransitioner,
            IGameTicker _GameTicker,
            ILevelsLoader _LevelsLoader,
            ILocalizationManager _LocalizationManager) 
            : base(_InputConfigurator)
        {
            ViewSettings = _ViewSettings;
            Prompts = _Prompts;
            Model = _Model;
            ContainersGetter = _ContainersGetter;
            CoordinateConverter = _CoordinateConverter;
            AppearTransitioner = _AppearTransitioner;
            GameTicker = _GameTicker;
            LevelsLoader = _LevelsLoader;
            LocalizationManager = _LocalizationManager;
        }

        #endregion
        
        #region api


        public override void OnMazeItemMoveStarted(MazeItemMoveEventArgs _Args)
        {
            if (Prompts.InTutorial)
                return;
            base.OnMazeItemMoveStarted(_Args);
        }

        public override void OnMazeItemMoveFinished(MazeItemMoveEventArgs _Args)
        {
            if (Prompts.InTutorial)
                return;
            base.OnMazeItemMoveFinished(_Args);
        }
        
        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                case ELevelStage.Paused:
                case ELevelStage.Finished:
                case ELevelStage.ReadyToUnloadLevel:
                case ELevelStage.Unloaded:
                case ELevelStage.CharacterKilled:
                    InputConfigurator.LockAllCommands();
                    break;
                case ELevelStage.ReadyToStartOrContinue:
                case ELevelStage.StartedOrContinued:
                    InputConfigurator.UnlockAllCommands();
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Args.Stage);
            }
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                    if (!m_ButtonsInitialized)
                    {
                        InitGameUI();
                        m_ButtonsInitialized = true;
                    }
                    SetLevelCheckMarks(_Args.LevelIndex, false);
                    ShowControls(true, false);
                    break;
                case ELevelStage.ReadyToStartOrContinue:
                case ELevelStage.StartedOrContinued:
                    bool enableRotation = Model.GetAllProceedInfos().Any(_Info =>
                        _Info.Type == EMazeItemType.GravityBlock || _Info.Type == EMazeItemType.GravityTrap);
                    if (!enableRotation)
                    {
                        InputConfigurator.LockCommand(InputCommands.RotateClockwise);
                        InputConfigurator.LockCommand(InputCommands.RotateCounterClockwise);
                    }
                    break;
                case ELevelStage.Finished:
                    SetCongratsString();
                    m_CongratsAnim.SetTrigger(AnimKeyCongratsAnim);
                    SetLevelCheckMarks(_Args.LevelIndex, true);
                    break;
                case ELevelStage.ReadyToUnloadLevel:
                    m_CongratsAnim.SetTrigger(AnimKeyCongratsIdle);
                    ShowControls(false, false);
                    break;
            }
            Prompts.OnLevelStageChanged(_Args);
        }

        #endregion

        #region nonpublic methds

        private void InitGameUI()
        {
            InitTopButtons();
            InitLevelAndCongratsPanel();
        }
        
        private void InitTopButtons()
        {
            const float topOffset = 1f;
            const float horOffset = 1f;
            float scale = GraphicUtils.AspectRatio * 3f;
            Dbg.Log(scale);
            var bounds = GraphicUtils.VisibleBounds;
            
            var cont = GetGameUIContainer();
            var goShopButton = PrefabUtilsEx.InitPrefab(
                cont, "ui_game", "shop_button");
            var goSettingsButton = PrefabUtilsEx.InitPrefab(
                cont, "ui_game", "settings_button");
            m_Renderers.AddRange( new object[]
            {
                goShopButton.GetCompItem<SpriteRenderer>("button"),
                goSettingsButton.GetCompItem<SpriteRenderer>("button")
            });
            goShopButton.transform.localScale = scale * Vector3.one;
            goShopButton.transform.SetPosXY(
                new Vector2(bounds.min.x, bounds.max.y)
                + Vector2.right * horOffset + Vector2.down * topOffset);
            goSettingsButton.transform.localScale = scale * Vector3.one;
            goSettingsButton.transform.SetPosXY(
                (Vector2)bounds.max
                + Vector2.left * horOffset + Vector2.down * topOffset);
            m_ShopButton = goShopButton.GetCompItem<ButtonOnRaycast>("button");
            m_SettingsButton = goSettingsButton.GetCompItem<ButtonOnRaycast>("button");
            m_ShopButton.OnClickEvent.AddListener(CommandShop);
            m_SettingsButton.OnClickEvent.AddListener(CommandSettings);
            goShopButton.SetActive(false);
            goSettingsButton.SetActive(false);
        }

        private void InitLevelAndCongratsPanel()
        {
            var bounds = GraphicUtils.VisibleBounds;
            var mazeBounds = CoordinateConverter.GetMazeBounds();
            float yPos = bounds.max.y;
            var cont = GetGameUIContainer();
            var goLevelText = PrefabUtilsEx.InitPrefab(
                cont, "ui_game", "level_text");
            m_LevelText = goLevelText.GetCompItem<TextMeshPro>("text");
            m_LevelText.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal, mazeBounds.size.x);
            m_LevelText.transform.position = new Vector3(
                mazeBounds.center.x, 
                yPos);
            var goLevelCheckMark = PrefabUtilsEx.GetPrefab(
                "ui_game", "level_check_mark");
            float markHeight = goLevelCheckMark.GetCompItem<Rectangle>("body").Height;
            for (int i = 0; i < RazorMazeUtils.LevelsInGroup; i++)
            {
                var go = Object.Instantiate(goLevelCheckMark, cont);
                m_CheckMarks.Add(go.GetCompItem<Animator>("animator"));
                m_Renderers.AddRange(new object[]
                {
                    go.GetCompItem<Rectangle>("body"),
                    go.GetCompItem<Line>("checkmark_1"),
                    go.GetCompItem<Line>("checkmark_2")
                });
                float xCoeff1 = 8f * ((i + 0.5f) / RazorMazeUtils.LevelsInGroup - 0.5f);
                go.transform.SetLocalPosXY(
                    mazeBounds.center.x + xCoeff1,
                    bounds.max.y 
                    - m_LevelText.rectTransform.rect.height 
                    - markHeight * 0.5f 
                    - 1f);
                if (i == 0)
                    yPos += -m_LevelText.rectTransform.rect.height - markHeight - 2f;
            }
            m_Renderers.Add(m_LevelText);
            goLevelCheckMark.DestroySafe();
            
            var goCongrads = PrefabUtilsEx.InitPrefab(
                cont, "ui_game", "congratulations_panel");
            goCongrads.transform.SetPosXY(new Vector2(bounds.center.x, yPos));
            m_CompletedText = goCongrads.GetCompItem<TextMeshPro>("text_completed");
            m_CongratsText = goCongrads.GetCompItem<TextMeshPro>("text_congrats");
            m_CongratsAnim = goCongrads.GetCompItem<Animator>("animator");
            m_Renderers.AddRange(new object[]
            {
                m_CompletedText,
                m_CongratsText,
                goCongrads.GetCompItem<Line>("line")
            });
        }

        private void ShowControls(bool _Show, bool _Instantly)
        {
            if (_Instantly && !_Show || _Show)
            {
                m_ShopButton.SetGoActive(_Show);
                m_SettingsButton.SetGoActive(_Show);
                m_ShopButton.enabled = _Show;
                m_SettingsButton.enabled = _Show;
            }
            if (_Instantly)
                return;
            AppearTransitioner.DoAppearTransitionSimple(_Show, GameTicker, 
                new Dictionary<object[], System.Func<Color>>
                {
                    {m_Renderers.ToArray(), () => DrawingUtils.ColorLines},
                }, _Type: EAppearTransitionType.WithoutDelay);
        }

        private void CommandShop()
        {
            InputConfigurator.RaiseCommand(InputCommands.ShopMenu, null);
        }

        private void CommandSettings()
        {
            InputConfigurator.RaiseCommand(InputCommands.SettingsMenu, null);
        }

        private Transform GetGameUIContainer()
        {
            return ContainersGetter.GetContainer(ContainerNames.GameUI);
        }

        private void SetLevelCheckMarks(int _Level, bool _Passed)
        {
            if (!_Passed)
                m_LevelText.text = LocalizationManager.GetTranslation("level") + " " + (_Level + 1);
            int levelIndexInGroup = _Level % RazorMazeUtils.LevelsInGroup;
            for (int i = 0; i < levelIndexInGroup; i++)
                m_CheckMarks[i].SetTrigger(AnimKeyChekMarkSet);
            m_CheckMarks[levelIndexInGroup].SetTrigger(_Passed ? AnimKeyCheckMarkPass : AnimKeyCheckMarkIdle);
            for (int i = levelIndexInGroup; i < m_CheckMarks.Count; i++)
                m_CheckMarks[i].SetTrigger(AnimKeyCheckMarkIdle);
        }

        private void SetCongratsString()
        {
            m_CompletedText.text = LocalizationManager.GetTranslation("completed");
            float levelTime = Model.LevelStaging.LevelTime;
            int diesCount = Model.LevelStaging.DiesCount;
            int pathesCount = Model.PathItemsProceeder.PathProceeds.Count;
            float coeff = (float) pathesCount / (diesCount + 1);
            string key;
            if (levelTime < coeff * ViewSettings.FinishTimeExcellent)
                key = "awesome";
            else if (levelTime < coeff * ViewSettings.FinishTimeGood)
                key = "good_job";
            else
                key = "not_bad";
            m_CongratsText.text = LocalizationManager.GetTranslation(key).ToUpperInvariant();
        }

        #endregion
    }
}