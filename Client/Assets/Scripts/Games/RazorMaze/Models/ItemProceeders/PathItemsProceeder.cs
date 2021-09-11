using System.Linq;
using Entities;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public delegate void PathProceedHandler(V2Int PathItem);
    
    public interface IPathItemsProceeder : ICharacterMoveContinued
    {
        event PathProceedHandler PathProceedEvent;
        event NoArgsHandler AllPathsProceededEvent;
    }
    
    public class PathItemsProceeder : IPathItemsProceeder
    {
        #region inject
        
        private IModelMazeData Data { get; }

        public PathItemsProceeder(IModelMazeData _Data)
        {
            Data = _Data;
        }
        
        #endregion
        
        #region api
        
        public event PathProceedHandler PathProceedEvent;
        public event NoArgsHandler AllPathsProceededEvent;

        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            foreach (var pathItem in RazorMazeUtils.GetFullPath(_Args.From, _Args.Current))
                ProceedPathItem(pathItem);
        }
        
        #endregion
        
        #region nonpublic methods

        private void ProceedPathItem(V2Int _PathItem)
        {
            if (!Data.PathProceeds.ContainsKey(_PathItem) || Data.PathProceeds[_PathItem])
                return;
            Data.PathProceeds[_PathItem] = true;
            PathProceedEvent?.Invoke(_PathItem); 
            
            if (Data.PathProceeds.Values.All(_Proceeded => _Proceeded))
                AllPathsProceededEvent?.Invoke();
        }
        
        #endregion
    }
}