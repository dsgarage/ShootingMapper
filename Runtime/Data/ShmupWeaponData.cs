using System.Collections.Generic;
using UnityEngine;

namespace ShmupCreator.Runtime.Data
{
    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "Shmup Creator/Weapon Data")]
    public class ShmupWeaponData : ScriptableObject
    {
        [Header("Firing")]
        public float fireRate = 0.2f;
        public AudioClip fireSound;

        [Header("Bullet Patterns")]
        public List<ShmupBulletPatternData> bulletPatterns = new List<ShmupBulletPatternData>();
    }
}
