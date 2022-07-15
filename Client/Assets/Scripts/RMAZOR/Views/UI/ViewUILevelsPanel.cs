using System.Collections.Generic;
using System.Linq;
using Common.CameraProviders;
using Common.Constants;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
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
        private readonly Dictionary<Transform, float> m_LevelPanelItemsFinishPositions = 
            new Dictionary<Transform, float>();

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
        private IContainersGetter           ContainersGetter  { get; }
        private ICameraProvider             CameraProvider    { get; }
        private IViewGameTicker             GameTicker        { get; }
        private IManagersGetter             Managers          { get; }

        private ViewUILevelsPanel(
            IModelGame                  _Model,
            IViewInputCommandsProceeder _CommandsProceeder,
            IContainersGetter           _ContainersGetter,
            ICameraProvider             _CameraProvider,
            IViewGameTicker             _GameTicker,
            IManagersGetter             _Managers)
        {
            Model             = _Model;
            CommandsProceeder = _CommandsProceeder;
            ContainersGetter  = _ContainersGetter;
            CameraProvider    = _CameraProvider;
            GameTicker        = _GameTicker;
            Managers          = _Managers;
        }

        #endregion

        #region api
        
        public void Init(Vector4 _Offsets)
        {
            GameTicker.Register(this);
            m_TopOffset = _Offsets.w;
            InitLevelPanel();
            InitCheckMarks();
            CommandsProceeder.Command += OnCommand;
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.LevelStage)
            {
                case ELevelStage.Loaded:
                    UpdateCheckMarks();
                    SetLevelCheckMarks(_Args, false);
                    EnableCurrentLevelBoxHighlighting(_Args);
                    break;
                case ELevelStage.Finished when _Args.PreviousStage != ELevelStage.Paused:
                    SetLevelCheckMarks(_Args, true);    
                    break;
            }
        }

        public List<Component> GetRenderers()
        {
            return m_Renderers;
        }

        public void ShowControls(bool _Show, bool _Instantly)
        {
            if (!_Show)
                return;
            Managers.LocalizationManager.AddTextObject(new LocalizableTextObjectInfo(
                m_LevelText,
                ETextType.GameUI,
                "level", 
                _Text => _Text + " " + (Model.LevelStaging.LevelIndex + 1)));
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
        
        private void OnCommand(EInputCommand _Command, object[] _Args)
        {
            if (m_FirstMoveOrRotateCommandInvoked ||
                !RmazorUtils.MoveAndRotateCommands.ContainsAlt(_Command))
                return;
            AnimateLevelsPanelAfterFirstMove();
            m_FirstMoveOrRotateCommandInvoked = true;
        }
        
        private void InitLevelPanel()
        {
            m_LevelPanelItemsFinishPositions.Clear();
            var screenBounds = GraphicUtils.GetVisibleBounds(CameraProvider.Camera);
            var cont = ContainersGetter.GetContainer(ContainerNames.GameUI);
            var goLevelText = Managers.PrefabSetManager.InitPrefab(
                cont, "ui_game", "level_text");
            m_LevelText = goLevelText.GetCompItem<TextMeshPro>("text");
            m_Renderers.Add(m_LevelText);
            m_LevelText.rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal, screenBounds.size.x);
            float yPos = screenBounds.max.y - m_TopOffset + 1f;
            m_LevelText.transform.SetLocalPosXY(screenBounds.center.x, yPos + StartAnimOffset);
            m_LevelPanelItemsFinishPositions.Add(m_LevelText.transform, yPos);
        }
        
        private void InitCheckMarks()
        {
            var cont = ContainersGetter.GetContainer(ContainerNames.GameUI);
            var screenBounds = GraphicUtils.GetVisibleBounds(CameraProvider.Camera);
            var goLevelCheckMark = Managers.PrefabSetManager.GetPrefab(
                "ui_game", "level_check_mark");
            float yPos = screenBounds.max.y - m_TopOffset - 3f;
            if (m_CheckMarks.Any()) 
                return;
            for (int i = 0; i < 5; i++)
            {
                var go = Object.Instantiate(goLevelCheckMark, cont);
                m_CheckMarks.Add(go.GetCompItem<Animator>("animator"));
                m_Renderers.AddRange(new Component[]
                {
                    go.GetCompItem<Rectangle>("body"),
                    go.GetCompItem<Line>("checkmark_1"),
                    go.GetCompItem<Line>("checkmark_2")
                });
                m_LevelBoxes.Add(go.GetCompItem<Rectangle>("body"));
                m_LevelPanelItemsFinishPositions.Add(go.transform, yPos);
                var triggerer = go.GetCompItem<AnimationTriggerer>("triggerer");
                triggerer.Trigger1 = DisableCurrentLevelBoxHighlighting;
            }
            goLevelCheckMark.DestroySafe();
        }
        
        private void UpdateCheckMarks()
        {          
            foreach (var checkmark in m_CheckMarks)
                checkmark.gameObject.SetActive(false);
            var screenBounds = GraphicUtils.GetVisibleBounds(CameraProvider.Camera);
            int groupIndex = RmazorUtils.GetGroupIndex(Model.LevelStaging.LevelIndex);
            int levelsInGroup = RmazorUtils.GetLevelsInGroup(groupIndex);
            float yPos = screenBounds.max.y - m_TopOffset - 3f;
            for (int i = 0; i < levelsInGroup; i++)
            {
                var checkmark = m_CheckMarks[i];
                checkmark.gameObject.SetActive(true);
                float i1 = i + 0.5f;
                const float c = 3f;
                float xIndent = (i1 - levelsInGroup * 0.5f) * c;
                checkmark.transform.SetLocalPosXY(
                    screenBounds.center.x + xIndent,
                    yPos + (m_FirstMoveOrRotateCommandInvoked ? 0f : StartAnimOffset));
            }
        }
        
        private void AnimateLevelsPanelAfterFirstMove()
        {
            const float animTime = 0.3f;
            foreach (var transform in m_LevelPanelItemsFinishPositions.Keys)
            {
                Cor.Run(Cor.Lerp(
                    GameTicker,
                    animTime,
                    transform.localPosition.y,
                    m_LevelPanelItemsFinishPositions[transform],
                    _YPos => transform.SetLocalPosXY(transform.localPosition.x, _YPos)));
            }
        }
        
        private void SetLevelCheckMarks(LevelStageArgs _Args, bool _Passed)
        {
            int levelIndexInGroup = RmazorUtils.GetIndexInGroup(_Args.LevelIndex);
            for (int i = 0; i < levelIndexInGroup; i++)
                m_CheckMarks[i].SetTrigger(AnimKeyChekMarkSet);
            m_CheckMarks[levelIndexInGroup].SetTrigger(_Passed ? AnimKeyCheckMarkPass : AnimKeyCheckMarkIdle);
            for (int i = levelIndexInGroup; i < m_CheckMarks.Count; i++)
                m_CheckMarks[i].SetTrigger(AnimKeyCheckMarkIdle);
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