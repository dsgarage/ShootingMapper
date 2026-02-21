using System.Collections.Generic;
using UnityEngine;

namespace ShmupCreator.Runtime.Data
{
    [CreateAssetMenu(fileName = "NewPlayerData", menuName = "Shmup Creator/Player Data")]
    public class ShmupPlayerData : ScriptableObject
    {
        [Header("Appearance")]
        public Sprite sprite;

        [Header("Movement")]
        public float speed = 5f;

        [Header("Combat")]
        public List<ShmupWeaponData> weaponSets = new List<ShmupWeaponData>();
        public ShmupWeaponData bombWeapon;
        public float hitboxRadius = 0.1f;
    }
}
