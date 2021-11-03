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
        [SerializeField] private Vector4 screenOffsets;

        public float   LineWidth                  => lineWidth * 0.01f;
        public float   CornerWidth                => cornerWidth * 0.01f;
        public float   CornerRadius               => cornerRadius * 0.01f;
        public float   MovingTrapRotationSpeed    => -movingTrapRotationSpeed;
        public float   ShredingerLineOffsetSpeed  => shredingerLineOffsetSpeed * 0.01f;
        public float   TurretBulletRotationSpeed  => turretBulletRotationSpeed * -10f;
        public int     BlockItemsCount            => blockItemsCount;
        public int     PathItemsCount             => pathItemsCount;
        public bool    StartPathItemFilledOnStart => startPathItemFilledOnStart;
        public float   MazeRotationSpeed          => mazeRotationSpeed;
        public float   FinishTimeExcellent        => finishTimeExcellent * 0.1f;
        public float   FinishTimeGood             => finishTimeGood * 0.1f;
        public float   ProposalDialogAnimSpeed    => proposalDialogAnimSpeed;
        public Vector4 ScreenOffsets              => screenOffsets;
    }
}