namespace Games.RazorMaze.Views.InputConfigurators
{
    public interface IInputConfigurator : IInit
    {
        event IntHandler Command; 
        bool Locked { get; set; }
    }
}