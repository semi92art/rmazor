using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using RMAZOR.Models.MazeInfos;

namespace RMAZOR.Models
{
    public interface IModelMazeInfoCorrector
    {
        MazeInfo GetCorrectedMazeInfo(MazeInfo _Info);
    }
    
    public class ModelMazeInfoCorrectorWithWallSurrounding : IModelMazeInfoCorrector
    {
        public MazeInfo GetCorrectedMazeInfo(MazeInfo _Info)
        {
            var info = AddMissingPathItems(_Info);
            info = SurroundWithAdditionalWallsIfItNeeds(info);
            return info;
        }

        private static MazeInfo AddMissingPathItems(MazeInfo _Info)
        {
            var itemsForAdditionalNodes = _Info.MazeItems
                .Where(_Item =>
                    _Item.Type == EMazeItemType.Portal
                    || _Item.Type == EMazeItemType.Springboard
                    || _Item.Type == EMazeItemType.GravityBlock
                    || _Item.Type == EMazeItemType.GravityTrap
                    || _Item.Type == EMazeItemType.ShredingerBlock
                    || _Item.Type == EMazeItemType.TrapMoving
                    || _Item.Type == EMazeItemType.GravityBlockFree
                    || _Item.Type == EMazeItemType.Spear
                    || _Item.Type == EMazeItemType.Diode)
                .ToList();
            foreach (var item in itemsForAdditionalNodes
                .Where(_Item => !_Info.PathItems.Select(_PI => _PI.Position).Contains(_Item.Position)))
            {
                var pathItem = new PathItem
                {
                    Position = item.Position,
                    Blank = item.Blank
                };
                _Info.PathItems.Add(pathItem);
            }
            return _Info;
        }

        private static MazeInfo SurroundWithAdditionalWallsIfItNeeds(MazeInfo _Info)
        {
            var points = _Info.MazeItems
                .Select(_I => _I.Position)
                .Concat(_Info.PathItems.Select(_I => _I.Position))
                .Distinct()
                .ToList();
            int minX = points.Min(_P => _P.X);
            int minY = points.Min(_P => _P.Y);
            int maxX = points.Max(_P => _P.X);
            int maxY = points.Max(_P => _P.Y);
            var xCoords = Enumerable.Range(minX, maxX + 1 - minX).ToList();
            var yCoords = Enumerable.Range(minY, maxY + 1 - minY).ToList();
            int xLeftAddiction = 0, xRightAddiction = 0;
            int yLeftAddiction = 0, yRightAddiction = 0;
            var additionalPoints = new List<V2Int>();
            if (yCoords.Any(_YPos => IsMazeItemOrNode(new V2Int(minX, _YPos), _Info)))
            {
                var toAdd = yCoords.Select(_P => new V2Int(minX - 1, _P));
                additionalPoints = additionalPoints.Concat(toAdd).ToList();
                xLeftAddiction++;
            }
            if (yCoords.Any(_YPos => IsMazeItemOrNode(new V2Int(maxX, _YPos), _Info)))
            {
                var toAdd = yCoords.Select(_P => new V2Int(maxX + 1, _P));
                additionalPoints = additionalPoints.Concat(toAdd).ToList();
                xRightAddiction++;
            }
            if (xCoords.Any(_XPos => IsMazeItemOrNode(new V2Int(_XPos, minY), _Info)))
            {
                var toAdd = xCoords.Select(_P => new V2Int(_P, minY - 1));
                additionalPoints = additionalPoints.Concat(toAdd).ToList();
                yLeftAddiction++;
            }
            if (xCoords.Any(_XPos => IsMazeItemOrNode(new V2Int(_XPos, maxY), _Info)))
            {
                var toAdd = xCoords.Select(_P => new V2Int(_P, maxY + 1));
                additionalPoints = additionalPoints.Concat(toAdd).ToList();
                yRightAddiction++;
            }
            var additionalItems = additionalPoints.Select(_P => new MazeItem
            {
                Position = _P,
                Type = EMazeItemType.Block
            });
            _Info.MazeItems = _Info.MazeItems
                .Concat(additionalItems)
                .ToList();
            var newSize = new V2Int(
                _Info.Size.X + xLeftAddiction + xRightAddiction,
                _Info.Size.Y + yLeftAddiction + yRightAddiction);
            _Info.Size = newSize;
            if (xLeftAddiction <= 0 && yLeftAddiction <= 0) 
                return _Info;
            var addict = new V2Int(xLeftAddiction, yLeftAddiction);
            _Info.MazeItems.ForEach(_I =>
            {
                _I.Position += addict;
                _I.Pair += addict;
                for (int i = 0; i < _I.Path.Count; i++)
                    _I.Path[i] += addict;
            });
            _Info.PathItems.ForEach(_I => _I.Position += addict);
            return _Info;
        }
        
        private static bool IsMazeItemOrNode(V2Int _Point, MazeInfo _Info)
        {
            return _Info.MazeItems.Any(_Item => _Item.Position == _Point 
                                                && _Item.Type != EMazeItemType.Block)
                   || _Info.PathItems.Any(_Item => _Item.Position == _Point);
        }
    }
}