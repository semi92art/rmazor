namespace Games.RazorMaze.Views
{
    public interface IInputConfigurator
    {
        event IntHandler Command; 
        void ConfigureInput();
    }
}