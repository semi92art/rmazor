using UnityEngine;

namespace Games.RazorMaze
{
    [CreateAssetMenu(fileName = "view_settings", menuName = "View Settings", order = 1)]
    public class ViewSettings : ScriptableObject
    {
        [SerializeField] private float   lineWidth;
        [SerializeField] private float   cornerWidth;
        [SerializeField] private float   cornerRadius;
        [SerializeField] private float   movingTrapRotationSpeed;
        [SerializeField] private float   shredingerLineOffsetSpeed;
        [SerializeField] private float   turretBulletRotationSpeed;
        [SerializeField] private int     blockItemsCount;
        [SerializeField] private int     pathItemsCount;
        [SerializeField] private bool    startPathItemFilledOnStart;
        [SerializeField] private float   mazeRotationSpeed;
        [SerializeField] private float   finishTimeExcellent;
        [SerializeField] private float   finishTimeGood;
        [SerializeField] private float   proposalDialogAnimSpeed;
        [SerializeField] private float   gravityTrapRotationSpeed;
        [SerializeField] private Vector4 screenOffsets;

        public float   LineWidth
        {
            get => lineWidth * 0.01f;
            set => lineWidth = value / 0.01f;
        }

        public float   CornerWidth
        {
            get => cornerWidth * 0.01f;
            set => cornerWidth = value / 0.01f;
        }

        public float   CornerRadius
        {
            get => cornerRadius * 0.01f;
            set => cornerRadius = value / 0.01f;
        }

        public float   MovingTrapRotationSpeed
        {
            get => -movingTrapRotationSpeed;
            set => movingTrapRotationSpeed = -value;
        }

        public float   ShredingerLineOffsetSpeed
        {
            get => shredingerLineOffsetSpeed * 0.01f;
            set => shredingerLineOffsetSpeed = value / 0.01f;
        }

        public float   TurretBulletRotationSpeed
        {
            get => turretBulletRotationSpeed * -10f;
            set => turretBulletRotationSpeed = value / -10f;
        }

        public int     BlockItemsCount
        {
            get => blockItemsCount;
            set => blockItemsCount = value;
        }

        public int     PathItemsCount
        {
            get => pathItemsCount;
            set => pathItemsCount = value;
        }

        public bool    StartPathItemFilledOnStart
        {
            get => startPathItemFilledOnStart;
            set => startPathItemFilledOnStart = value;
        }

        public float   MazeRotationSpeed
        {
            get => mazeRotationSpeed;
            set => mazeRotationSpeed = value;
        }

        public float   FinishTimeExcellent
        {
            get => finishTimeExcellent * 0.1f;
            set => finishTimeExcellent = value / 0.1f;
        }

        public float   FinishTimeGood
        {
            get => finishTimeGood * 0.1f;
            set => finishTimeGood = value / 0.1f;
        }

        public float   ProposalDialogAnimSpeed
        {
            get => proposalDialogAnimSpeed;
            set => proposalDialogAnimSpeed = value;
        }

        public float GravityTrapRotationSpeed
        {
            get => gravityTrapRotationSpeed;
            set => gravityTrapRotationSpeed = value;
        }

        public Vector4 ScreenOffsets
        {
            get => screenOffsets;
            set => screenOffsets = value;
        }
    }
}