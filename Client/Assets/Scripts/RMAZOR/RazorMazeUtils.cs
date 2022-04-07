using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using Common.Exceptions;
using Common.Extensions;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;
using UnityEngine;

namespace RMAZOR
{
    public static class RazorMazeUtils
    {
        #region api

        public static readonly int[] LevelsInGroupList          = {3, 4, 5};
        public static          bool  LoadNextLevelAutomatically = true;
        
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
        
        public static V2Int GetDropDirection(MazeOrientation _Orientation)
        {
            return _Orientation switch
            {
                MazeOrientation.North => V2Int.Down,
                MazeOrientation.South => V2Int.Up,
                MazeOrientation.East  => V2Int.Right,
                MazeOrientation.West  => V2Int.Left,
                _                     => throw new SwitchCaseNotImplementedException(_Orientation)
            };
        }
        
        public static bool MazeContainsGravityItems(IEnumerable<IMazeItemProceedInfo> _Infos)
        {
            return _Infos
                .Any(_Info => GravityItemTypes.ContainsAlt(_Info.Type));
        }
        
        public static V2Int GetDirectionVector(EMazeMoveDirection _Direction, MazeOrientation _Orientation)
        {
            return _Orientation switch
            {
                MazeOrientation.North => _Direction switch
                {
                    EMazeMoveDirection.Up    => V2Int.Up,
                    EMazeMoveDirection.Right => V2Int.Right,
                    EMazeMoveDirection.Down  => V2Int.Down,
                    EMazeMoveDirection.Left  => V2Int.Left,
                    _                        => throw new SwitchCaseNotImplementedException(_Direction)
                },
                MazeOrientation.East => _Direction switch
                {
                    EMazeMoveDirection.Up    => V2Int.Left,
                    EMazeMoveDirection.Right => V2Int.Up,
                    EMazeMoveDirection.Down  => V2Int.Right,
                    EMazeMoveDirection.Left  => V2Int.Down,
                    _                        => throw new SwitchCaseNotImplementedException(_Direction)
                },
                MazeOrientation.South => _Direction switch
                {
                    EMazeMoveDirection.Up    => V2Int.Down,
                    EMazeMoveDirection.Right => V2Int.Left,
                    EMazeMoveDirection.Down  => V2Int.Up,
                    EMazeMoveDirection.Left  => V2Int.Right,
                    _                        => throw new SwitchCaseNotImplementedException(_Direction)
                },
                MazeOrientation.West => _Direction switch
                {
                    EMazeMoveDirection.Up    => V2Int.Right,
                    EMazeMoveDirection.Right => V2Int.Down,
                    EMazeMoveDirection.Down  => V2Int.Left,
                    EMazeMoveDirection.Left  => V2Int.Up,
                    _                        => throw new SwitchCaseNotImplementedException(_Direction)
                },
                _ => throw new SwitchCaseNotImplementedException(_Orientation)
            };
        }

        public static EMazeMoveDirection GetMoveDirection(V2Int _DirectionVector, MazeOrientation _Orientation)
        {
            switch (_Orientation)
            {
                case MazeOrientation.North:
                    if (_DirectionVector == V2Int.Up)
                        return EMazeMoveDirection.Up;
                    else if (_DirectionVector == V2Int.Right)
                        return EMazeMoveDirection.Right;
                    else if (_DirectionVector == V2Int.Down)
                        return EMazeMoveDirection.Down;
                    else if (_DirectionVector == V2Int.Left)
                        return EMazeMoveDirection.Left;
                    break;
                case MazeOrientation.East:
                    if (_DirectionVector == V2Int.Left)
                        return EMazeMoveDirection.Up;
                    else if (_DirectionVector == V2Int.Up)
                        return EMazeMoveDirection.Right;
                    else if (_DirectionVector == V2Int.Right)
                        return EMazeMoveDirection.Down;
                    else if (_DirectionVector == V2Int.Down)
                        return EMazeMoveDirection.Left;
                    break;
                case MazeOrientation.South:
                    if (_DirectionVector == V2Int.Down)
                        return EMazeMoveDirection.Up;
                    else if (_DirectionVector == V2Int.Left)
                        return EMazeMoveDirection.Right;
                    else if (_DirectionVector == V2Int.Up)
                        return EMazeMoveDirection.Down;
                    else if (_DirectionVector == V2Int.Right)
                        return EMazeMoveDirection.Left;
                    break;
                case MazeOrientation.West:
                    if (_DirectionVector == V2Int.Right)
                        return EMazeMoveDirection.Up;
                    else if (_DirectionVector == V2Int.Down)
                        return EMazeMoveDirection.Right;
                    else if (_DirectionVector == V2Int.Left)
                        return EMazeMoveDirection.Down;
                    else if (_DirectionVector == V2Int.Up)
                        return EMazeMoveDirection.Left;
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
                levelIndexTemp += LevelsInGroupList[groupIndexInList];
                groupIndex++;
                groupIndexInList++;
                if (groupIndexInList >= LevelsInGroupList.Length)
                    groupIndexInList = 0;
            }
            return groupIndex;
        }

        public static int GetLevelsInGroup(int _GroupIndex)
        {
            int groupIndexInList = (_GroupIndex - 1) % LevelsInGroupList.Length;
            return LevelsInGroupList[groupIndexInList];
        }

        public static int GetIndexInGroup(long _LevelIndex)
        {
            int groupIndex = GetGroupIndex(_LevelIndex);
            int groupIndexInList = 0;
            long levelsCount = 0;
            for (int i = 0; i < groupIndex - 1; i++)
            {
                levelsCount += LevelsInGroupList[groupIndexInList];
                groupIndexInList++;
                if (groupIndexInList >= LevelsInGroupList.Length)
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
                index += LevelsInGroupList[groupIndexInList];
                groupIndexInList++;
                if (groupIndexInList >= LevelsInGroupList.Length)
                    groupIndexInList = 0;
            }
            return index;
        }

        #endregion
    }
}