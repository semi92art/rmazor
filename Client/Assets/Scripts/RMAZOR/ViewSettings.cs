using System;
using System.Linq;
using UnityEngine;

namespace RMAZOR
{
    [Serializable]
    [CreateAssetMenu(fileName = "view_settings", menuName = "Configs and Sets/View Settings", order = 1)]
    public class ViewSettings : ScriptableObject
    {
        public float  lineThickness;
        public float  cornerRadius;
        public float  movingTrapRotationSpeed;
        public float  shredingerLineOffsetSpeed;
        public float  turretProjectileRotationSpeed;
        public int    blockItemsCount;
        public int    pathItemsCount;
        public bool   collectStartPathItemOnLevelLoaded;
        public float  mazeRotationSpeed;
        public float  finishTimeExcellent;
        public float  finishTimeGood;
        public float  gravityTrapRotationSpeed;
        public float  bottomScreenOffset;
        public float  topScreenOffset;
        public float  moveSwipeThreshold;
        public float  afterRotationEnableMoveDelay;
        public int    levelsCountMain;
        public int    firstLevelToRateGame;
        public int    firstLevelToRateGameThisSession;
        public float  betweenLevelTransitionTime;
        public float  spearProjectileSpeed;
        public float  spearRotationSpeed;
        public string additionalBackTexturesInUse;
        public float  filledPathAlpha;
        public float  additionalBackgroundAlpha;
        public float  skipLevelSeconds;
        public bool   animatePathFill;
        public float  animatePathFillTime;
        public float  cameraSpeed;
        public float  pathItemCharMoveHighlightTime;
        public bool   highlightPathItem;
        public bool   showFullTutorial;
        public string levelsInGroup;
        

        public float LineThickness
        {
            get => lineThickness * 0.01f;
            set => lineThickness = value / 0.01f;
        }

        public float CornerRadius
        {
            get => cornerRadius * 0.01f;
            set => cornerRadius = value / 0.01f;
        }

        public float ShredingerLineOffsetSpeed
        {
            get => shredingerLineOffsetSpeed * 0.01f;
            set => shredingerLineOffsetSpeed = value / 0.01f;
        }

        public float FinishTimeExcellent
        {
            get => finishTimeExcellent * 0.1f;
            set => finishTimeExcellent = value / 0.1f;
        }

        public float FinishTimeGood
        {
            get => finishTimeGood * 0.1f;
            set => finishTimeGood = value / 0.1f;
        }

        public int[] LevelsInGroup
        {
            get => levelsInGroup.Split(',').Select(_V => Convert.ToInt32(_V)).ToArray();
            set
            {
                levelsInGroup = string.Join(",", value.Select(_V => _V.ToString()));
                RmazorUtils.LevelsInGroupArray = value;
            }
        }
    }
}