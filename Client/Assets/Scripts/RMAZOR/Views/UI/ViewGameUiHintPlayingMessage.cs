using Common.Constants;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using UnityEngine;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Views.UI
{
    public interface IViewGameUiHintPlayingMessage
        : IInitViewUIItem,
          IOnLevelStageChanged { }
    
    public class ViewGameUiHintPlayingMessage : InitBase, IViewGameUiHintPlayingMessage
    {
        #region nonpublic members

        private Transform      m_MessageTr;
        private SpriteRenderer m_IconHint;

        #endregion

        #region inject

        private IPrefabSetManager PrefabSetManager { get; }
        private ICameraProvider   CameraProvider   { get; }

        private ViewGameUiHintPlayingMessage(
            IPrefabSetManager _PrefabSetManager,
            ICameraProvider   _CameraProvider)
        {
            PrefabSetManager = _PrefabSetManager;
            CameraProvider   = _CameraProvider;
        }

        #endregion

        #region api
        
        public void Init(Vector4 _Offsets)
        {
            InitMessage();
            CameraProvider.ActiveCameraChanged += OnActiveCameraChanged;
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.LevelStage)
            {
                case ELevelStage.ReadyToStart when _Args.PreviousStage == ELevelStage.Loaded:
                {
                    object showPromptArg = _Args.Arguments.GetSafe(KeyShowPuzzleLevelHint, out bool keyExist);
                    if (!keyExist || !(bool) showPromptArg)
                        break;
                    ShowPrompt();
                }
                    break;
                case ELevelStage.Finished when _Args.PreviousStage == ELevelStage.StartedOrContinued:
                {
                    _Args.Arguments.GetSafe(KeyShowPuzzleLevelHint, out bool keyExist);
                    if (!keyExist)
                        break;
                    HidePrompt();
                }
                    break;
            }
        }

        #endregion

        #region nonpublic methods

        private void OnActiveCameraChanged(Camera _Camera)
        {
            var parent = _Camera.transform;
            m_MessageTr.SetParent(parent);
            var bounds = GraphicUtils.GetVisibleBounds(_Camera);
            m_MessageTr
                .SetLocalScaleXY(Vector2.one)
                .SetLocalPosX(bounds.center.x)
                .SetLocalPosY(bounds.max.y - 8f);
        }

        private void InitMessage()
        {
            var parent = CameraProvider.Camera.transform;
            var go = PrefabSetManager.InitPrefab(
                parent, 
                CommonPrefabSetNames.UiGame,
                "hint_playing_message");
            m_MessageTr = go.transform;
            m_IconHint = go.GetCompItem<SpriteRenderer>("icon_hint");
            HidePrompt();
        }

        private void ShowPrompt()
        {
            m_IconHint.enabled = true;
        }

        private void HidePrompt()
        {
            m_IconHint.enabled = false;
        }

        #endregion
    }
}