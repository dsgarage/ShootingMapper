using UnityEngine;

namespace ShmupCreator.Runtime.Data
{
    [CreateAssetMenu(fileName = "NewBulletPattern", menuName = "Shmup Creator/Bullet Pattern")]
    public class ShmupBulletPatternData : ScriptableObject
    {
        [Header("Pattern")]
        public BulletSpreadType spreadType = BulletSpreadType.Fan;
        public int bulletCount = 3;
        public float spreadAngle = 30f;
        public float angleOffset;

        [Header("Bullet Properties")]
        public float speed = 5f;
        public float acceleration;
        public Sprite sprite;
        public float size = 1f;
        public bool homing;
        public float homingStrength = 1f;

        [Header("Sub Weapon")]
        public ShmupBulletPatternData subPattern;
        public float subPatternDelay;
    }

    public enum BulletSpreadType
    {
        Fan,
        Circle,
        Line,
        Random
    }
}
