using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RMAZOR
{
    [Serializable]
    [CreateAssetMenu(fileName = "view_settings", menuName = "Configs and Sets/View Settings", order = 1)]
    public class ViewSettings : ScriptableObject
    {
        public float  lineThickness;
        public float  pathItemBorderThickness;
        public int    blockItemsCount;
        public int    pathItemsCount;
        public float  mazeRotationSpeed;
        public float  bottomScreenOffset;
        public float  topScreenOffset;
        public float  moveSwipeThreshold;
        public float  afterRotationEnableMoveDelay;
        public int    firstLevelToRateGame;
        public float  betweenLevelTransitionTime;
        public float  spearRotationSpeed;
        public float  skipLevelSeconds;
        public bool   animatePathFill;
        public float  animatePathFillTime;
        public float  cameraSpeed;
        public float  pathItemCharMoveHighlightTime;
        public bool   highlightPathItem;
        public int    finishLevelGroupPanelGetMoneyButtonTextVariant;
        public int    finishLevelGroupPanelBackgroundVariant;
        public string extraBordersIndices;
        public string backgroundTextures;
        public int    additionalBackgroundType;
        public bool   showPathItems;
        public bool   drawAdditionalMazeNet;
        public string pathItemContentShapeType;
        public bool   mazeItemBlockColorEqualsMainColor;
        public string betweenLevelsTransitionTextureName;
        public bool   loadMainGameModeOnStart;
        public float  specialOfferDurationInMinutes;
        
        public float LineThickness
        {
            get => lineThickness * 0.01f;
            set => lineThickness = value * 100f;
        }
        
        public float PathItemBorderThickness
        {
            get => pathItemBorderThickness * 0.01f;
            set => pathItemBorderThickness = value * 100f;
        }

        public List<string> BackgroundTextures
        {
            get => backgroundTextures.Split(',').ToList();
            set => backgroundTextures = string.Join(",", value);
        }
    }
}