using UnityEngine;

namespace ShmupCreator.Runtime.Data
{
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "Shmup Creator/Enemy Data")]
    public class ShmupEnemyData : ScriptableObject
    {
        [Header("Appearance")]
        public Sprite sprite;

        [Header("Stats")]
        public int hp = 1;
        public int scoreValue = 100;

        [Header("Behavior")]
        public ShmupWeaponData weapon;
        public Vector2[] movePath;
        public float moveSpeed = 3f;

        [Header("Effects")]
        public ExplosionData explosion;
    }

    [System.Serializable]
    public class ExplosionData
    {
        public Sprite sprite;
        public float duration = 0.5f;
        public AudioClip sound;
    }
}
