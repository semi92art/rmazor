namespace Games.RazorMaze.Views
{
    public interface IInputConfigurator
    {
        event IntHandler OnCommand; 
        void ConfigureInput();
    }
}