﻿using System.Collections.Generic;
using DI.Extensions;
using Games.RazorMaze.Models;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.ContainerGetters
{
    public interface IContainersGetter : IOnLevelStageChanged
    {
        Transform GetContainer(string _Name);
    }
    
    public class ContainersGetter : IContainersGetter
    {
        #region nonpublic members
        
        private readonly Dictionary<string, Transform> m_Containers = new Dictionary<string, Transform>();
        private readonly Dictionary<string, bool> m_Initialized = new Dictionary<string, bool>();
        
        #endregion
        
        #region inject

        private IModelGame               Model               { get; }
        private IMazeCoordinateConverter CoordinateConverter { get; }

        public ContainersGetter(IModelGame _Model, IMazeCoordinateConverter _CoordinateConverter)
        {
            Model = _Model;
            CoordinateConverter = _CoordinateConverter;
            CoordinateConverter.GetContainer = GetContainer;
        }
        
        #endregion

        #region api
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage == ELevelStage.Loaded)
            {
                CoordinateConverter.MazeSize = Model.Data.Info.Size;
                var mazeItemsCont = GetContainer(ContainerNames.MazeItems);
                mazeItemsCont.SetLocalPosXY(Vector2.zero);
                mazeItemsCont.PlusLocalPosY(CoordinateConverter.Scale * 0.5f);
            }
        }

        public Transform GetContainer(string _Name)
        {
            bool isMazeHolder = _Name == ContainerNames.MazeHolder;
            bool isMaze = _Name == ContainerNames.Maze;
            bool inMaze = _Name == ContainerNames.MazeItems || _Name == ContainerNames.Character;
            if (!m_Initialized.ContainsKey(_Name))
            {
                m_Initialized.Add(_Name, false);
                m_Containers.Add(_Name, null);
            }
            if (m_Initialized[_Name])
                return m_Containers[_Name];
            var cont = CommonUtils.FindOrCreateGameObject(_Name, out _).transform;
            cont.SetParent(inMaze ? GetContainer(ContainerNames.Maze) : null);
            if (isMazeHolder)
                cont.SetLocalPosXY(CoordinateConverter.GetMazeCenter());
            else if (isMaze)
            {
                cont.SetParent(GetContainer(ContainerNames.MazeHolder));
                cont.SetLocalPosXY(Vector2.zero);
            }
            m_Initialized[_Name] = true;
            m_Containers[_Name] = cont;
            return m_Containers[_Name];
        }

        #endregion
    }
}