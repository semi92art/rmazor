namespace Games.RazorMaze.Models.ItemProceeders
{
    public interface ICharacterMoveFinished
    {
        void OnCharacterMoveFinished(CharacterMovingEventArgs _Args);
    }
}