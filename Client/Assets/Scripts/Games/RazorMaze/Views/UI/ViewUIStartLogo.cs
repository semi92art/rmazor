using System.Collections.Generic;
using System.Linq;
using Constants;
using DI.Extensions;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.InputConfigurators;
using Shapes;
using Ticker;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.UI
{
    public interface IViewUIStartLogo : IOnLevelStageChanged, IInitViewUIItem { }
    
    public class ViewUIStartLogo : IViewUIStartLogo
    {
        #region nonpublic members

        private static int AnimKeyStartLogoAppear    => AnimKeys.Anim;
        private static int AnimKeyStartLogoDisappear => AnimKeys.Stop;
        private static int AnimKeyStartLogoHide      => AnimKeys.Stop2;
        
        private Dictionary<string, Animator> m_StartLogoCharAnims;
        private float                        m_TopOffset;
        private bool                         m_OnStart = true;

        #endregion

        #region inject
        
        private ICameraProvider             CameraProvider      { get; }
        private IContainersGetter           ContainersGetter    { get; }
        private IViewInputCommandsProceeder CommandsProceeder   { get; }
        private IColorProvider              ColorProvider       { get; }
        private IViewGameTicker             GameTicker          { get; }


        public ViewUIStartLogo(
            ICameraProvider _CameraProvider,
            IContainersGetter _ContainersGetter,
            IViewInputCommandsProceeder _CommandsProceeder,
            IColorProvider _ColorProvider,
            IViewGameTicker _GameTicker)
        {
            CameraProvider = _CameraProvider;
            ContainersGetter = _ContainersGetter;
            CommandsProceeder = _CommandsProceeder;
            ColorProvider = _ColorProvider;
            GameTicker = _GameTicker;
        }

        #endregion

        #region api

        public void Init(Vector4 _Offsets)
        {
            CommandsProceeder.Command += OnCommand;
            m_TopOffset = _Offsets.w;
            InitStartLogo();
        }

        private void OnCommand(EInputCommand _Command, object[] _Args)
        {
            if (m_OnStart
                && (RazorMazeUtils.GetMoveCommands().Contains(_Command)
                || RazorMazeUtils.GetRotateCommands().Contains(_Command)))
            {
                HideStartLogo();
                m_OnStart = false;
            }
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            ShowStartLogo(_Args);
        }
        
        #endregion

        #region nonpublic methods

        private void InitStartLogo()
        {
            var screenBounds = GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera);
            const float additionalOffset = 8f;
            var go = PrefabUtilsEx.InitPrefab(
                ContainersGetter.GetContainer(ContainerNames.GameUI),
                "ui_game",
                "start_logo");
            float yPos = screenBounds.max.y - m_TopOffset - additionalOffset;
            go.transform.SetLocalPosXY(
                screenBounds.center.x,
                yPos);
            go.transform.localScale = Vector3.one * 3f;
            m_StartLogoCharAnims = new Dictionary<string, Animator>
            {
                {"R1", go.GetCompItem<Animator>("R1")},
                {"M", go.GetCompItem<Animator>("M")},
                {"A", go.GetCompItem<Animator>("A")},
                {"Z", go.GetCompItem<Animator>("Z")},
                {"O", go.GetCompItem<Animator>("O")},
                {"R2", go.GetCompItem<Animator>("R2")}
            };
            foreach (var anim in m_StartLogoCharAnims.Values)
                anim.SetTrigger(AnimKeyStartLogoHide);
            var trigerrer1 = go.GetCompItem<AnimationTriggerer>("trigerrer_1");
            var trigerrer2 = go.GetCompItem<AnimationTriggerer>("trigerrer_2");
            trigerrer1.Trigger1 += () => CommandsProceeder.LockCommands(CommandsProceeder.GetAllCommands());
            trigerrer2.Trigger1 += () => CommandsProceeder.UnlockAllCommands();
            var eye1 = go.GetCompItem<Rectangle>("eye_1");
            var eye2 = go.GetCompItem<Rectangle>("eye_2");
            eye1.Color = eye2.Color = ColorProvider.GetColor(ColorIds.Background);

            var shapeTypes = new [] {typeof(Line), typeof(Disc), typeof(Rectangle)};
            var uiCol = ColorProvider.GetColor(ColorIds.UI);
            shapeTypes.SelectMany(_Type => go
                .GetComponentsInChildren(_Type, true))
                .Cast<ShapeRenderer>()
                .Except(new [] { eye1, eye2})
                .ToList()
                .ForEach(_Shape => _Shape.Color = uiCol.SetA(_Shape.Color.a));
        }
        
        private void ShowStartLogo(LevelStageArgs _Args)
        {
            switch (_Args.Stage)
            {
                case ELevelStage.ReadyToStart when _Args.PreviousStage != ELevelStage.Paused && m_OnStart:
                    ShowStartLogo();
                    break;
            }
        }
        
        private void ShowStartLogo()
        {
            foreach (var anim in m_StartLogoCharAnims.Values)
                anim.speed = 1.5f;
            ShowStartLogoItem("R1", 0f);
            ShowStartLogoItem("M", 0.1f);
            ShowStartLogoItem("A", 0.2f);
            ShowStartLogoItem("Z", 0.3f);
            ShowStartLogoItem("O", 0.5f);
            ShowStartLogoItem("R2", 0.4f);
        }

        private void ShowStartLogoItem(string _Key, float _Delay)
        {
            float time = GameTicker.Time;
            Coroutines.Run(Coroutines.WaitWhile(
                () => time + _Delay > GameTicker.Time,
                () => m_StartLogoCharAnims[_Key].SetTrigger(AnimKeyStartLogoAppear)));
        }

        private void HideStartLogo()
        {
            foreach (var anim in m_StartLogoCharAnims.Values)
                anim.speed = 3f;
            foreach (var anim in m_StartLogoCharAnims.Values)
                anim.SetTrigger(AnimKeyStartLogoDisappear);
        }

        #endregion
    }
}