using System.Collections.Generic;
using Constants;
using DI.Extensions;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Lean.Touch;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Games.RazorMaze.Views.InputConfigurators
{
    public interface IViewInputController : IInit, IOnLevelStageChanged { }
    
    public class ViewInputController : IViewInputController
    {
        protected IViewInputCommandsProceeder CommandsProceeder { get; }
        protected IViewInputTouchProceeder    TouchProceeder    { get; }

        public ViewInputController(
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewInputTouchProceeder _TouchProceeder)
        {
            CommandsProceeder = _CommandsProceeder;
            TouchProceeder = _TouchProceeder;
        }

        public event UnityAction Initialized;
        
        public void Init()
        {
            TouchProceeder.Init();
            Initialized?.Invoke();
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            TouchProceeder.OnLevelStageChanged(_Args);
        }
    }
}