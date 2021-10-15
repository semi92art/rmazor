using System.Collections.Generic;
using DI.Extensions;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.ContainerGetters
{
    public class ContainersGetter : IContainersGetter
    {
        #region nonpublic members
        
        private readonly Dictionary<string, Transform> m_Containers = new Dictionary<string, Transform>();
        private readonly Dictionary<string, bool> m_Initialized = new Dictionary<string, bool>();
        
        #endregion
        
        #region inject

        private ICoordinateConverter CoordinateConverter { get; }

        public ContainersGetter(ICoordinateConverter _CoordinateConverter)
        {
            CoordinateConverter = _CoordinateConverter;
        }
        
        #endregion

        #region api

        public Transform GetContainer(string _Name)
        {
            bool inMaze = _Name == ContainerNames.MazeItems || _Name == ContainerNames.Character;
            if (!m_Initialized.ContainsKey(_Name))
            {
                m_Initialized.Add(_Name, false);
                m_Containers.Add(_Name, null);
            }
            if (m_Initialized[_Name])
                return m_Containers[_Name];
            m_Containers[_Name] = CommonUtils.FindOrCreateGameObject(_Name, out _).transform;
            m_Containers[_Name].SetParent(inMaze ? GetContainer(ContainerNames.Maze) : null);
            m_Containers[_Name].SetPosXY(CoordinateConverter.GetMazeCenter());
            m_Initialized[_Name] = true;
            return m_Containers[_Name];
        }

        #endregion
    }
}