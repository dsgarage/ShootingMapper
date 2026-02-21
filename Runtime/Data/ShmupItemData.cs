using UnityEngine;

namespace ShmupCreator.Runtime.Data
{
    public enum ItemType
    {
        PowerUp,
        Bomb,
        Score,
        Life
    }

    [CreateAssetMenu(fileName = "NewItemData", menuName = "Shmup Creator/Item Data")]
    public class ShmupItemData : ScriptableObject
    {
        public ItemType type = ItemType.PowerUp;
        public Sprite sprite;
        public string effect;
    }
}
