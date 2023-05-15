using System.Globalization;
using Common.Constants;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Utils;
using TMPro;
using UnityEngine;

namespace RMAZOR.Views.UI
{
    public interface IViewGameUiCreatingLevelMessage : IInitViewUIItem, IOnLevelStageChanged
    {
        void ShowMessage();
    }
    
    public class ViewGameUiCreatingLevelMessage : IViewGameUiCreatingLevelMessage
    {
        #region nonpublic members

        private TextMeshPro m_MessageText;

        #endregion

        #region inject

        private ViewSettings         ViewSettings        { get; }
        private IModelGame           Model               { get; }
        private IPrefabSetManager    PrefabSetManager    { get; }
        private ILocalizationManager LocalizationManager { get; }
        private ICameraProvider      CameraProvider      { get; }
        private IViewGameTicker      ViewGameTicker      { get; }

        public ViewGameUiCreatingLevelMessage(
            ViewSettings         _ViewSettings,
            IModelGame           _Model,
            IPrefabSetManager    _PrefabSetManager,
            ILocalizationManager _LocalizationManager,
            ICameraProvider      _CameraProvider,
            IViewGameTicker      _ViewGameTicker)
        {
            ViewSettings        = _ViewSettings;
            Model               = _Model;
            PrefabSetManager    = _PrefabSetManager;
            LocalizationManager = _LocalizationManager;
            CameraProvider      = _CameraProvider;
            ViewGameTicker      = _ViewGameTicker;
        }

        #endregion

        #region api

        public void Init(Vector4 _Offsets)
        {
            InitMessage();
            CameraProvider.ActiveCameraChanged += OnActiveCameraChanged;
        }
        
        public void ShowMessage()
        {
            var levelStage = Model.LevelStaging.LevelStage;
            
            if (levelStage != ELevelStage.ReadyToUnloadLevel
                && levelStage != ELevelStage.Unloaded
                && levelStage != ELevelStage.None)
            {
                return;
            }
            m_MessageText.enabled = true;
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            string gameMode = (string) _Args.Arguments.GetSafe(ComInComArg.KeyGameMode, out _);
            if (gameMode != ComInComArg.ParameterGameModeRandom)
                return;
            switch (_Args.LevelStage)
            {
                case ELevelStage.ReadyToUnloadLevel: 
                    Cor.Run(Cor.Delay(
                        ViewSettings.betweenLevelTransitionTime * 0.8f, ViewGameTicker, ShowMessage)); 
                    break;
                case ELevelStage.Loaded: 
                case ELevelStage.ReadyToStart:
                    HideMessage();
                    break;
            }
        }

        #endregion

        #region nonpublic methods
        
        private void OnActiveCameraChanged(Camera _Camera)
        {
            var bounds = GraphicUtils.GetVisibleBounds(_Camera);
            m_MessageText.transform.SetParent(_Camera.transform);
            m_MessageText.transform
                .SetLocalPosX(bounds.center.x)
                .SetLocalPosY(bounds.center.y)
                .SetLocalPosZ(10f);
        }

        private void InitMessage()
        {
            var parent = CameraProvider.Camera.transform;
            var go = PrefabSetManager.InitPrefab(
                parent, CommonPrefabSetNames.UiGame, "creating_level_message");
            m_MessageText = go.GetCompItem<TextMeshPro>("text");
            m_MessageText.sortingOrder = SortingOrders.GameLogoBackground;
            m_MessageText.color = Color.black;
            var locTextInfo = new LocTextInfo(
                m_MessageText, ETextType.GameUI,
                "creating_level", 
                _T => _T.ToLower(CultureInfo.CurrentUICulture));
            LocalizationManager.AddLocalization(locTextInfo);
            HideMessage();
        }

        private void HideMessage()
        {
            m_MessageText.enabled = false;
        }

        #endregion
    }
}