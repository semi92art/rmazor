namespace Games.RazorMaze.Views.InputConfigurators
{
    public interface IInputConfigurator
    {
        event IntHandler Command; 
        void ConfigureInput();
        bool Locked { get; set; }
    }
}