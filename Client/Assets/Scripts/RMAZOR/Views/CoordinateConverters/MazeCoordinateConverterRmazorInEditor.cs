using System;
using Common.CameraProviders;
using Common.Entities;
using UnityEngine;

namespace RMAZOR.Views.CoordinateConverters
{
    public interface ICoordinateConverterRmazorInEditor : ICoordinateConverterRmazorBase
    {
        void SetMazeSize(V2Int _Size);
    }
    
    [Serializable]
    public class CoordinateConverterRmazorInEditor :
        CoordinateConverterBase, 
        ICoordinateConverterRmazorInEditor
    {
        #region inject

        private CoordinateConverterRmazorInEditor(
            ViewSettings    _ViewSettings,
            ICameraProvider _CameraProvider)
            : base(_ViewSettings, _CameraProvider) { }

        #endregion

        #region factory method

        public static CoordinateConverterRmazorInEditor Create(
            ViewSettings    _ViewSettings,
            ICameraProvider _CameraProvider,
            bool            _Debug)
        {
            return new CoordinateConverterRmazorInEditor(_ViewSettings, _CameraProvider) {Debug = _Debug};
        }

        #endregion

        #region api

        public void SetMazeSize(V2Int _Size)
        {
            MazeSize = _Size;
            MazeDataWasSet = true;
            CheckForErrors();
            SetScale();
        }

        #endregion

        #region nonpublic menhods

        protected override Vector2 ToLocalMazePosition(Vector2 _Point)
        {
            CheckForErrors();
            return ScaleValue * (_Point - MazeSize * 0.5f);
        }
        
        #endregion
        
    }
}