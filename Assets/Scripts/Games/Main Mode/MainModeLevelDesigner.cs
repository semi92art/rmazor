using System.Collections.Generic;
using Extensions;
using Games.Main_Mode.LevelStages;
using Games.Main_Mode.StageBlocks;
using UnityEngine;

namespace Games.Main_Mode
{
    
    
    public class MainModeLevelDesigner : MonoBehaviour, ILevelDesigner
    {
        [HideInInspector] public Level level;
        [HideInInspector] public LevelStageBase levelStage;
        [HideInInspector] public int stageIndex;
        [HideInInspector] public LevelStageType stageType;
        
        public List<StageBlockProps> blockProps;

        public GameObject LevelObject
        {
            get => level.gameObject;
            set
            {
                level.gameObject.DestroySafe();
                level = value.GetComponent<Level>();
            }
        }

        public GameObject StageObject
        {
            get => levelStage.gameObject;
            set
            {
                levelStage.gameObject.DestroySafe();
                levelStage = value.GetComponent<LevelStageBase>();
            }
        }

        public int StageIndex
        {
            get => stageIndex;
            set => stageIndex = value;
        }
    }
}