using System.Collections.Generic;
using Common.Constants;
using Common.Extensions;
using Common.Helpers;
using Common.Utils;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Coordinate_Converters;
using UnityEngine;

namespace RMAZOR.Views.ContainerGetters
{
    public interface IContainersGetterRmazorBase : IContainersGetter, IOnLevelStageChanged { }
    
    public abstract class ContainersGetterRmazorBase : IContainersGetterRmazorBase
    {
        #region nonpublic members

        private readonly Dictionary<string, Transform> m_Containers  = new Dictionary<string, Transform>();
        private readonly Dictionary<string, bool>      m_Initialized = new Dictionary<string, bool>();
        
        #endregion
        
        #region inject

        protected IModelGame                     Model               { get; }
        private   ICoordinateConverterBase CoordinateConverter { get; }

        protected ContainersGetterRmazorBase(
            IModelGame                     _Model,
            ICoordinateConverterBase _CoordinateConverter)
        {
            Model               = _Model;
            CoordinateConverter = _CoordinateConverter;
        }
        
        #endregion

        #region api
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.LevelStage != ELevelStage.Loaded) 
                return;
            UpdateCoordinateConverterState();
            var mazeHolderCont = GetContainer(ContainerNamesMazor.MazeHolder);
            mazeHolderCont.SetLocalPosXY(CoordinateConverter.GetMazeBounds().center);
            var mazeItemsCont = GetContainer(ContainerNamesMazor.MazeItems);
            mazeItemsCont.SetLocalPosXY(Vector2.zero);
            mazeItemsCont.PlusLocalPosY(CoordinateConverter.Scale * 0.5f);
        }

        public Transform GetContainer(string _Name)
        {
            bool isMaze = _Name == ContainerNamesMazor.Maze;
            bool inMaze = _Name == ContainerNamesMazor.MazeItems || _Name == ContainerNamesMazor.Character;
            if (!m_Initialized.ContainsKey(_Name))
            {
                m_Initialized.Add(_Name, false);
                m_Containers.Add(_Name, null);
            }
            if (m_Initialized[_Name])
                return m_Containers[_Name];
            var cont = CommonUtils.FindOrCreateGameObject(_Name, out _).transform;
            cont.SetParent(inMaze ? GetContainer(ContainerNamesMazor.Maze) : null);
            if (isMaze)
            {
                cont.SetParent(GetContainer(ContainerNamesMazor.MazeHolder));
                cont.SetLocalPosXY(Vector2.zero);
            }
            m_Initialized[_Name] = true;
            m_Containers[_Name] = cont;
            return m_Containers[_Name];
        }

        #endregion

        #region nonpublic members

        protected abstract void UpdateCoordinateConverterState();

        #endregion
    }
}