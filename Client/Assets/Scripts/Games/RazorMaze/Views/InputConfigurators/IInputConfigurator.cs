namespace Games.RazorMaze.Views.InputConfigurators
{
    public interface IInputConfigurator : IInit
    {
        event IntHandlerWithArgs Command; 
        bool Locked { get; set; }
    }
}