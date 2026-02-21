using UnityEngine;

namespace ShmupCreator.Runtime.Data
{
    [CreateAssetMenu(fileName = "NewWaveData", menuName = "Shmup Creator/Wave Data")]
    public class ShmupWaveData : ScriptableObject
    {
        public ShmupEnemyData enemyData;
        public float spawnTime;
        public int count = 1;
        public float spacing = 0.5f;

        [Header("Formation")]
        public FormationType formation = FormationType.Line;
        public Vector2[] formationPath;
    }

    public enum FormationType
    {
        Line,
        V,
        Circle,
        Random,
        Custom
    }
}
