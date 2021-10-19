using System.Collections.Generic;
using System.Linq;
using Constants;
using DI.Extensions;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.InputConfigurators;
using Games.RazorMaze.Views.Utils;
using Shapes;
using Ticker;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Games.RazorMaze.Views.UI
{
    public interface IViewUIGameControls : IInit, IOnLevelStageChanged
    {
        IViewUIPrompts Prompts { get; }
    }
    
    public class ViewUIGameControls : IViewUIGameControls
    {
        #region nonpublic members

        private static int AnimKeyCheckMarkPass => AnimKeys.Anim;
        private static int AnimKeyChekMarkSet => AnimKeys.Anim2;
        private static int AnimKeyCkeckMarkIdle => AnimKeys.Stop;

        private ButtonOnRaycast m_RotateClockwiseButton;
        private ButtonOnRaycast m_RotateCounterClockwiseButton;
        private ButtonOnRaycast m_ShopButton;
        private ButtonOnRaycast m_SettingsButton;
        private TextMeshPro m_LevelText;
        private readonly List<ShapeRenderer> m_TopShapeRenderers = new List<ShapeRenderer>();
        private readonly List<SpriteRenderer> m_TopSpriteRenderers = new List<SpriteRenderer>();
        private readonly List<Animator> m_CheckMarks = new List<Animator>();
        private bool m_ButtonsInitialized;

        #endregion
        
        #region inject
        
        public IViewUIPrompts Prompts { get; }
        private IModelGame Model { get; }
        private IContainersGetter ContainersGetter { get; }
        private IMazeCoordinateConverter CoordinateConverter { get; }
        private IViewInputConfigurator InputConfigurator { get; }
        private IViewAppearTransitioner AppearTransitioner { get; }
        private IGameTicker GameTicker { get; }
        private ILevelsLoader LevelsLoader { get; }
        private ILocalizationManager LocalizationManager { get; }

        public ViewUIGameControls(
            IViewUIPrompts _Prompts,
            IModelGame _Model,
            IContainersGetter _ContainersGetter,
            IMazeCoordinateConverter _CoordinateConverter,
            IViewInputConfigurator _InputConfigurator,
            IViewAppearTransitioner _AppearTransitioner,
            IGameTicker _GameTicker,
            ILevelsLoader _LevelsLoader,
            ILocalizationManager _LocalizationManager)
        {
            Prompts = _Prompts;
            Model = _Model;
            ContainersGetter = _ContainersGetter;
            CoordinateConverter = _CoordinateConverter;
            InputConfigurator = _InputConfigurator;
            AppearTransitioner = _AppearTransitioner;
            GameTicker = _GameTicker;
            LevelsLoader = _LevelsLoader;
            LocalizationManager = _LocalizationManager;
        }

        #endregion
        
        #region api

        public event UnityAction Initialized;
        public void Init()
        {
            Initialized?.Invoke();
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            Prompts.OnLevelStageChanged(_Args);
            if (_Args.Stage == ELevelStage.Loaded)
            {
                if (!m_ButtonsInitialized)
                {
                    InitGameUI();
                    m_ButtonsInitialized = true;
                }
                SetLevelCheckMarks(_Args.LevelIndex, false);
                ShowTopButtons(true, false);
            }
            else if (_Args.Stage == ELevelStage.Finished)
            {
                SetLevelCheckMarks(_Args.LevelIndex, true);
                ShowTopButtons(false, false);
            }
        }

        #endregion

        #region nonpublic methds

        private void InitGameUI()
        {
            InitTopButtons();
            InitLevelPanel();
        }
        
        private void InitTopButtons()
        {
            const float topOffset = 1f;
            const float horOffset = 1f;
            var cont = GetGameUIContainer();
            var goShopButton = PrefabUtilsEx.InitPrefab(
                cont, "ui_game", "shop_button");
            var goSettingsButton = PrefabUtilsEx.InitPrefab(
                cont, "ui_game", "settings_button");
            m_TopSpriteRenderers.AddRange( new []
            {
                goShopButton.GetCompItem<SpriteRenderer>("button"),
                goSettingsButton.GetCompItem<SpriteRenderer>("button")
            });
            float scale = CoordinateConverter.Scale * 0.5f;
            var bounds = GameUtils.GetVisibleBounds();
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

        private void InitLevelPanel()
        {
            var cont = GetGameUIContainer();
            var goLevelText = PrefabUtilsEx.InitPrefab(
                cont, "ui_game", "level_text");
            m_LevelText = goLevelText.GetCompItem<TextMeshPro>("text");
            float scale = CoordinateConverter.Scale * 0.5f;
            var bounds = GameUtils.GetVisibleBounds();
            var mazeBounds = CoordinateConverter.GetMazeBounds();
            m_LevelText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, mazeBounds.size.x);
            m_LevelText.transform.position = new Vector3(
                mazeBounds.center.x, 
                bounds.max.y - CoordinateConverter.GetScreenOffsets().z * 0.2f);
            m_LevelText.fontSize = 25f;
            var goLevelCheckMark = PrefabUtilsEx.GetPrefab(
                "ui_game", "level_check_mark");
            for (int i = 0; i < RazorMazeUtils.LevelsInGroup; i++)
            {
                var go = Object.Instantiate(goLevelCheckMark, cont);
                m_CheckMarks.Add(go.GetCompItem<Animator>("animator"));
                m_TopShapeRenderers.Add(go.GetCompItem<Rectangle>("body"));
                m_TopShapeRenderers.Add(go.GetCompItem<Line>("checkmark_1"));
                m_TopShapeRenderers.Add(go.GetCompItem<Line>("checkmark_2"));
                float xCoeff1 = 5f * ((i + 0.5f) / RazorMazeUtils.LevelsInGroup - 0.5f);
                go.transform.SetLocalPosXY(
                    mazeBounds.center.x + xCoeff1 * scale,
                    bounds.max.y - CoordinateConverter.GetScreenOffsets().z * 0.6f);
            }
            goLevelCheckMark.DestroySafe();
        }

        private void ShowTopButtons(bool _Show, bool _Instantly)
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
                    {m_TopSpriteRenderers.Cast<object>().ToArray(), () => DrawingUtils.ColorLines},
                    {m_TopShapeRenderers.Cast<object>().ToArray(), () => DrawingUtils.ColorLines},
                    {new object[] { m_LevelText }, () => DrawingUtils.ColorLines}
                }, _Type: EAppearTransitionType.WithoutDelay);
        }
        
        private void CommandRotateClockwise()
        {
            InputConfigurator.RaiseCommand(InputCommands.RotateClockwise, null);
        }
        
        private void CommandRotateCounterClockwise()
        {
            InputConfigurator.RaiseCommand(InputCommands.RotateCounterClockwise, null);
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
            m_CheckMarks[levelIndexInGroup].SetTrigger(_Passed ? AnimKeyCheckMarkPass : AnimKeyCkeckMarkIdle);
            for (int i = levelIndexInGroup; i < m_CheckMarks.Count; i++)
                m_CheckMarks[i].SetTrigger(AnimKeyCkeckMarkIdle);
        }

        #endregion
    }
}