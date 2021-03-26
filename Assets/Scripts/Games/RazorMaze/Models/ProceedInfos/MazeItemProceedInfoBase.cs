namespace Games.RazorMaze.Models.ProceedInfos
{
    public abstract class MazeItemProceedInfoBase
    {
        public MazeItem Item { get; set; }
        public bool IsProceeding { get; set; }
        public int ProceedingStage { get; set; }
        public float PauseTimer { get; set; }
    }
}