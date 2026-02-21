using System.Collections.Generic;
using UnityEngine;

namespace ShmupCreator.Runtime.Data
{
    [CreateAssetMenu(fileName = "NewHUDData", menuName = "Shmup Creator/HUD Data")]
    public class ShmupHUDData : ScriptableObject
    {
        public HUDElementData scoreDisplay;
        public HUDElementData lifeDisplay;
        public List<HUDGaugeData> gauges = new List<HUDGaugeData>();
        public Font font;
    }

    [System.Serializable]
    public class HUDElementData
    {
        public Vector2 position;
        public Vector2 size;
        public Sprite icon;
        public string format = "{0}";
    }

    [System.Serializable]
    public class HUDGaugeData
    {
        public string label;
        public Vector2 position;
        public Vector2 size;
        public Color fillColor = Color.green;
        public Color backgroundColor = Color.gray;
    }
}
