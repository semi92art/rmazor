using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using UnityEngine;

namespace RMAZOR.Views
{
    [Serializable]
    public class LevelGenerationParams
    {
        // ReSharper disable once InconsistentNaming
        public Vector2Int size;
        public float      a;
        public int[]      pathLengths;
        public bool       inUse;

        /// <summary>
        /// Creates RazeMaze Generation Params
        /// </summary>
        /// <param name="_Size">Maze size, X - width and Y - height</param>
        /// <param name="_A">Maze items to path items ratio, value 0 to 1</param>
        /// <param name="_PathLengths">Path lengths</param>
        public LevelGenerationParams(V2Int _Size, float _A, int[] _PathLengths)
        {
            size        = _Size;
            a           = _A;
            pathLengths = _PathLengths;
        }
    }

    public interface ILevelGeneratorRmazor
    {
        MazeInfo CreateDefaultLevelInfo(V2Int _Size, bool _OnlyMazeItems = false);

        Entity<MazeInfo> GetLevelInfoRandom(LevelGenerationParams      _GenerationParams);
        Entity<MazeInfo> GetLevelInfoRandomAsync(LevelGenerationParams _GenerationParams);
    }

    public class LevelGeneratorRmazor : ILevelGeneratorRmazor
    {
        #region constants

        private const int GenerationAttemptsCount = 1;

        #endregion

        #region nonpublic members

        private static float Rand => MathUtils.RandomGen.NextFloat();

        #endregion
        
        #region inject
        
        private ILevelAnalyzerRmazor LevelAnalyzerRmazor { get; }

        public LevelGeneratorRmazor(ILevelAnalyzerRmazor _LevelAnalyzerRmazor)
        {
            LevelAnalyzerRmazor = _LevelAnalyzerRmazor;
        }

        #endregion
        
        #region api

        public MazeInfo CreateDefaultLevelInfo(V2Int _Size, bool _OnlyMazeItems = false)
        {
            GetDefaultPathItemsAndMazeItemsPositions(_Size, _OnlyMazeItems,
                out var pathItemsPositions, out var mazeItemsPositions);
            return new MazeInfo
            {
                Size = _Size,
                PathItems = pathItemsPositions.Select(_P => new PathItem {Position = _P}).ToList(),
                MazeItems = mazeItemsPositions.Select(_Pos => new MazeItem
                {
                    Position = _Pos, Type = EMazeItemType.Block, Path = new List<V2Int> {_Pos}
                }).ToList(),
                AdditionalInfo = new AdditionalInfo
                {
                    Arguments = "[Empty]",
                    Comment   = "[Empty]"
                }
            };
        }

        public Entity<MazeInfo> GetLevelInfoRandom(LevelGenerationParams _GenerationParams)
        {
            var pathLengths = _GenerationParams.pathLengths;
            if (pathLengths == null || !pathLengths.Any())
                return null;
            var entityLevelInfo = new Entity<MazeInfo>();
            CreateRandomLevelInfoCore(_GenerationParams, entityLevelInfo);
            return entityLevelInfo;
        }

        public Entity<MazeInfo> GetLevelInfoRandomAsync(LevelGenerationParams _GenerationParams)
        {
#if UNITY_WEBGL
            return GetLevelInfoRandom(_GenerationParams);
#else
            var entity = new Entity<MazeInfo>();
            var pathLengths = _GenerationParams.pathLengths;
            if (pathLengths == null || !pathLengths.Any())
            {
                entity.Result = EEntityResult.Fail;
                entity.Error = "Path lengths array is empty";
                return entity;
            }
            Task.Run(() =>
            {
                int attemptCount = 0;
                while (entity.Result != EEntityResult.Success || attemptCount++ < GenerationAttemptsCount)
                    CreateRandomLevelInfoCore(_GenerationParams, entity);
            });
            return entity;
#endif
        }

        private void CreateRandomLevelInfoCore(
            LevelGenerationParams _GenerationParams,
            Entity<MazeInfo>      _Entity)
        {
            int w = _GenerationParams.size.x;
            int h = _GenerationParams.size.y;
            var position = GetStartPosition(w, h);
            GetDefaultPathItemsAndMazeItemsPositions((V2Int)_GenerationParams.size, true,
                out var pathItemPositions, out var mazeItemsPositions);
            pathItemPositions.Add(position);
            mazeItemsPositions.Remove(position);
            var passLevelCommadsRecord = new InputCommandsRecord
                {Records = new List<InputCommandRecord>()};
            V2Int dir = GetRandomDirection();
            PaveTheWay(300,
                new V2Int(w, h),
                ref position,
                ref dir,
                mazeItemsPositions,
                pathItemPositions,
                passLevelCommadsRecord,
                _GenerationParams);
            foreach (var pathItem in pathItemPositions.ToList()
                .Where(_PathItemPos => mazeItemsPositions.Any(
                    _MazeItemPos => _MazeItemPos == _PathItemPos)))
                pathItemPositions.Remove(pathItem);
            var levelInfo = CreateLevelInfo(
                (V2Int) _GenerationParams.size,
                pathItemPositions, 
                mazeItemsPositions,
                passLevelCommadsRecord);
            bool isValid = LevelAnalyzerRmazor.IsValid(levelInfo);
            if (isValid)
            {
                var passMoveDirections = LevelAnalyzerRmazor.GetPassMoveDirections(levelInfo);
                passMoveDirections = RemoveAdjacentDuplicates(passMoveDirections);
                passLevelCommadsRecord.Records = passMoveDirections.Select(_Dir => new InputCommandRecord
                {
                    Command = GetInputCommandByDirection(_Dir)
                }).ToList();
            }
            RemoveEmptyRowsAndColumns(levelInfo);
            _Entity.Value = levelInfo;
            _Entity.Error = isValid ? null : "Level is not valid";
            _Entity.Result = isValid ? EEntityResult.Success : EEntityResult.Pending;
        }

        #endregion

        #region nonpublic methods

        private static V2Int GetStartPosition(int _Width, int _Height)
        {
            int xPos = 1 + Mathf.FloorToInt((_Width - 2) * Rand);
            int yPos = 1 + Mathf.FloorToInt((_Height - 2) * Rand);
            return new V2Int(xPos, yPos);
        }

        private static bool IsFillingCoefficientReached(
            V2Int              _MazeSize,
            IEnumerable<V2Int> _AlreadyGenerated,
            float              _FillingCoefficient)
        {
            int maxPathItemsCount = Mathf.RoundToInt(_MazeSize.X * _MazeSize.Y * _FillingCoefficient);
            var alreadyGeneratedPathItems = _AlreadyGenerated.ToList();
            return alreadyGeneratedPathItems.Count >= maxPathItemsCount;
        }

        private static void GetDefaultPathItemsAndMazeItemsPositions(
            V2Int           _Size,
            bool            _OnlyMazeItems,
            out List<V2Int> _PathItems,
            out List<V2Int> _MazeItemsPositions)
        {
            (_PathItems, _MazeItemsPositions) = (new List<V2Int>(), new List<V2Int>());
            for (int i = 0; i < _Size.X; i++)
            for (int j = 0; j < _Size.Y; j++)
            {
                var pos = new V2Int(i, j);
                if (_OnlyMazeItems || i == 0 || i == _Size.X - 1 || j == 0 || j == _Size.Y - 1)
                    _MazeItemsPositions.Add(pos);
                else
                    _PathItems.Add(pos);
            }
        }

        private void PaveTheWay(
            int                   _MovesCount,
            V2Int                 _MazeSize,
            ref V2Int             _Position,
            ref V2Int             _Direction,
            ICollection<V2Int>    _MazeItemPositions,
            ICollection<V2Int>    _PathItemPositions,
            InputCommandsRecord   _PassLevelCommandsRecord,
            LevelGenerationParams _GenerationParams)
        {
            int k = 0;
            int lengthIdx = 0;
            while (k++ < _MovesCount)
            {
                if (IsFillingCoefficientReached(_MazeSize, _PathItemPositions, _GenerationParams.a))
                    break;
                FollowDirectionByLength(
                    ref _Position,
                    ref _Direction,
                    _MazeItemPositions,
                    _PathItemPositions,
                    _PassLevelCommandsRecord,
                    _GenerationParams,
                    lengthIdx);
                lengthIdx = MathUtils.ClampInverse(++lengthIdx, 0, _GenerationParams.pathLengths.Length - 1);
            }
        }

        private EInputCommand? m_PrCommand;
        private int            m_ValidationDecimatorCounter;

        private void FollowDirectionByLength(
            ref V2Int             _Position,
            ref V2Int             _Direction,
            ICollection<V2Int>    _MazeItemPositions,
            ICollection<V2Int>    _PathItemPositions,
            InputCommandsRecord   _PassLevelCommandsRecord,
            LevelGenerationParams _Params,
            int                   _PathLenghtIndex)
        {
            var startPos = _Position;
            var dirs = _Direction == V2Int.Left || _Direction == V2Int.Right
                ? new List<V2Int> {V2Int.Up, V2Int.Down}
                : new List<V2Int> {V2Int.Left, V2Int.Right};
            int pathLength = _Params.pathLengths[_PathLenghtIndex];
            while (dirs.Any())
            {
                _Direction = GetNextDirection(ref dirs);
                if (!IsValidPosition(_Position + _Direction * pathLength, (V2Int)_Params.size))
                    continue;
                int nextPosIdx = 0;
                var newPathItemPositions = new List<V2Int>();
                bool addCommandToRecord = true;
                while (nextPosIdx < _Params.pathLengths[_PathLenghtIndex])
                {
                    _Position += _Direction;
                    var pos = _Position;
                    var mazeItem = _MazeItemPositions.FirstOrDefault(_Item => _Item == pos);
                    if (!IsValidPosition(pos, (V2Int) _Params.size))
                    {
                        _Position -= _Direction;
                        addCommandToRecord = false;
                        break;
                    }
                    if (mazeItem != default)
                    {
                        newPathItemPositions.Add(pos);
                        _MazeItemPositions.Remove(mazeItem);
                        _PathItemPositions.Add(_Position);
                    }
                    nextPosIdx++;
                }
                var levelInfo = CreateLevelInfo(
                    (V2Int) _Params.size,
                    _PathItemPositions,
                    _MazeItemPositions, 
                    _PassLevelCommandsRecord);
                m_ValidationDecimatorCounter++;
                bool isValid = LevelAnalyzerRmazor.IsValid(levelInfo);
                if (isValid)
                {
                    var inputCommand = GetInputCommandByDirection(_Direction);
                    var records = _PassLevelCommandsRecord.Records;
                    addCommandToRecord = addCommandToRecord && (!m_PrCommand.HasValue || m_PrCommand != inputCommand);
                    if (addCommandToRecord)
                    {
                        records.Add(new InputCommandRecord {Command = inputCommand, Span = default});
                        m_PrCommand = inputCommand;
                    }
                    break;
                }
                foreach (var pos in newPathItemPositions)
                {
                    _MazeItemPositions.Add(pos);
                    var item = _PathItemPositions.First(_PathItem => _PathItem == pos);
                    _PathItemPositions.Remove(item);
                }
                _Position = startPos;
            }
        }

        private static EInputCommand GetInputCommandByDirection(V2Int _Direction)
        {
            if (_Direction == V2Int.Left)  return EInputCommand.MoveLeft;
            if (_Direction == V2Int.Right) return EInputCommand.MoveRight;
            if (_Direction == V2Int.Down)  return EInputCommand.MoveDown;
            if (_Direction == V2Int.Up)    return EInputCommand.MoveUp;
            return default;
        }

        private static V2Int GetRandomDirection()
        {
            int dirIdx = Mathf.RoundToInt(Rand * Directions.Count * 0.99f - 0.48f);
            return new V2Int(Directions[dirIdx]);
        }

        private static V2Int GetNextDirection(ref List<V2Int> _Directions)
        {
            int idx = Mathf.RoundToInt(Rand * _Directions.Count * 0.99f - 0.49f);
            var dir = _Directions[idx];
            _Directions.Remove(dir);
            return dir;
        }

        private static List<Vector2Int> Directions => new List<Vector2Int>
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left
        };

        private static MazeInfo CreateLevelInfo(
            V2Int              _Size,
            IEnumerable<V2Int> _PathItemPositions,
            IEnumerable<V2Int> _MazeItemPositions,
            InputCommandsRecord _PassLevelCommandsRecord)
        {
            return new MazeInfo
            {
                Size = _Size,
                PathItems = _PathItemPositions.Select(_P => new PathItem {Position = _P}).ToList(),
                MazeItems = _MazeItemPositions.Select(_P => new MazeItem {Position = _P}).ToList(),
                AdditionalInfo = new AdditionalInfo {PassCommandsRecord = _PassLevelCommandsRecord}
            };
        }

        private static void RemoveEmptyRowsAndColumns(MazeInfo _LevelInfo)
        {
            while (IsColumnEmpty(_LevelInfo, 0))
                RemoveColumn(    _LevelInfo, 0);
            while (IsRowEmpty(   _LevelInfo, 0))
                RemoveRow(       _LevelInfo, 0);
            while (IsColumnEmpty(_LevelInfo, _LevelInfo.Size.X - 1))
                RemoveColumn(    _LevelInfo, _LevelInfo.Size.X - 1);
            while(IsRowEmpty(    _LevelInfo, _LevelInfo.Size.Y - 1))
                RemoveRow(       _LevelInfo, _LevelInfo.Size.Y - 1);
        }

        private static void RemoveColumn(MazeInfo _LevelInfo, int _PosX)
        {
            var mazeItems = _LevelInfo.MazeItems.ToList();
            foreach (var mazeItem in mazeItems.Where(_Item => _Item.Position.X == _PosX))
                _LevelInfo.MazeItems.Remove(mazeItem);
            foreach (var mazeItem in _LevelInfo.MazeItems.Where(_Item => _Item.Position.X > _PosX))
            {
                var pos = mazeItem.Position;
                mazeItem.Position = new V2Int(pos.X - 1, pos.Y);
            }
            var pathItems = _LevelInfo.PathItems.ToList();
            foreach (var pathItem in pathItems.Where(_Item => _Item.Position.X == _PosX))
                _LevelInfo.PathItems.Remove(pathItem);
            foreach (var pathItem in _LevelInfo.PathItems.Where(_Item => _Item.Position.X > _PosX))
            {
                var pos = pathItem.Position;
                pathItem.Position = new V2Int(pos.X - 1, pos.Y);
            }
            _LevelInfo.Size = new V2Int(_LevelInfo.Size.X - 1, _LevelInfo.Size.Y);
        }
        
        private static void RemoveRow(MazeInfo _LevelInfo, int _PosY)
        {
            var mazeItems = _LevelInfo.MazeItems.ToList();
            foreach (var mazeItem in mazeItems.Where(_Item => _Item.Position.Y == _PosY))
                _LevelInfo.MazeItems.Remove(mazeItem);
            foreach (var mazeItem in _LevelInfo.MazeItems.Where(_Item => _Item.Position.Y > _PosY))
            {
                var pos = mazeItem.Position;
                mazeItem.Position = new V2Int(pos.X, pos.Y - 1);
            }
            var pathItems = _LevelInfo.PathItems.ToList();
            foreach (var pathItem in pathItems.Where(_Item => _Item.Position.Y == _PosY))
                _LevelInfo.PathItems.Remove(pathItem);
            foreach (var pathItem in _LevelInfo.PathItems.Where(_Item => _Item.Position.Y > _PosY))
            {
                var pos = pathItem.Position;
                pathItem.Position = new V2Int(pos.X, pos.Y - 1);
            }
            _LevelInfo.Size = new V2Int(_LevelInfo.Size.X, _LevelInfo.Size.Y - 1);
        }
        
        private static bool IsColumnEmpty(MazeInfo _LevelInfo, int _PosX)
        {
            return _LevelInfo.PathItems.All(_Item => _Item.Position.X != _PosX);
        }
        
        private static bool IsRowEmpty(MazeInfo _LevelInfo, int _PosY)
        {
            return _LevelInfo.PathItems.All(_Item => _Item.Position.Y != _PosY);
        }

        private static bool IsValidPosition(V2Int _Position, V2Int _MazeSize)
        {
            return _Position.X >= 0 && _Position.Y >= 0 && _Position.X < _MazeSize.X && _Position.Y < _MazeSize.Y;
        }
        
        private static List<V2Int> RemoveAdjacentDuplicates(List<V2Int> _Arr)
        {
            if (_Arr == null || _Arr.Count <= 1)
                return _Arr;
            var arr2 = _Arr.ToArray();
            for (int dl = 3; dl >= 1; dl--)
                arr2 = RemoveAdjacentDuplicates(arr2, dl);
            return arr2.ToList();
        }

        private static V2Int[] RemoveAdjacentDuplicates(V2Int[] _Arr, int _DL)
        {
            if (_DL > _Arr.Length - _DL)
                return _Arr;
            var arr2 = _Arr.ToList();
            int k = 0;
            while (k < arr2.Count - _DL + 1)
            {
                var subArray = new V2Int[_DL];
                for (int i = 0; i < _DL; i++)
                    subArray[i] = arr2[i+k];               
                while (true)
                {
                    int m = FindLast(arr2.ToArray(), subArray);
                    if (m != k + _DL)
                        break;
                    for (int i = 0; i < _DL; i++)
                        arr2.RemoveAt(m);
                }
                k++;
            }
            return arr2.ToArray();
        }

        //https://stackoverflow.com/questions/55150204/find-subarray-in-array-in-c-sharp
        private static int FindLast(IReadOnlyList<V2Int> _Haystack, IReadOnlyList<V2Int> _Needle)
        {
            // iterate backwards, stop if the rest of the array is shorter than needle (i >= needle.Length)
            for (int i = _Haystack.Count - 1; i >= _Needle.Count - 1; i--)
            {
                bool found = true;
                // also iterate backwards through needle, stop if elements do not match (!found)
                for (int j = _Needle.Count - 1; j >= 0 && found; j--)
                {
                    // compare needle's element with corresponding element of haystack
                    found = _Haystack[i - (_Needle.Count - 1 - j)] == _Needle[j];
                }
                if (found)
                    // result was found, i is now the index of the last found element, so subtract needle's length - 1
                    return i - (_Needle.Count - 1);
            }
            // not found, return -1
            return -1;
        }

        #endregion
    }
}