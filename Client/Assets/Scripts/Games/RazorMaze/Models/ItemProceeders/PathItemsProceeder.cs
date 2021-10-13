﻿using System.Collections.Generic;
using System.Linq;
using Entities;
using Games.RazorMaze.Views;
using UnityEngine.Events;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public delegate void PathProceedHandler(V2Int PathItem);
    
    public interface IPathItemsProceeder : ICharacterMoveContinued
    {
        Dictionary<V2Int, bool> PathProceeds { get; }
        event PathProceedHandler PathProceedEvent;
        event UnityAction AllPathsProceededEvent;
    }
    
    public class PathItemsProceeder : IPathItemsProceeder, IOnLevelStageChanged
    {
        #region inject
        
        private IModelData Data { get; }

        public PathItemsProceeder(IModelData _Data)
        {
            Data = _Data;
        }
        
        #endregion
        
        #region api
        
        public Dictionary<V2Int, bool> PathProceeds { get; private set; }
        public event PathProceedHandler PathProceedEvent;
        public event UnityAction AllPathsProceededEvent;

        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            foreach (var pathItem in RazorMazeUtils.GetFullPath(_Args.From, _Args.Position))
                ProceedPathItem(pathItem);
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage == ELevelStage.Loaded)
                CollectPathProceeds();
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

        private void CollectPathProceeds()
        {
            PathProceeds = Data.Info.Path.ToDictionary(_P => _P, _P => false);
            PathProceeds[Data.Info.Path[0]] = true;
        }
        
        #endregion
    }
}