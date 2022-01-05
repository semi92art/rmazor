namespace Games.RazorMaze.Models.ItemProceeders
{
    public interface ICharacterMoveStarted
    {
        void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args);
    }
}