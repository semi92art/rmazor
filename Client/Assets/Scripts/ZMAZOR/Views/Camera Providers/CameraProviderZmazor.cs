using Common.Extensions;
using Common.Managers;
using Common.Ticker;
using Common.Utils;
using RMAZOR;
using RMAZOR.Camera_Providers;
using UnityEngine;

namespace ZMAZOR.Views.Camera_Providers
{
    public class CameraProviderZmazor : CameraProviderRmazor, IFixedUpdateTick
    {
        #region constants

        private const float MaxFollowDistanceX = 2f;
        private const float MaxFollowDistanceY = 2f;
        private const float MaxMazeBorderIndent = 5f;

        #endregion
        
        #region nonpublic members

        private Vector2? m_CameraPosition; 

        #endregion
        
        #region inject

        private ViewSettings    ViewSettings   { get; }

        private CameraProviderZmazor(
            IPrefabSetManager _PrefabSetManager,
            ViewSettings      _ViewSettings,
            IViewGameTicker   _ViewGameTicker) 
            : base(_PrefabSetManager, _ViewGameTicker)
        {
            ViewSettings   = _ViewSettings;
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

        #endregion

        #region nonpublic methods

        private void CameraFollow()
        {
            if (!LevelCameraInitialized || !FollowTransformIsNotNull)
                return;
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