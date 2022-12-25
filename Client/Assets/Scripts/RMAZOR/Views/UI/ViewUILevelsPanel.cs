using System.Collections.Generic;
using System.Linq;
using Common.Constants;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Utils;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.UI.Game_Logo;
using RMAZOR.Views.Utils;
using Shapes;
using TMPro;
using UnityEngine;

namespace RMAZOR.Views.UI
{
    public interface IViewUILevelsPanel : 
        IOnLevelStageChanged, 
        IInitViewUIItem,
        IViewUIGetRenderers
    {
        void ShowControls(bool _Show, bool _Instantly);
    }
    
    public class ViewUILevelsPanel : IViewUILevelsPanel, IUpdateTick
    {
        #region constants
        
        private const float StartAnimOffset = 15f;

        #endregion

        #region nonpublic members
        
        private static int AnimKeyCheckMarkPass => AnimKeys.Anim;
        private static int AnimKeyChekMarkSet   => AnimKeys.Anim2;
        private static int AnimKeyCheckMarkIdle => AnimKeys.Stop;
        
        private readonly List<Component> m_Renderers  = new List<Component>();
        private readonly List<Rectangle> m_LevelBoxes = new List<Rectangle>();
        private readonly List<Animator>  m_CheckMarks = new List<Animator>();
        
        private readonly Dictionary<Transform, float> m_LevelPanelItemsFinishPositionsY = 
            new Dictionary<Transform, float>();

        private TextMeshPro m_StageText;
        private TextMeshPro m_LevelText;
        private float       m_TopOffset;
        private bool        m_FirstMoveOrRotateCommandInvoked;
        private bool        m_HighlightCurrentLevelBox;
        private int         m_CurrentLevelGroupIndex;
        private float       m_CurrentLevelBoxDashOffset;

        #endregion

        #region inject

        private IModelGame                  Model             { get; }
        private IViewInputCommandsProceeder CommandsProceeder { get; }
        private ICameraProvider             CameraProvider    { get; }
        private IViewGameTicker             GameTicker        { get; }
        private IManagersGetter             Managers          { get; }
        private IViewUIGameLogo             GameLogo          { get; }
        private IFontProvider               FontProvider      { get; }

        private ViewUILevelsPanel(
            IModelGame                  _Model,
            IViewInputCommandsProceeder _CommandsProceeder,
            ICameraProvider             _CameraProvider,
            IViewGameTicker             _GameTicker,
            IManagersGetter             _Managers,
            IViewUIGameLogo             _GameLogo,
            IFontProvider               _FontProvider)
        {
            Model             = _Model;
            CommandsProceeder = _CommandsProceeder;
            CameraProvider    = _CameraProvider;
            GameTicker        = _GameTicker;
            Managers          = _Managers;
            GameLogo          = _GameLogo;
            FontProvider      = _FontProvider;
        }

        #endregion

        #region api
        
        public void Init(Vector4 _Offsets)
        {
            GameTicker.Register(this);
            m_TopOffset = _Offsets.w;
            InitLevelPanel();
            InitCheckMarks();
            CameraProvider.ActiveCameraChanged += OnActiveCameraChanged;
            Managers.LocalizationManager.LanguageChanged += OnLanguageChanged;
            CommandsProceeder.Command += OnCommand;
        }

        public IEnumerable<Component> GetRenderers()
        {
            return m_Renderers;
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.LevelStage)
            {
                case ELevelStage.Loaded:
                    SetLevelCheckMarks(_Args, false);
                    EnableCurrentLevelBoxHighlighting(_Args);
                    break;
                case ELevelStage.Finished when _Args.PreviousStage != ELevelStage.Paused:
                    SetLevelCheckMarks(_Args, true);    
                    break;
            }
        }
        
        public void ShowControls(bool _Show, bool _Instantly)
        {
            if (!_Show)
                return;
            SetStageAndLevelTexts();
        }
        
        public void UpdateTick()
        {
            if (!m_HighlightCurrentLevelBox)
                return;
            m_CurrentLevelBoxDashOffset += GameTicker.DeltaTime * 0.3f;
            m_CurrentLevelBoxDashOffset = MathUtils.ClampInverse(m_CurrentLevelBoxDashOffset, 0f, 10f);
            m_LevelBoxes[m_CurrentLevelGroupIndex].SetDashOffset(m_CurrentLevelBoxDashOffset);
        }

        #endregion

        #region nonpublic methods
        
        private void OnLanguageChanged(ELanguage _Language)
        {
            SetStageAndLevelTexts();
        }
        
        private void OnActiveCameraChanged(Camera _Camera)
        {
            if (GameLogo.WasShown)
            {
                var parent = _Camera.transform;
                foreach (var transform in m_LevelPanelItemsFinishPositionsY.Keys)
                {
                    float yPos = m_LevelPanelItemsFinishPositionsY[transform];
                    transform.SetParentEx(parent).SetLocalPosY(yPos).SetLocalPosZ(10f);
                }
                var screenBounds = GraphicUtils.GetVisibleBounds(CameraProvider.Camera);
                m_StageText.transform.SetLocalPosX(screenBounds.center.x);
                m_LevelText.transform.SetLocalPosX(screenBounds.center.x);
            }
            UpdateCheckMarks(_Camera);
        }
        
        private void OnCommand(EInputCommand _Command, Dictionary<string, object> _Args)
        {
            if (m_FirstMoveOrRotateCommandInvoked ||
                !RmazorUtils.MoveAndRotateCommands.ContainsAlt(_Command))
                return;
            AnimateLevelsPanelAfterFirstMove();
            m_FirstMoveOrRotateCommandInvoked = true;
        }

        private void SetStageAndLevelTexts()
        {
            var locMan = Managers.LocalizationManager;
            var font = FontProvider.GetFont(ETextType.GameUI, locMan.GetCurrentLanguage());
            m_StageText.font = m_LevelText.font = font;
            string nextLevelType = (string)Model.LevelStaging.Arguments.GetSafe(
                CommonInputCommandArg.KeyNextLevelType, out _);
            bool isNextLevelBonus = nextLevelType == CommonInputCommandArg.ParameterLevelTypeBonus;
            if (isNextLevelBonus)
            {
                m_StageText.text = m_LevelText.text = string.Empty;
                return;
            }
            int levelsGroupIdx = RmazorUtils.GetLevelsGroupIndex(Model.LevelStaging.LevelIndex);
            m_StageText.text = locMan.GetTranslation("stage") + " " + levelsGroupIdx;
            m_LevelText.text = locMan.GetTranslation("level") + " " + (Model.LevelStaging.LevelIndex + 1);
        }
        
        private void InitLevelPanel()
        {
            var screenBounds = GraphicUtils.GetVisibleBounds(CameraProvider.Camera);
            var parent = CameraProvider.Camera.transform;
            var goLevelText = Managers.PrefabSetManager.InitPrefab(
                parent, CommonPrefabSetNames.UiGame, "level_text");
            goLevelText.transform.SetLocalPosXY(Vector2.zero).SetLocalScaleXY(Vector2.one);
            m_StageText = goLevelText.GetCompItem<TextMeshPro>("stage_text");
            m_LevelText = goLevelText.GetCompItem<TextMeshPro>("level_text");
            m_StageText.sortingOrder = SortingOrders.GameUI;
            m_LevelText.sortingOrder = SortingOrders.GameUI;
            m_Renderers.AddRange(new [] {m_StageText, m_LevelText});
            m_StageText.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal, screenBounds.size.x * 0.5f);
            m_LevelText.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal, screenBounds.size.x * 0.5f);
            float yPos = screenBounds.max.y - 5f;
            m_StageText.transform
                .SetLocalPosX(screenBounds.center.x)
                .SetLocalPosY(yPos + StartAnimOffset)
                .SetLocalPosZ(10f);
            m_LevelText.transform
                .SetLocalPosX(screenBounds.center.x)
                .SetLocalPosY(yPos - 2.5f + StartAnimOffset)
                .SetLocalPosZ(10f);
            m_LevelPanelItemsFinishPositionsY.Add(m_StageText.transform, yPos);
            m_LevelPanelItemsFinishPositionsY.Add(m_LevelText.transform, yPos - 2.5f);
        }
        
        private void InitCheckMarks()
        {
            var screenBounds = GraphicUtils.GetVisibleBounds(CameraProvider.Camera);
            var goLevelCheckMark = Managers.PrefabSetManager.GetPrefab(
                CommonPrefabSetNames.UiGame, "level_check_mark");
            float yPos = screenBounds.max.y - 8.5f;
            var parent = CameraProvider.Camera.transform;
            for (int i = 0; i < 10; i++)
            {
                var go = Object.Instantiate(goLevelCheckMark, parent);
                m_CheckMarks.Add(go.GetCompItem<Animator>("animator"));
                m_Renderers.AddRange(new Component[]
                {
                    go.GetCompItem<Rectangle>("body")  .SetSortingOrder(SortingOrders.GameUI),
                    go.GetCompItem<Line>("checkmark_1").SetSortingOrder(SortingOrders.GameUI),
                    go.GetCompItem<Line>("checkmark_2").SetSortingOrder(SortingOrders.GameUI)
                });
                m_LevelBoxes.Add(go.GetCompItem<Rectangle>("body"));
                go.transform
                    .SetLocalPosX(screenBounds.center.x)
                    .SetLocalPosY(yPos + StartAnimOffset)
                    .SetLocalPosZ(10f);
                m_LevelPanelItemsFinishPositionsY.Add(go.transform, yPos);
                var triggerer = go.GetCompItem<AnimationTriggerer>("triggerer");
                triggerer.Trigger1 = DisableCurrentLevelBoxHighlighting;
            }
            goLevelCheckMark.DestroySafe();
        }
        
        private void UpdateCheckMarks(Camera _Camera)
        {          
            foreach (var checkmark in m_CheckMarks)
                checkmark.gameObject.SetActive(false);
            var screenBounds = GraphicUtils.GetVisibleBounds(_Camera);
            int groupIndex = RmazorUtils.GetLevelsGroupIndex(Model.LevelStaging.LevelIndex);
            string nextLevelType = (string)Model.LevelStaging.Arguments.GetSafe(
                CommonInputCommandArg.KeyNextLevelType, out _);
            bool isNextLevelBonus = nextLevelType == CommonInputCommandArg.ParameterLevelTypeBonus;
            int checkMarksCount = isNextLevelBonus ?
                0 :  RmazorUtils.GetLevelsInGroup(groupIndex);
            float yPos = screenBounds.max.y - 8.5f;
            for (int i = 0; i < checkMarksCount; i++)
            {
                var checkmark = m_CheckMarks[i];
                checkmark.gameObject.SetActive(true);
                float i1 = i + 0.5f;
                const float c = 3f;
                float xIndent = (i1 - checkMarksCount * 0.5f) * c;
                float yIndent = m_FirstMoveOrRotateCommandInvoked ? 0f : StartAnimOffset;
                checkmark.transform
                    .SetLocalPosX(screenBounds.center.x + xIndent)
                    .SetLocalPosY(yPos + yIndent);
            }
        }
        
        private void SetLevelCheckMarks(LevelStageArgs _Args, bool _LevelPassedJustNow)
        {
            string nextLevelType = (string)_Args.Args.GetSafe(
                CommonInputCommandArg.KeyNextLevelType, out _);
            bool isNextLevelBonus = nextLevelType == CommonInputCommandArg.ParameterLevelTypeBonus;
            if (isNextLevelBonus)
                return;
            int lastPassedLevelInGroupIndex = RmazorUtils.GetIndexInGroup(_Args.LevelIndex);
            for (int i = 0; i < lastPassedLevelInGroupIndex; i++)
                m_CheckMarks[i].SetTrigger(AnimKeyChekMarkSet);
            m_CheckMarks[lastPassedLevelInGroupIndex].SetTrigger(
                _LevelPassedJustNow ? AnimKeyCheckMarkPass : AnimKeyCheckMarkIdle);
            for (int i = lastPassedLevelInGroupIndex; i < m_CheckMarks.Count; i++)
                m_CheckMarks[i].SetTrigger(AnimKeyCheckMarkIdle);
        }
        
        private void AnimateLevelsPanelAfterFirstMove()
        {
            const float animTime = 0.3f;
            foreach (var transform in m_LevelPanelItemsFinishPositionsY.Keys)
            {
                Cor.Run(Cor.Lerp(
                    GameTicker,
                    animTime,
                    transform.localPosition.y,
                    m_LevelPanelItemsFinishPositionsY[transform],
                    _YPos => transform.SetLocalPosXY(transform.localPosition.x, _YPos)));
            }
        }

        private void EnableCurrentLevelBoxHighlighting(LevelStageArgs _Args)
        {
            foreach (var box in m_LevelBoxes)
                box.SetDashed(false).SetDashOffset(0f);
            m_CurrentLevelGroupIndex = RmazorUtils.GetIndexInGroup(_Args.LevelIndex);
            m_HighlightCurrentLevelBox = true;
            m_LevelBoxes[m_CurrentLevelGroupIndex].SetDashed(true)
                .SetDashSize(8f)
                .SetDashType(DashType.Rounded)
                .SetDashSpace(DashSpace.FixedCount)
                .SetMatchDashSpacingToDashSize(false)
                .SetDashSpacing(0.3f);
        }

        private void DisableCurrentLevelBoxHighlighting()
        {
            Cor.Run(Cor.Lerp(
                GameTicker,
                1f / m_CheckMarks.First().speed,
                _OnProgress: _P =>
                {
                    m_LevelBoxes[m_CurrentLevelGroupIndex].SetDashSpacing(0.3f * (1 - _P));
                },
                _OnFinish: () =>
                {
                    m_HighlightCurrentLevelBox = false;
                    m_LevelBoxes[m_CurrentLevelGroupIndex]
                        .SetDashed(false)
                        .SetDashOffset(m_CurrentLevelBoxDashOffset = 0f);
                }));
        }

        #endregion
    }
}