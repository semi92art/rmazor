namespace Games.RazorMaze.Models.ItemProceeders
{
    public interface ICharacterMoveStarted
    {
        void OnCharacterMoveStarted(CharacterMovingEventArgs _Args);
    }
}