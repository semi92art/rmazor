using System.Collections.Generic;
using Common.Constants;
using Common.Managers;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Utils;
using RMAZOR.Helpers;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using UnityEngine;

namespace RMAZOR.Views.UI.Game_UI_Top_Buttons
{
    public interface IViewGameUiButton
        : IInit, 
          IShowControls,
          IViewUIGetRenderers { }
    
    public abstract class ViewGameUiButtonBase : InitBase, IViewGameUiButton
    {
        #region constants

        protected const float TopOffset = 5f;

        #endregion
        
        #region nonpublic members
        
        protected readonly List<Component> Renderers = new List<Component>();
            
        protected ButtonOnRaycast ButtonOnRaycast;

        protected abstract bool   CanShow    { get; }
        protected abstract string PrefabName { get; }

        #endregion

        #region inject

        protected IModelGame                  Model             { get; }
        protected ICameraProvider             CameraProvider    { get; }
        protected IViewInputCommandsProceeder CommandsProceeder { get; }
        private   IPrefabSetManager           PrefabSetManager  { get; }
        private   IHapticsManager             HapticsManager    { get; }
        private   IViewInputTouchProceeder    TouchProceeder    { get; }
        protected IAnalyticsManager           AnalyticsManager  { get; }

        protected ViewGameUiButtonBase(
            IModelGame                  _Model,
            ICameraProvider             _CameraProvider,
            IViewInputCommandsProceeder _CommandsProceeder,
            IPrefabSetManager           _PrefabSetManager,
            IHapticsManager             _HapticsManager,
            IViewInputTouchProceeder    _TouchProceeder,
            IAnalyticsManager           _AnalyticsManager)
        {
            Model             = _Model;
            CameraProvider    = _CameraProvider;
            CommandsProceeder = _CommandsProceeder;
            PrefabSetManager  = _PrefabSetManager;
            HapticsManager    = _HapticsManager;
            TouchProceeder    = _TouchProceeder;
            AnalyticsManager  = _AnalyticsManager;
        }

        #endregion

        #region api
        
        public override void Init()
        {
            InitButton();
            CameraProvider.ActiveCameraChanged += OnActiveCameraChanged;
            base.Init();
        }

        public virtual void ShowControls(bool _Show, bool _Instantly)
        {
            if (_Show || _Instantly)
                ButtonOnRaycast.SetGoActive(_Show && CanShow);
            ButtonOnRaycast.enabled = _Show && CanShow;
        }

        public IEnumerable<Component> GetRenderers()
        {
            return Renderers;
        }

        #endregion

        #region nonpublic methods
        
        private void OnActiveCameraChanged(Camera _Camera)
        {
            var parent = _Camera.transform;
            var scaleVec = Vector2.one * GraphicUtils.AspectRatio * 3f;
            var pos = GetPosition(_Camera);
            ButtonOnRaycast.transform
                .SetParentEx(parent)
                .SetLocalScaleXY(scaleVec)
                .SetLocalPosXY(pos)
                .SetLocalPosZ(10f);
        }

        protected virtual void InitButton()
        {
            var cont = CameraProvider.Camera.transform;
            var buttonGo = PrefabSetManager.InitPrefab(
                cont, CommonPrefabSetNames.UiGame, PrefabName);
            var renderer = buttonGo.GetCompItem<SpriteRenderer>("button_sprite");
            renderer.sortingOrder = SortingOrders.GameUI;
            Renderers.Add(renderer);
            ButtonOnRaycast = buttonGo.GetCompItem<ButtonOnRaycast>("button");
            ButtonOnRaycast.Init(
                OnButtonPressed, 
                () => Model.LevelStaging.LevelStage, 
                CameraProvider,
                HapticsManager,
                TouchProceeder);
            buttonGo.SetActive(false);
        }

        protected abstract Vector2 GetPosition(Camera _Camera);

        protected abstract void OnButtonPressed();
        
        protected void CallCommand(EInputCommand _Command, Dictionary<string, object> _Args = null)
        {
            CommandsProceeder.RaiseCommand(_Command, _Args);
        }

        protected static Bounds GetVisibleBounds(Camera _Camera)
        {
            return GraphicUtils.GetVisibleBounds(_Camera);
        }

        #endregion
    }
}