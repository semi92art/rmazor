using System.Collections.Generic;
using System.Linq;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.UI.StartLogo
{
    public abstract class ViewUIStartLogoBase : IViewUIStartLogo
    {
        #region nonpublic members

        private static int AnimKeyStartLogoAppear    => AnimKeys.Anim;
        private static int AnimKeyStartLogoDisappear => AnimKeys.Stop;

        protected virtual  float                     AnimationSpeed => 1f;
        protected abstract string                    PrefabName     { get; }
        protected abstract Dictionary<string, float> KeysAndDelays  { get; }
        
        protected GameObject                   StartLogoObj;
        protected bool                         LogoShowingAnimationPassed;
        private   Dictionary<string, Animator> m_StartLogoCharAnims;
        private   float                        m_TopOffset;
        private   bool                         m_OnStart = true;

        #endregion
        
        #region inject

        private   IViewInputCommandsProceeder CommandsProceeder { get; }
        protected ICameraProvider             CameraProvider    { get; }
        protected IContainersGetter           ContainersGetter  { get; }
        protected IColorProvider              ColorProvider     { get; }
        protected IViewGameTicker             GameTicker        { get; }
        protected IPrefabSetManager           PrefabSetManager  { get; }
        
        protected ViewUIStartLogoBase(
            ICameraProvider             _CameraProvider,
            IContainersGetter           _ContainersGetter,
            IViewInputCommandsProceeder _CommandsProceeder,
            IColorProvider              _ColorProvider,
            IViewGameTicker             _GameTicker,
            IPrefabSetManager           _PrefabSetManager)
        {
            CameraProvider    = _CameraProvider;
            ContainersGetter  = _ContainersGetter;
            CommandsProceeder = _CommandsProceeder;
            ColorProvider     = _ColorProvider;
            GameTicker        = _GameTicker;
            PrefabSetManager  = _PrefabSetManager;
        }

        #endregion

        #region api

        public void Init(Vector4 _Offsets)
        {
            CommandsProceeder.Command += OnCommand;
            ColorProvider.ColorChanged += OnColorChanged;
            m_TopOffset = _Offsets.w;
            InitStartLogo();
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            ShowStartLogo(_Args);
        }
        
        #endregion

        #region nonpublic members

        private void OnCommand(EInputCommand _Command, object[] _Args)
        {
            if (!m_OnStart || !RazorMazeUtils.MoveAndRotateCommands.ContainsAlt(_Command)) 
                return;
            HideStartLogo();
            m_OnStart = false;
        }
        
        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId != ColorIdsCommon.UI)
                return;
            SetColors(_Color);
        }
        
        private void ShowStartLogo(LevelStageArgs _Args)
        {
            if (_Args.Stage == ELevelStage.ReadyToStart
                && _Args.PreviousStage == ELevelStage.Loaded
                && m_OnStart)
            {
                ShowStartLogo();
            }
        }

        protected virtual void SetColors(Color _Color)
        {
            var shapeTypes = new [] {typeof(Line), typeof(Disc), typeof(Rectangle)};
            shapeTypes.SelectMany(_Type => StartLogoObj
                    .GetComponentsInChildren(_Type, true))
                .Cast<ShapeRenderer>()
                .Except(GetExceptedLogoColorObjects())
                .ToList()
                .ForEach(_Shape => _Shape.Color = _Color.SetA(_Shape.Color.a));
        }

        protected virtual void InitStartLogo()
        {
            var screenBounds = GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera);
            bool isLowAspect = GraphicUtils.AspectRatio < 0.6f;
            const float additionalOffset = 3f;
            var go = PrefabSetManager.InitPrefab(
                ContainersGetter.GetContainer(ContainerNames.GameUI),
                "ui_game",
                PrefabName);
            StartLogoObj = go;
            float yPos = screenBounds.max.y - m_TopOffset - additionalOffset;
            go.transform.SetLocalPosXY(
                screenBounds.center.x,
                yPos);
            go.transform.localScale = Vector3.one * GraphicUtils.AspectRatio * (isLowAspect ? 7f : 5f);
            m_StartLogoCharAnims = KeysAndDelays.Keys
                .ToDictionary(
                    _C => _C, 
                    _C => go.GetCompItem<Animator>(_C));
            SetColors(ColorProvider.GetColor(ColorIdsCommon.UI));
        }
        

        protected virtual IEnumerable<ShapeRenderer> GetExceptedLogoColorObjects()
        {
            return new[] { (ShapeRenderer)null };
        }

        private void ShowStartLogo()
        {
            foreach ((string key, var value) in m_StartLogoCharAnims)
            {
                ShowStartLogoItem(key, KeysAndDelays[key]);
                value.speed = AnimationSpeed;
            }
        }

        private void HideStartLogo()
        {
            if (!LogoShowingAnimationPassed)
            {
                StartLogoObj.SetActive(false);
                return;
            }
            foreach (var anim in m_StartLogoCharAnims.Values)
                anim.speed = 3f;
            foreach (var anim in m_StartLogoCharAnims.Values)
                anim.SetTrigger(AnimKeyStartLogoDisappear);
        }
        
        private void ShowStartLogoItem(string _Key, float _Delay)
        {
            float time = GameTicker.Time;
            Cor.Run(Cor.WaitWhile(
                () => time + _Delay > GameTicker.Time,
                () => m_StartLogoCharAnims[_Key].SetTrigger(AnimKeyStartLogoAppear)));
        }
        
        #endregion
    }
}