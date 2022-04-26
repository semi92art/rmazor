namespace RMAZOR.Models.ItemProceeders.Additional
{
    public interface ICharacterMoveStarted
    {
        void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args);
    }
}