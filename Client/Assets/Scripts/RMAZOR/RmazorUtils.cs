using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using Common.Exceptions;
using Common.Extensions;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;
using UnityEngine;

namespace RMAZOR
{
    public static class RmazorUtils
    {
        #region api

        public static int[] LevelsInGroupArray = {5, 5, 5};
        
        public static Tuple<float, float> GetRightAndLeftScreenOffsets()
        {
            float ratio = GraphicUtils.AspectRatio;
            float leftScreenOffset, rightScreenOffset;
            if (ratio > 0.7f)       (leftScreenOffset, rightScreenOffset) = (1f, 1f);
            else if (ratio > 0.54f) (leftScreenOffset, rightScreenOffset) = (1f, 1f);
            else                    (leftScreenOffset, rightScreenOffset) = (0f, 0f);
            return new Tuple<float, float>(leftScreenOffset, rightScreenOffset);
        }
        
        public static readonly EInputCommand[] MoveCommands =
        {
            EInputCommand.MoveLeft,
            EInputCommand.MoveRight,
            EInputCommand.MoveDown,
            EInputCommand.MoveUp
        };
        
        public static readonly EInputCommand[] RotateCommands =
        {
            EInputCommand.RotateClockwise,
            EInputCommand.RotateCounterClockwise
        };

        public static readonly EInputCommand[] MoveAndRotateCommands =
            MoveCommands
                .Concat(RotateCommands)
                .ToArray();

        public static readonly EMazeItemType[] GravityItemTypes =
        {
            EMazeItemType.GravityBlock,
            EMazeItemType.GravityBlockFree,
            EMazeItemType.GravityTrap
        };
        
        public static V2Int GetDropDirection(EMazeOrientation _Orientation)
        {
            return _Orientation switch
            {
                EMazeOrientation.North => V2Int.Down,
                EMazeOrientation.South => V2Int.Up,
                EMazeOrientation.East  => V2Int.Right,
                EMazeOrientation.West  => V2Int.Left,
                _                     => throw new SwitchCaseNotImplementedException(_Orientation)
            };
        }
        
        public static bool MazeContainsGravityItems(IEnumerable<IMazeItemProceedInfo> _Infos)
        {
            return _Infos
                .Any(_Info => GravityItemTypes.ContainsAlt(_Info.Type));
        }
        
        public static V2Int GetDirectionVector(EDirection _Direction, EMazeOrientation _Orientation)
        {
            return _Orientation switch
            {
                EMazeOrientation.North => _Direction switch
                {
                    EDirection.Up    => V2Int.Up,
                    EDirection.Right => V2Int.Right,
                    EDirection.Down  => V2Int.Down,
                    EDirection.Left  => V2Int.Left,
                    _                        => throw new SwitchCaseNotImplementedException(_Direction)
                },
                EMazeOrientation.East => _Direction switch
                {
                    EDirection.Up    => V2Int.Left,
                    EDirection.Right => V2Int.Up,
                    EDirection.Down  => V2Int.Right,
                    EDirection.Left  => V2Int.Down,
                    _                        => throw new SwitchCaseNotImplementedException(_Direction)
                },
                EMazeOrientation.South => _Direction switch
                {
                    EDirection.Up    => V2Int.Down,
                    EDirection.Right => V2Int.Left,
                    EDirection.Down  => V2Int.Up,
                    EDirection.Left  => V2Int.Right,
                    _                        => throw new SwitchCaseNotImplementedException(_Direction)
                },
                EMazeOrientation.West => _Direction switch
                {
                    EDirection.Up    => V2Int.Right,
                    EDirection.Right => V2Int.Down,
                    EDirection.Down  => V2Int.Left,
                    EDirection.Left  => V2Int.Up,
                    _                        => throw new SwitchCaseNotImplementedException(_Direction)
                },
                _ => throw new SwitchCaseNotImplementedException(_Orientation)
            };
        }
        
        public static V2Int GetDirectionVector(V2Int _DirectionVector, EMazeOrientation _Orientation)
        {
            EDirection direction = default;
            if (_DirectionVector == V2Int.Left)
                direction = EDirection.Left;
            else if (_DirectionVector == V2Int.Right)
                direction = EDirection.Right;
            else if (_DirectionVector == V2Int.Down)
                direction = EDirection.Down;
            else if (_DirectionVector == V2Int.Up)
                direction = EDirection.Up;
            return GetDirectionVector(direction, _Orientation);
        }

        public static EDirection GetMoveDirection(V2Int _DirectionVector, EMazeOrientation _Orientation)
        {
            switch (_Orientation)
            {
                case EMazeOrientation.North:
                    if (_DirectionVector == V2Int.Up)
                        return EDirection.Up;
                    else if (_DirectionVector == V2Int.Right)
                        return EDirection.Right;
                    else if (_DirectionVector == V2Int.Down)
                        return EDirection.Down;
                    else if (_DirectionVector == V2Int.Left)
                        return EDirection.Left;
                    break;
                case EMazeOrientation.East:
                    if (_DirectionVector == V2Int.Left)
                        return EDirection.Up;
                    else if (_DirectionVector == V2Int.Up)
                        return EDirection.Right;
                    else if (_DirectionVector == V2Int.Right)
                        return EDirection.Down;
                    else if (_DirectionVector == V2Int.Down)
                        return EDirection.Left;
                    break;
                case EMazeOrientation.South:
                    if (_DirectionVector == V2Int.Down)
                        return EDirection.Up;
                    else if (_DirectionVector == V2Int.Left)
                        return EDirection.Right;
                    else if (_DirectionVector == V2Int.Up)
                        return EDirection.Down;
                    else if (_DirectionVector == V2Int.Right)
                        return EDirection.Left;
                    break;
                case EMazeOrientation.West:
                    if (_DirectionVector == V2Int.Right)
                        return EDirection.Up;
                    else if (_DirectionVector == V2Int.Down)
                        return EDirection.Right;
                    else if (_DirectionVector == V2Int.Left)
                        return EDirection.Down;
                    else if (_DirectionVector == V2Int.Up)
                        return EDirection.Left;
                    break;
                default: throw new SwitchCaseNotImplementedException(_Orientation);
            }
            throw new ArgumentException("Wrong direction vector");
        }

        public static V2Int[] GetFullPath(V2Int _From, V2Int _To)
        {
            int min, max;
            V2Int[] res = null;
            if (_From.X == _To.X)
            {
                min = Math.Min(_From.Y, _To.Y);
                max = Math.Max(_From.Y, _To.Y);
                res = new V2Int[max - min + 1];
                for (int i = min; i <= max; i++)
                {
                    int idx = min == _From.Y ? i - min : max - i;
                    try
                    {
                        res[idx] = new V2Int(_From.X, i);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Dbg.LogError(nameof(GetFullPath) + " Length: " + res.Length + "; idx: " + idx
                        + "; min: " + min + "max: " + max);
                        throw;
                    }
                }
            }
            if (_From.Y == _To.Y)
            {
                min = Math.Min(_From.X, _To.X);
                max = Math.Max(_From.X, _To.X);
                res = new V2Int[max - min + 1];
                for (int i = min; i <= max; i++)
                {
                    int idx = min == _From.X ? i - min : max - i;
                    try
                    {
                        res[idx] = new V2Int(i, _From.Y);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Dbg.LogError(nameof(GetFullPath) + " Length: " + res.Length + "; idx: " + idx
                                     + "; min: " + min + "max: " + max);
                        throw;
                    }
                }
            }
            if (res == null)
                throw new ArgumentException($"Cannot build direct path from {_From} to {_To}");
            return res;
        }

        /// <summary>
        /// returns -1 if _A less than _B, 0 if _A equals _B, 1 if _A greater than _B
        /// </summary>
        /// <param name="_Path">path</param>
        /// <param name="_A">first path item</param>
        /// <param name="_B">second path item</param>
        /// <returns></returns>
        public static int CompareItemsOnPath(V2Int[] _Path, V2Int _A, V2Int _B)
        {
            if (!_Path.Contains(_A))
                throw new ArgumentException($"Path from {_Path.First()} to" +
                                            $" {_Path.Last()} does not contain point _A {_A}");
            if (!_Path.Contains(_B))
                throw new ArgumentException($"Path from {_Path.Last()} to" +
                                            $" {_Path.Last()} does not contain point _B {_B}");
            if (_A == _B)
                return 0;
            float distA = Vector2.Distance(_Path[0], _A);
            float distB = Vector2.Distance(_Path[0], _B);
            return distA < distB ? -1 : 1;
        }

        public static int GetGroupIndex(long _LevelIndex)
        {
            long levelIndexTemp = 0;
            int groupIndexInList = 0;
            int groupIndex = 0;
            while (levelIndexTemp <= _LevelIndex)
            {
                levelIndexTemp += LevelsInGroupArray[groupIndexInList];
                groupIndex++;
                groupIndexInList++;
                if (groupIndexInList >= LevelsInGroupArray.Length)
                    groupIndexInList = 0;
            }
            return groupIndex;
        }

        public static int GetLevelsInGroup(int _GroupIndex)
        {
            int groupIndexInList = (_GroupIndex - 1) % LevelsInGroupArray.Length;
            return LevelsInGroupArray[groupIndexInList];
        }

        public static int GetIndexInGroup(long _LevelIndex)
        {
            int groupIndex = GetGroupIndex(_LevelIndex);
            int groupIndexInList = 0;
            long levelsCount = 0;
            for (int i = 0; i < groupIndex - 1; i++)
            {
                levelsCount += LevelsInGroupArray[groupIndexInList];
                groupIndexInList++;
                if (groupIndexInList >= LevelsInGroupArray.Length)
                    groupIndexInList = 0;
            }
            return (int)(_LevelIndex - levelsCount);
        }

        public static int GetFirstLevelInGroup(int _GroupIndex)
        {
            int groupIndexInList = 0;
            int index = 0;
            for (int i = 0; i < _GroupIndex - 1; i++)
            {
                index += LevelsInGroupArray[groupIndexInList];
                groupIndexInList++;
                if (groupIndexInList >= LevelsInGroupArray.Length)
                    groupIndexInList = 0;
            }
            return index;
        }

        public static bool IsLastLevelInGroup(long _LevelIndex)
        {
            int groupIdx = GetGroupIndex(_LevelIndex);
            int levelsInGroup = GetLevelsInGroup(groupIdx);
            int indexInGroup = GetIndexInGroup(_LevelIndex);
            return levelsInGroup == indexInGroup + 1;
        }

        #endregion
    }
}