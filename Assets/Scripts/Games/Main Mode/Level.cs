using System.Collections.Generic;
using System.Linq;
using Extensions;
using UnityEngine;
using Exceptions;
using Games.Main_Mode.LevelStages;
using Games.Main_Mode.StageBlocks;

namespace Games.Main_Mode
{
    public class Level : MonoBehaviour
    {
        private const string ParentName = "Level Content";
        private Transform m_Parent;
        [SerializeField, HideInInspector]
        public List<LevelStageBase> stages = new List<LevelStageBase>();

        

        public void CreateStage(
            LevelStageType _Type,
            int _StageIdx, 
            IEnumerable<StageBlockProps> _BlocksProps)
        {
            float verticalPosition = 0;
            foreach (var st in stages)
            {
                verticalPosition += st.Height + MainModeConstants.GapBetweenStages;
            }
            
            CheckForParentExist();
            LevelStageBase stage;
            switch (_Type)
            {
                case LevelStageType.Circle:
                    stage = LevelStageCircle.Create(m_Parent, _StageIdx, _BlocksProps, verticalPosition);
                    break;
                case LevelStageType.Square:
                    stage = LevelStageSquare.Create(m_Parent, _StageIdx, _BlocksProps, verticalPosition);
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Type);
            }
            stages.Add(stage);
        }

        public void RemoveStage(int _Idx)
        {
            stages[_Idx].gameObject.DestroySafe();
            stages.RemoveAt(_Idx);
        }
        
        public void ClearStages()
        {
            foreach (var stage in stages)
                stage.DestroySafe();
            stages.Clear();
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