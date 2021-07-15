﻿using UnityEngine;

namespace Games.RazorMaze
{
    [CreateAssetMenu(fileName = "view_settings", menuName = "View Settings", order = 1)]
    public class ViewSettings : ScriptableObject
    {
        [SerializeField] private float lineWidth;
        [SerializeField] private float cornerRadius;
        [SerializeField] private float movingTrapRotationSpeed;
        [SerializeField] private float shredingerLineOffsetSpeed;
        [SerializeField] private float turretBulletRotationSpeed;

        public float LineWidth => lineWidth * 0.01f;
        public float CornerRadius => cornerRadius * 0.01f;
        public float MovingTrapRotationSpeed => -movingTrapRotationSpeed;
        public float ShredingerLineOffsetSpeed => shredingerLineOffsetSpeed * 0.01f;
        public float TurretBulletRotationSpeed => turretBulletRotationSpeed * -10f;
    }
}