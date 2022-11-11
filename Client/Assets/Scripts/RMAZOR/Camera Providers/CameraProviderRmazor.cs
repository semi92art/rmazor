using System;
using Common.CameraProviders;
using Common.Entities;
using Common.Helpers;
using UnityEngine;

namespace RMAZOR.Camera_Providers
{
    public class CameraProviderRmazor : InitBase, ICameraProvider
    {
        #region nonpublic members
        
        private ICameraProvider CurrentProvider =>
            RmazorUtils.IsBigMaze(CommonDataRmazor.LastMazeSize) ? 
                (ICameraProvider) DynamicCameraProvider 
                : StaticCameraProvider;

        #endregion

        #region inject

        private IStaticCameraProvider  StaticCameraProvider  { get; }
        private IDynamicCameraProvider DynamicCameraProvider { get; }

        public CameraProviderRmazor(
            IStaticCameraProvider  _StaticCameraProvider,
            IDynamicCameraProvider _DynamicCameraProvider)
        {
            StaticCameraProvider  = _StaticCameraProvider;
            DynamicCameraProvider = _DynamicCameraProvider;
        }

        #endregion

        #region api
        
        public override void Init()
        {
            if (Initialized)
                return;
            CommonDataRmazor.LastMazeSizeChanged += OnLastMazeSizeChanged;
            StaticCameraProvider.Init();
            DynamicCameraProvider.Init();
            base.Init();
        }

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

        #endregion

        #region nonpublic methods

        private void OnLastMazeSizeChanged(V2Int _Size)
        {
            bool isBigMaze = RmazorUtils.IsBigMaze(_Size);
            StaticCameraProvider.Camera.enabled = !isBigMaze;
            DynamicCameraProvider.Camera.enabled = isBigMaze;
        }

        #endregion
    }
}