using System.Collections;
using Entities;
using Games.RazorMaze.Models;
using UnityEngine;
using Utils;

namespace Games.RazorMaze
{
    public interface ICharacterMover
    {
        event CharacterMovingHandler   CharacterMoveStarted;
        event CharacterMovingHandler   CharacterMoveContinued;
        event CharacterMovingHandler   CharacterMoveFinished;
        void MoveCharacter(V2Int _From, V2Int _To);
    }

    public class CharacterMover : ICharacterMover
    {
        #region inject

        private RazorMazeModelSettings Settings { get; }

        public CharacterMover(RazorMazeModelSettings _Settings)
        {
            Settings = _Settings;
        }

        #endregion

        #region api

        public event CharacterMovingHandler CharacterMoveStarted;
        public event CharacterMovingHandler CharacterMoveContinued;
        public event CharacterMovingHandler CharacterMoveFinished;

        public void MoveCharacter(V2Int _From, V2Int _To)
        {
            Coroutines.Run(MoveCharacterCore(_From, _To));
        }

        #endregion

        #region private methods

        private IEnumerator MoveCharacterCore(V2Int _From, V2Int _To)
        {
            CharacterMoveStarted?.Invoke(new CharacterMovingEventArgs(_From, _To, 0));
            int pathLength = Mathf.RoundToInt(Vector2Int.Distance(_From.ToVector2Int(), _To.ToVector2Int()));
            yield return Coroutines.Lerp(
                0f,
                1f,
                pathLength / Settings.characterSpeed,
                _Progress => CharacterMoveContinued?.Invoke(new CharacterMovingEventArgs(_From, _To, _Progress)),
                GameTimeProvider.Instance,
                (_Breaked, _Progress) => CharacterMoveFinished?.Invoke(new CharacterMovingEventArgs(_From, _To, _Progress)));
        }

        #endregion
    }
}