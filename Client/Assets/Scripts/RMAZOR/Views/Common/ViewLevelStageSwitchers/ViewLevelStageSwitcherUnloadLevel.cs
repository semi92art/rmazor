using System;
using System.Collections.Generic;
using System.Linq;
using mazing.common.Runtime.Extensions;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using UnityEngine.Events;

namespace RMAZOR.Views.Common.ViewLevelStageSwitchers
{
    public interface IViewLevelStageSwitcherUnloadLevel
        : IViewLevelStageSwitcherSingleStage { }
    
    public class ViewLevelStageSwitcherUnloadLevel : IViewLevelStageSwitcherUnloadLevel
    {
        #region inject

        private IViewInputCommandsProceeder CommandsProceeder { get; }
        private IModelGame                  Model             { get; }

        public ViewLevelStageSwitcherUnloadLevel(
            IViewInputCommandsProceeder _CommandsProceeder,
            IModelGame                  _Model)
        {
            CommandsProceeder = _CommandsProceeder;
            Model             = _Model;
        }

        #endregion

        #region api

        public void SwitchLevelStage(Dictionary<string, object> _Args)
        {
            if (Model.LevelStaging.LevelStage == ELevelStage.None)
                return;
            _Args.RemoveSafe(ComInComArg.KeyChallengeGoal, out _);
            RemoveMethodArgs(_Args);
            CommandsProceeder.RaiseCommand(EInputCommand.UnloadLevel, _Args, true);
        }

        #endregion
        
        #region nonpublic methods

        private void RemoveMethodArgs(Dictionary<string, object> _Args)
        {
            var keysToRemove = new List<string>();
            foreach ((string key, var value) in _Args.ToList())
            {
                var type = value.GetType();
                if (IsAction(type) || IsFunc(type) || IsUnityAction(type))
                    keysToRemove.Add(key);
            }
            foreach (string key in keysToRemove)
                _Args.RemoveSafe(key, out _);
        }
        
        private static bool IsAction(Type _Type)
        {
            if (_Type == typeof(Action)) return true;
            Type generic = null;
            if (_Type.IsGenericTypeDefinition) generic = _Type;
            else if (_Type.IsGenericType) generic = _Type.GetGenericTypeDefinition();
            var genericTypes = new[]
            {
                typeof(Action<>), typeof(Action<,>), typeof(Action<,,>), typeof(Action<,,,>),
                typeof(Action<,,,,>), typeof(Action<,,,,,>), typeof(Action<,,,,,,>), typeof(Action<,,,,,,,>),
                typeof(Action<,,,,,,,,>), typeof(Action<,,,,,,,,,>), typeof(Action<,,,,,,,,,,>),
                typeof(Action<,,,,,,,,,,,>), typeof(Action<,,,,,,,,,,,,>), typeof(Action<,,,,,,,,,,,,,>),
                typeof(Action<,,,,,,,,,,,,,,>), typeof(Action<,,,,,,,,,,,,,,,>)
            };
            return genericTypes.Contains(generic);
        }
        
        private static bool IsFunc(Type _Type)
        {
            if (_Type == typeof(Func<>)) return true;
            Type generic = null;
            if (_Type.IsGenericTypeDefinition) generic = _Type;
            else if (_Type.IsGenericType) generic = _Type.GetGenericTypeDefinition();
            var genericTypes = new[]
            {
                typeof(Func<,>), typeof(Func<,,>), typeof(Func<,,,>),
                typeof(Func<,,,,>), typeof(Func<,,,,,>), typeof(Func<,,,,,,>), typeof(Func<,,,,,,,>),
                typeof(Func<,,,,,,,,>), typeof(Func<,,,,,,,,,>), typeof(Func<,,,,,,,,,,>),
                typeof(Func<,,,,,,,,,,,>), typeof(Func<,,,,,,,,,,,,>), typeof(Func<,,,,,,,,,,,,,>),
                typeof(Func<,,,,,,,,,,,,,,>), typeof(Func<,,,,,,,,,,,,,,,>), typeof(Func<,,,,,,,,,,,,,,,,>),
                typeof(Func<,,,,,,,,,,,,,,,,>), typeof(Func<,,,,,,,,,,,,,,,,>)
            };
            return genericTypes.Contains(generic);
        }
        
        private static bool IsUnityAction(Type _Type)
        {
            if (_Type == typeof(UnityAction)) return true;
            Type generic = null;
            if (_Type.IsGenericTypeDefinition) generic = _Type;
            else if (_Type.IsGenericType) generic = _Type.GetGenericTypeDefinition();
            var genericTypes = new[]
            {
                typeof(UnityAction<>), typeof(UnityAction<,>), typeof(UnityAction<,,>), typeof(UnityAction<,,,>),
            };
            return genericTypes.Contains(generic);
        }
        
        #endregion
    }
}