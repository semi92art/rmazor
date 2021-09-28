using System.Collections.Generic;
using System.Linq;
using Entities;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public delegate void PathProceedHandler(V2Int PathItem);
    
    public interface IPathItemsProceeder : ICharacterMoveContinued
    {
        void OnMazeInfoSet(MazeInfo _Info);
        Dictionary<V2Int, bool> PathProceeds { get; }
        event PathProceedHandler PathProceedEvent;
        event NoArgsHandler AllPathsProceededEvent;
    }
    
    public class PathItemsProceeder : IPathItemsProceeder
    {
        #region inject
        
        private IModelData Data { get; }

        public PathItemsProceeder(IModelData _Data)
        {
            Data = _Data;
        }
        
        #endregion
        
        #region api

        public void OnMazeInfoSet(MazeInfo _Info)
        {
            PathProceeds = _Info.Path.ToDictionary(_P => _P, _P => false);
            PathProceeds[_Info.Path[0]] = true;
        }

        public Dictionary<V2Int, bool> PathProceeds { get; private set; }
        public event PathProceedHandler PathProceedEvent;
        public event NoArgsHandler AllPathsProceededEvent;

        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            foreach (var pathItem in RazorMazeUtils.GetFullPath(_Args.From, _Args.Position))
                ProceedPathItem(pathItem);
        }
        
        #endregion
        
        #region nonpublic methods

        private void ProceedPathItem(V2Int _PathItem)
        {
            if (!PathProceeds.ContainsKey(_PathItem) || PathProceeds[_PathItem])
                return;
            PathProceeds[_PathItem] = true;
            PathProceedEvent?.Invoke(_PathItem); 
            
            if (PathProceeds.Values.All(_Proceeded => _Proceeded))
                AllPathsProceededEvent?.Invoke();
        }
        
        #endregion
    }
}