using Games.RazorMaze.Models;
using Games.RazorMaze.Prot;
using Utils;

namespace Games.RazorMaze.Views
{
    public class MazeViewProt : IMazeView
    {
        private MazeProtItems Maze { get; set; }
        private IMazeModel Model { get; set; }

        public void Init(IMazeModel _Model)
        {
            Model = _Model;
            Maze = MazeProtItems.Create(
                _Model.Info, 
                CommonUtils.FindOrCreateGameObject("Maze", out _).transform, 
                true);
        }

        public void SetLevel(int _Level) { }
        public void StartRotation(MazeRotateDirection _Direction, MazeOrientation _Orientation)
        {
            throw new System.NotImplementedException();
        }

        public void Rotate(float _Progress)
        {
            throw new System.NotImplementedException();
        }

        public void FinishRotation(MazeRotateDirection _Direction, MazeOrientation _Orientation)
        {
            throw new System.NotImplementedException();
        }
    }
}