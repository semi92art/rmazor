namespace Games.RazorMaze.Models
{
    public delegate void EInputCommandHandler(EInputCommand _Command);
    
    public interface IInputScheduler
    {
        event EInputCommandHandler MoveCommand; 
        event EInputCommandHandler RotateCommand; 
        void AddCommand(EInputCommand _Command);
        //void LockMovement();
        void UnlockMovement();
        //void LockRotation();
        void UnlockRotation();
    }
}