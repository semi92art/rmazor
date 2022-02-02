using System.Collections.Generic;
using System.Linq;
using Common.CameraProviders;
using Common.Constants;
using Common.Extensions;
using Common.Ticker;
using Common.Utils;
using Managers;
using RMAZOR.Models;
using RMAZOR.Views.ContainerGetters;
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
    
    public class ViewUILevelsPanel : IViewUILevelsPanel
    {
        #region constants
        
        private const float StartAnimOffset = 15f;

        #endregion

        #region nonpublic members
        
        private static int AnimKeyCheckMarkPass => AnimKeys.Anim;
        private static int AnimKeyChekMarkSet   => AnimKeys.Anim2;
        private static int AnimKeyCheckMarkIdle => AnimKeys.Stop;
        
        private readonly List<Component> m_Renderers  = new List<Component>();
        private readonly List<Animator>  m_CheckMarks = new List<Animator>();
        private readonly Dictionary<Transform, float> m_LevelPanelItemsFinishPositions = 
            new Dictionary<Transform, float>();

        private TextMeshPro m_LevelText;
        private float       m_TopOffset;
        private bool        m_FirstMoveOrRotateCommandInvoked;

        #endregion

        #region inject

        private IModelGame                  Model             { get; }
        private IViewInputCommandsProceeder CommandsProceeder { get; }
        private IContainersGetter           ContainersGetter  { get; }
        private ICameraProvider             CameraProvider    { get; }
        private IViewGameTicker             GameTicker        { get; }
        private IManagersGetter             Managers          { get; }

        public ViewUILevelsPanel(
            IModelGame                  _Model,
            IViewInputCommandsProceeder _CommandsProceeder,
            IContainersGetter           _ContainersGetter,
            ICameraProvider             _CameraProvider,
            IViewGameTicker             _GameTicker,
            IManagersGetter             _Managers)
        {
            Model = _Model;
            CommandsProceeder = _CommandsProceeder;
            ContainersGetter = _ContainersGetter;
            CameraProvider = _CameraProvider;
            GameTicker = _GameTicker;
            Managers = _Managers;
        }

        #endregion

        #region api
        
        public void Init(Vector4 _Offsets)
        {
            m_TopOffset = _Offsets.w;
            InitLevelPanel();
            InitCheckMarks();
            CommandsProceeder.Command += OnCommand;
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                    UpdateCheckMarks();
                    SetLevelCheckMarks(_Args.LevelIndex, false);
                    break;
                case ELevelStage.Finished when _Args.PreviousStage != ELevelStage.Paused:
                    SetLevelCheckMarks(_Args.LevelIndex, true);    
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
            Managers.LocalizationManager.AddTextObject(
                m_LevelText,
                "level", 
                _Text => _Text + " " + (Model.LevelStaging.LevelIndex + 1));
        }

        #endregion

        #region nonpublic methods
        
        private void OnCommand(EInputCommand _Command, object[] _Args)
        {
            if (!m_FirstMoveOrRotateCommandInvoked 
                && RazorMazeUtils.MoveAndRotateCommands.ContainsAlt(_Command))
            {
                AnimateLevelsPanelAfterFirstMove();
                m_FirstMoveOrRotateCommandInvoked = true;
            }
        }
        
        private void InitLevelPanel()
        {
            m_LevelPanelItemsFinishPositions.Clear();
            var screenBounds = GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera);
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
            var screenBounds = GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera);
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
                m_LevelPanelItemsFinishPositions.Add(go.transform, yPos);
            }
            goLevelCheckMark.DestroySafe();
        }
        
        private void UpdateCheckMarks()
        {          
            foreach (var checkmark in m_CheckMarks)
                checkmark.gameObject.SetActive(false);
            var screenBounds = GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera);
            int groupIndex = RazorMazeUtils.GetGroupIndex(Model.LevelStaging.LevelIndex);
            int levelsInGroup = RazorMazeUtils.GetLevelsInGroup(groupIndex);
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
                    transform.localPosition.y,
                    m_LevelPanelItemsFinishPositions[transform],
                    animTime,
                    _YPos => transform.SetLocalPosXY(transform.localPosition.x, _YPos),
                    GameTicker));
            }
        }
        
        private void SetLevelCheckMarks(int _LevelIndex, bool _Passed)
        {
            int levelIndexInGroup = RazorMazeUtils.GetIndexInGroup(_LevelIndex);
            for (int i = 0; i < levelIndexInGroup; i++)
                m_CheckMarks[i].SetTrigger(AnimKeyChekMarkSet);
            m_CheckMarks[levelIndexInGroup].SetTrigger(_Passed ? AnimKeyCheckMarkPass : AnimKeyCheckMarkIdle);
            for (int i = levelIndexInGroup; i < m_CheckMarks.Count; i++)
                m_CheckMarks[i].SetTrigger(AnimKeyCheckMarkIdle);
        }

        #endregion
        
    }
}