using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Constants;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.Utils;
using Shapes;
using TMPro;
using UnityEngine;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Views.UI
{
    public interface IViewUICongratsMessage : 
        IOnLevelStageChanged,
        IInitViewUIItem, 
        IViewUIGetRenderers { }
    
    public class ViewUICongratsMessage : IViewUICongratsMessage
    {
        #region nonpublic members

        private static int AnimKeyCongratsAnim => AnimKeys.Anim;
        private static int AnimKeyCongratsIdle => AnimKeys.Stop;
        
        private readonly List<Component> m_Renderers = new List<Component>();

        private GameObject  m_CongratsGo;
        private TextMeshPro m_BottomText;
        private TextMeshPro m_TopText;
        private Line        m_CongratsLine;
        private Animator    m_CongratsAnim;
        private float       m_TopOffset;

        #endregion

        #region inject

        private IModelGame        Model            { get; }
        private ICameraProvider   CameraProvider   { get; }
        private IManagersGetter   Managers         { get; }
        private IColorProvider    ColorProvider    { get; }

        private ViewUICongratsMessage(
            IModelGame        _Model,
            ICameraProvider   _CameraProvider,
            IManagersGetter   _Managers,
            IColorProvider    _ColorProvider)
        {
            Model            = _Model;
            CameraProvider   = _CameraProvider;
            Managers         = _Managers;
            ColorProvider    = _ColorProvider;
        }

        #endregion

        #region api
        
        public void Init(Vector4 _Offsets)
        {
            m_TopOffset = _Offsets.w;
            InitCongratsMessage();
            CameraProvider.ActiveCameraChanged += OnActiveCameraChanged;
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
                    ConsiderCongratsPanelWhileAppearing(false);
                    HideCongratsPanel();
                    break;
                case ELevelStage.Finished when _Args.PreviousStage != ELevelStage.Paused: 
                    ConsiderCongratsPanelWhileAppearing(true);
                    SetTexts();
                    ShowCongratsPanel(_Args);
                    break;
                case ELevelStage.Unloaded:
                    m_CongratsAnim.SetTrigger(AnimKeyCongratsIdle);
                    break;
            }
        }

        #endregion

        #region nonpublic methods
        
        private void OnActiveCameraChanged(Camera _Camera)
        {
            var parent = CameraProvider.Camera.transform;
            m_CongratsGo.SetParent(parent);
            var screenBounds = GraphicUtils.GetVisibleBounds(CameraProvider.Camera);
            m_CongratsGo.transform
                .SetLocalPosX(screenBounds.center.x)
                .SetLocalPosY(screenBounds.max.y - m_TopOffset - 5f);
        }

        private void InitCongratsMessage()
        {
            var parent = CameraProvider.Camera.transform;
            var goCongrads = Managers.PrefabSetManager.InitPrefab(
                parent, CommonPrefabSetNames.UiGame, "congratulations_panel");
            m_CongratsGo                 = goCongrads;
            m_TopText                    = m_CongratsGo.GetCompItem<TextMeshPro>("text_completed");
            m_BottomText                 = m_CongratsGo.GetCompItem<TextMeshPro>("text_congrats");
            m_CongratsLine               = m_CongratsGo.GetCompItem<Line>("line");
            m_CongratsAnim               = m_CongratsGo.GetCompItem<Animator>("animator");
            m_TopText.sortingOrder       = SortingOrders.GameUI;
            m_BottomText.sortingOrder    = SortingOrders.GameUI;
            m_CongratsLine.SortingOrder  = SortingOrders.GameUI;
        }
        
        private void SetTexts()
        {
            var locMan = Managers.LocalizationManager;
            m_TopText.font       = m_BottomText.font = Managers.LocalizationManager.GetFont(ETextType.GameUI);
            string keyTopText    = GetTopTextLocalizationKey(); 
            m_TopText.text       = locMan.GetTranslation(keyTopText).ToUpperInvariant();
            string keyBottomText = GetBottomTextLocalizationKey();
            m_BottomText.text    = locMan.GetTranslation(keyBottomText).ToUpperInvariant();
        }

        private string GetTopTextLocalizationKey()
        {
            const string defaultKey = "completed"; 
            var levelStagingArgs = Model.LevelStaging.Arguments;
            string gameMode = (string) levelStagingArgs.GetSafe(KeyGameMode, out _);
            return gameMode != ParameterGameModeDailyChallenge ? defaultKey : "challenge";
        }

        private string GetBottomTextLocalizationKey()
        {
            var levelStagingArgs = Model.LevelStaging.Arguments;
            float levelTime = Model.LevelStaging.LevelTime;
            var mazeAdditionalInfo = Model.Data.Info.AdditionalInfo;
            string key;
            if (levelTime < mazeAdditionalInfo.Time3Stars)
                key = "awesome";
            else if (levelTime < mazeAdditionalInfo.Time2Stars)
                key = "good_job";
            else if (levelTime < mazeAdditionalInfo.Time1Star)
                key = "not_bad";
            else 
                key = "could_be_better";
            string gameMode = (string) levelStagingArgs.GetSafe(KeyGameMode, out _);
            if (gameMode != ParameterGameModeDailyChallenge) 
                return key;
            bool isSuccess = (bool)levelStagingArgs.GetSafe(KeyIsDailyChallengeSuccess, out _);
            return isSuccess ? "completed" : "failed_1";
        }

        private void ConsiderCongratsPanelWhileAppearing(bool _Consider)
        {
            var congratRenderers = new Behaviour[]
            {
                m_TopText,
                m_BottomText,
                m_CongratsLine
            };

            if (_Consider)
            {
                if (!m_Renderers.Contains(congratRenderers.First()))
                    m_Renderers.AddRange(congratRenderers);
            }
            else
            {
                foreach (var rend in congratRenderers)
                    m_Renderers.Remove(rend);
            }
        }
        
        private void ShowCongratsPanel(LevelStageArgs _Args)
        {
            if (!CanShowCongratsPanel(_Args))
                return;
            var col = ColorProvider.GetColor(ColorIds.UI);
            m_TopText.color = m_BottomText.color = m_CongratsLine.Color = col;
            m_CongratsAnim.SetTrigger(AnimKeyCongratsAnim);
        }

        private void HideCongratsPanel()
        {
            m_TopText.color = m_BottomText.color = m_CongratsLine.Color = Color.white.SetA(0f);
            m_CongratsAnim.SetTrigger(AnimKeyCongratsIdle);
        }

        private static bool CanShowCongratsPanel(LevelStageArgs _Args)
        {
            return (string) _Args.Arguments[KeyGameMode] != ParameterGameModePuzzles 
                   || !_Args.Arguments.ContainsKey(KeyShowPuzzleLevelHint);
        }

        #endregion
    }
}