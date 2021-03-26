namespace Games.RazorMaze.Views.InputConfigurators
{
    public interface IInputConfigurator
    {
        event IntHandler Command; 
        void ConfigureInput();
    }
}