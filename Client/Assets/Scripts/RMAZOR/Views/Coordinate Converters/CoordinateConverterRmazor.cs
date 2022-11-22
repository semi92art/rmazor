using System;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using UnityEngine;

namespace RMAZOR.Views.Coordinate_Converters
{
    public interface ICoordinateConverter : ICoordinateConverterBase { }
    
    public class CoordinateConverterRmazor : ICoordinateConverter
    {
        #region nonpublic members
        private ICoordinateConverter CurrentCoordinateConverter => 
            RmazorUtils.IsBigMaze(Model.Data.Info.Size) ? 
            (ICoordinateConverter)CoordinateConverterForBigLevels 
            : CoordinateConverterForSmallLevels;

        #endregion

        #region inject

        private IModelGame                         Model                             { get; }
        private ICoordinateConverterForSmallLevels CoordinateConverterForSmallLevels { get; }
        private ICoordinateConverterForBigLevels   CoordinateConverterForBigLevels   { get; }

        protected CoordinateConverterRmazor(
            IModelGame                         _Model,
            ICoordinateConverterForSmallLevels _CoordinateConverterForSmallLevels,
            ICoordinateConverterForBigLevels   _CoordinateConverterForBigLevels)
        {
            Model                             = _Model;
            CoordinateConverterForSmallLevels = _CoordinateConverterForSmallLevels;
            CoordinateConverterForBigLevels   = _CoordinateConverterForBigLevels;
        }

        #endregion

        #region api

        public float Scale => CurrentCoordinateConverter.Scale;

        public Func<string, Transform> GetContainer
        {
            set
            {
                CoordinateConverterForSmallLevels.GetContainer = value;
                CoordinateConverterForBigLevels.GetContainer = value;
            }
        }

        public bool IsValid()
        {
            return CurrentCoordinateConverter.IsValid();
        }

        public void Init()
        {
            CoordinateConverterForSmallLevels.Init();
            CoordinateConverterForBigLevels.Init();
        }

        public void SetMazeInfo(MazeInfo _Info)
        {
            if (RmazorUtils.IsBigMaze(_Info.Size))
                CoordinateConverterForBigLevels.SetMazeInfo(_Info);
            else 
                CoordinateConverterForSmallLevels.SetMazeInfo(_Info);
        }

        public Bounds GetMazeBounds()
        {
            return CurrentCoordinateConverter.GetMazeBounds();
        }

        public Vector2 ToGlobalMazeItemPosition(Vector2 _Point)
        {
            return CurrentCoordinateConverter.ToGlobalMazeItemPosition(_Point);
        }

        public Vector2 ToLocalMazeItemPosition(Vector2 _Point)
        {
            return CurrentCoordinateConverter.ToLocalMazeItemPosition(_Point);
        }

        public Vector2 ToLocalCharacterPosition(Vector2 _Point)
        {
            return CurrentCoordinateConverter.ToLocalCharacterPosition(_Point);
        }

        #endregion
    }
}