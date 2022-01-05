namespace Games.RazorMaze.Models.ItemProceeders
{
    public interface ICharacterMoveFinished
    {
        void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args);
    }
}