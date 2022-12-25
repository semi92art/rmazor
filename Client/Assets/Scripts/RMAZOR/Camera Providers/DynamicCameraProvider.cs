using Common.Entities;
using Common.Extensions;
using Common.Managers;
using Common.Utils;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Views;
using UnityEngine;

namespace RMAZOR.Camera_Providers
{
    public interface IDynamicCameraProvider : ICameraProvider { }
    
    public class DynamicCameraProvider :
        CameraProviderBase,
        IDynamicCameraProvider,
        IFixedUpdateTick,
        IOnLevelStageChanged
    {
        #region constants

        private const float MaxFollowDistanceX  = 2f;
        private const float MaxFollowDistanceY  = 2f;
        private const float MaxMazeBorderIndent = 5f;

        #endregion
        
        #region nonpublic members

        protected override string CameraName => "Dynamic Camera";
        
        private Vector2? m_CameraPosition;
        private bool     m_EnableFollow;

            #endregion
        
        #region inject

        private IModelGame   Model        { get; }
        private ViewSettings ViewSettings { get; }

        protected DynamicCameraProvider(
            IModelGame        _Model,
            IPrefabSetManager _PrefabSetManager,
            ViewSettings      _ViewSettings,
            IViewGameTicker   _ViewGameTicker) 
            : base(_PrefabSetManager, _ViewGameTicker)
        {
            Model        = _Model;
            ViewSettings = _ViewSettings;
        }

        #endregion

        #region api
        
        public override void Init()
        {
            if (Initialized)
                return;
            ViewGameTicker.Register(this);
            base.Init();
        }

        public void FixedUpdateTick()
        {
            CameraFollow();
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.LevelStage != ELevelStage.Loaded || _Args.PreviousStage == ELevelStage.Paused)
                return;
            m_EnableFollow = RmazorUtils.IsBigMaze(Model.Data.Info.Size);
        }

        #endregion

        #region nonpublic methods
        
        private void OnLastMazeSizeChanged(V2Int _Size)
        {
            
        }

        private void CameraFollow()
        {
            if (!LevelCameraInitialized
                || !FollowTransformIsNotNull
                || !m_EnableFollow)
            {
                return;
            }
            var camPos = SetCameraPositionRaw();
            camPos = KeepCameraInCharacterRectangle(camPos);
            camPos = KeepCameraInMazeRectangle(camPos);
            m_CameraPosition = camPos;
            LevelCameraTr.SetPosXY(camPos);
        }

        private Vector2 SetCameraPositionRaw()
        {
            if (!m_CameraPosition.HasValue)
                m_CameraPosition = Follow.position;
            else
            {
                var newPos = Vector2.Lerp(
                    m_CameraPosition.Value, Follow.position, ViewSettings.cameraSpeed);
                m_CameraPosition = newPos;
            }
            return m_CameraPosition!.Value;
        }

        private Vector2 KeepCameraInCharacterRectangle(Vector2 _CameraPosition)
        {
            if (GetConverterScale == null)
                return _CameraPosition;
            float scale = GetConverterScale();
            var followPos = Follow.position;
            var camPos = _CameraPosition;
            float minX = followPos.x - MaxFollowDistanceX * scale;
            float maxX = followPos.x + MaxFollowDistanceX * scale;
            float minY = followPos.y - MaxFollowDistanceY * scale;
            float maxY = followPos.y + MaxFollowDistanceY * scale;
            camPos.x = MathUtils.Clamp(camPos.x, minX, maxX);
            camPos.y = MathUtils.Clamp(camPos.y, minY, maxY);
            return camPos;
        }

        private Vector2 KeepCameraInMazeRectangle(Vector2 _CameraPosition)
        {
            if (GetConverterScale == null)
                return _CameraPosition;
            float scale = GetConverterScale();
            var mazeBounds = GetMazeBounds();
            var camPos = _CameraPosition;
            float minX = mazeBounds.min.x + MaxMazeBorderIndent * scale;
            float maxX = mazeBounds.max.x - MaxMazeBorderIndent * scale;
            float minY = mazeBounds.min.y + MaxMazeBorderIndent * scale;
            float maxY = mazeBounds.max.y - MaxMazeBorderIndent * scale;
            camPos.x = MathUtils.Clamp(camPos.x, minX, maxX);
            camPos.y = MathUtils.Clamp(camPos.y, minY, maxY);
            return camPos;
        }

        #endregion
    }
}