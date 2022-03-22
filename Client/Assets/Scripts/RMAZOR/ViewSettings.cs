using System;
using UnityEngine;

namespace RMAZOR
{
    [Serializable]
    [CreateAssetMenu(fileName = "view_settings", menuName = "Configs and Sets/View Settings", order = 1)]
    public class ViewSettings : ScriptableObject
    {
        public float lineWidth;
        public float cornerWidth;
        public float cornerRadius;
        public float movingTrapRotationSpeed;
        public float shredingerLineOffsetSpeed;
        public float turretProjectileRotationSpeed;
        public int   blockItemsCount;
        public int   pathItemsCount;
        public bool  startPathItemFilledOnStart;
        public float mazeRotationSpeed;
        public float finishTimeExcellent;
        public float finishTimeGood;
        public float gravityTrapRotationSpeed;
        public float leftScreenOffset;
        public float rightScreenOffset;
        public float bottomScreenOffset;
        public float topScreenOffset;
        public float moveSwipeThreshold;
        public bool  springboardAnimatedHighlight;
        public float afterRotationEnableMoveDelay;
        public int   rateRequestsFrequency;
        public int   adsRequestsFrequency;
        public int   levelsCountMain;
        public int   firstLevelToRateGame;
        public float mazeItemTransitionTime;
        public float mazeItemTransitionDelayCoefficient;

        public float LineWidth
        {
            get => lineWidth * 0.01f;
            set => lineWidth = value / 0.01f;
        }

        public float CornerWidth
        {
            get => cornerWidth * 0.01f;
            set => cornerWidth = value / 0.01f;
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

        public float TurretProjectileRotationSpeed
        {
            get => turretProjectileRotationSpeed * -10f;
            set => turretProjectileRotationSpeed = value / -10f;
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
    }
}