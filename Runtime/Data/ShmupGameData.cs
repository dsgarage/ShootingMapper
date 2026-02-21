using System.Collections.Generic;
using UnityEngine;

namespace ShmupCreator.Runtime.Data
{
    public enum ScrollDirection
    {
        Vertical,
        Horizontal
    }

    [CreateAssetMenu(fileName = "NewGameData", menuName = "Shmup Creator/Game Data")]
    public class ShmupGameData : ScriptableObject
    {
        [Header("Game Info")]
        public string gameName = "New Shmup";
        public Vector2Int resolution = new Vector2Int(1920, 1080);
        public ScrollDirection scrollDirection = ScrollDirection.Vertical;

        [Header("References")]
        public ShmupPlayerData playerData;
        public List<ShmupLevelData> levels = new List<ShmupLevelData>();
    }
}
