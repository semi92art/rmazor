using System.Linq;
using RMAZOR.Models.MazeInfos;

namespace RMAZOR.Models
{
    public enum MazeOrientation { North, East, South, West }
    public enum EMazeMoveDirection { Up, Right, Down, Left }
    
    public interface IModelData
    {
        MazeInfo Info { get; set; }
        MazeOrientation Orientation { get; set; }
        bool ProceedingControls { get; set; }
    }
    
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ModelData : IModelData
    {
        #region nonpublic members
        
        private MazeInfo m_Info;

        #endregion

        #region api
        
        public MazeOrientation Orientation { get; set; } = MazeOrientation.North;
        public bool ProceedingControls { get; set; } = true;

        public MazeInfo Info
        {
            get => m_Info;
            set => m_Info = CorrectInfo(value);
        }

        #endregion

        #region nonpublic methods

        private static MazeInfo CorrectInfo(MazeInfo _Info)
        {
            var info = _Info;
            var additionalPathPositions = info.MazeItems
                .Where(_Item =>
                    _Item.Type == EMazeItemType.Portal
                    || _Item.Type == EMazeItemType.Springboard
                    || _Item.Type == EMazeItemType.GravityBlock
                    || _Item.Type == EMazeItemType.GravityTrap
                    || _Item.Type == EMazeItemType.ShredingerBlock
                    || _Item.Type == EMazeItemType.TrapMoving
                    || _Item.Type == EMazeItemType.GravityBlockFree)
                .Select(_Item => _Item.Position)
                .ToList();
            foreach (var pos in additionalPathPositions
                .Where(_Pos => !info.PathItems.Select(_PI => _PI.Position).Contains(_Pos)))
            {
                info.PathItems.Add(new PathItem {Position = pos});
            }
            return info;
        }

        #endregion
    }
}