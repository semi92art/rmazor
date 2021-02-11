using System.Collections.Generic;
using Extensions;
using UnityEngine;
using Exceptions;
using Games.Main_Mode.LevelStages;

namespace Games.Main_Mode
{
    public class Level : MonoBehaviour
    {
        private const string ParentName = "Level Content";
        private Transform m_Parent;
        [SerializeField, HideInInspector]
        private List<LevelStageBase> stages = new List<LevelStageBase>();

        public void ClearStages()
        {
            foreach (var stage in stages)
                stage.DestroySafe();
            stages.Clear();
        }

        public void CreateStage(LevelStageType _Type, int _StageIdx, int _BlocksCount)
        {
            CheckForParentExist();
            LevelStageBase stage;
            switch (_Type)
            {
                case LevelStageType.Circle:
                    stage = LevelStageCircle.CreateRandom(m_Parent, _StageIdx, _BlocksCount);
                    break;
                case LevelStageType.Square:
                    stage = LevelStageSquare.Create(m_Parent, _StageIdx, _BlocksCount);
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Type);
            }
            stages.Add(stage);
        }

        public void RemoveLastStage()
        {
            
        }

        public void RemoveStage(int _Idx)
        {
            
        }

        private void CheckForParentExist()
        {
            if (!m_Parent.IsNull())
                return;
            var go = GameObject.Find(ParentName);
            if (go == null)
                go = new GameObject(ParentName);
            m_Parent = go.transform;
        }
    }
}