using System;
using Common.CameraProviders;
using Common.Extensions;
using Common.Helpers;
using Common.Ticker;
using Lean.Common;
using RMAZOR.Models;
using RMAZOR.Views;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.Camera_Providers
{
    public class CameraProviderRmazor : InitBase, ICameraProvider, IOnLevelStageChanged, IUpdateTick
    {
        #region nonpublic members
        
        private ICameraProvider CurrentProvider
        {
            get
            {
                if (Model.Data.Info == null)
                {
                    if (StaticCameraProvider.Camera.IsNull())
                        return CameraProviderFake;
                    return StaticCameraProvider;
                }
                return RmazorUtils.IsBigMaze(Model.Data.Info.Size)
                    ? (ICameraProvider) DynamicCameraProvider
                    : StaticCameraProvider;
            }
        }

        #endregion

        #region inject

        private IModelGame             Model                 { get; }
        private IStaticCameraProvider  StaticCameraProvider  { get; }
        private IDynamicCameraProvider DynamicCameraProvider { get; }
        private ICameraProviderFake    CameraProviderFake    { get; }
        private IViewGameTicker        Ticker                { get; }

        public CameraProviderRmazor(
            IModelGame             _Model,
            IStaticCameraProvider  _StaticCameraProvider,
            IDynamicCameraProvider _DynamicCameraProvider,
            ICameraProviderFake    _CameraProviderFake,
            IViewGameTicker _Ticker)
        {
            Model                 = _Model;
            StaticCameraProvider  = _StaticCameraProvider;
            DynamicCameraProvider = _DynamicCameraProvider;
            CameraProviderFake    = _CameraProviderFake;
            Ticker = _Ticker;
        }

        #endregion

        #region api
        
        public override void Init()
        {
            if (Initialized)
                return;
            StaticCameraProvider.Init();
            DynamicCameraProvider.Init();
            Ticker.Register(this);
            base.Init();
        }

        public event UnityAction<Camera> ActiveCameraChanged;

        public Func<Bounds> GetMazeBounds
        {
            set => CurrentProvider.GetMazeBounds = value;
        }

        public Func<float> GetConverterScale
        {
            set => CurrentProvider.GetConverterScale = value;
        }

        public Transform Follow
        {
            set
            {
                StaticCameraProvider.Follow = value;
                DynamicCameraProvider.Follow = value;
            }
        }

        public Camera Camera => CurrentProvider.Camera;

        public void SetEffectProps<T>(ECameraEffect _Effect, T _Args) where T : ICameraEffectProps
        {
            CurrentProvider.SetEffectProps(_Effect, _Args);
        }

        public void AnimateEffectProps<T>(ECameraEffect _Effect, T _From, T _To, float _Duration)
            where T : ICameraEffectProps
        {
            CurrentProvider.AnimateEffectProps(_Effect, _From, _To, _Duration);
        }

        public bool IsEffectEnabled(ECameraEffect _Effect)
        {
            return CurrentProvider.IsEffectEnabled(_Effect);
        }

        public void EnableEffect(ECameraEffect _Effect, bool _Enabled)
        {
            CurrentProvider.EnableEffect(_Effect, _Enabled);
        }

        public void UpdateState()
        {
            StaticCameraProvider.UpdateState();
            DynamicCameraProvider.UpdateState();
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.LevelStage != ELevelStage.Loaded || _Args.PreviousStage == ELevelStage.Paused)
                return;
            DynamicCameraProvider.Camera.enabled = false;
            StaticCameraProvider.Camera.enabled = false;
            Camera.enabled = true;
            ActiveCameraChanged?.Invoke(Camera);
            if (DynamicCameraProvider is IOnLevelStageChanged onLevelStageChangedDynamicCameraProvider)
                onLevelStageChangedDynamicCameraProvider.OnLevelStageChanged(_Args);
            if (CurrentProvider is IDynamicCameraProvider)
                CurrentProvider.Camera.tag = "MainCamera";
        }

        #endregion

        public void UpdateTick()
        {
            if (LeanInput.GetPressed(KeyCode.U))
                ActiveCameraChanged?.Invoke(Camera);
        }
    }
}