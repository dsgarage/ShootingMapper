using System.Collections.Generic;
using UnityEngine;

namespace ShmupCreator.Runtime.Data
{
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "Shmup Creator/Level Data")]
    public class ShmupLevelData : ScriptableObject
    {
        public string levelName = "New Level";
        public float duration = 120f;

        [Header("Timeline")]
        public List<ShmupWaveData> waves = new List<ShmupWaveData>();
        public List<BackgroundEntry> backgrounds = new List<BackgroundEntry>();
        public List<TriggerEntry> triggers = new List<TriggerEntry>();
    }

    [System.Serializable]
    public class BackgroundEntry
    {
        public Sprite sprite;
        public float scrollSpeed = 1f;
        public int layer;
        public bool loop = true;
    }

    [System.Serializable]
    public class TriggerEntry
    {
        public float triggerTime;
        public TriggerType type;
        public string parameter;
    }

    public enum TriggerType
    {
        CameraShake,
        BGMChange,
        WaveActivate,
        BossWarning,
        SpeedChange
    }
}
