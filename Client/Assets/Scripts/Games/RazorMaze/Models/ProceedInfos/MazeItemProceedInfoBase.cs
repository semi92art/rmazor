namespace Games.RazorMaze.Models.ProceedInfos
{
    public interface IMazeItemProceedInfo
    {
        MazeItem Item { get; set; }
        bool IsProceeding { get; set; }
        int ProceedingStage { get; set; }
        float PauseTimer { get; set; }
    }
    
    public abstract class MazeItemProceedInfoBase : IMazeItemProceedInfo
    {
        public MazeItem Item { get; set; }
        public bool IsProceeding { get; set; }
        public int ProceedingStage { get; set; }
        public float PauseTimer { get; set; }
    }
}