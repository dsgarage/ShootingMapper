using UnityEngine;

namespace ShmupCreator.Runtime.Data
{
    [CreateAssetMenu(fileName = "NewGameplayData", menuName = "Shmup Creator/Gameplay Data")]
    public class ShmupGameplayData : ScriptableObject
    {
        [Header("Chain System")]
        public bool chainEnabled;
        public float chainTimeWindow = 1f;
        public float chainMultiplierStep = 0.1f;

        [Header("Medal System")]
        public bool medalSystem;
        public int medalBaseScore = 100;
        public float medalScoreMultiplier = 2f;

        [Header("Bullet Cancel")]
        public bool bulletCancel;
        public int cancelBonusPerBullet = 50;

        [Header("Rank System")]
        public bool rankSystem;
        public float rankIncreaseRate = 0.01f;
        public float rankDecreaseOnDeath = 0.5f;
    }
}
