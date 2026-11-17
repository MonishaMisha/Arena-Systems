using System;
using UnityEngine;

namespace PortfolioShowcase.Data
{
    [Serializable]
    public class WaveEntry
    {
        public EnemyData enemyData;
        public GameObject enemyPrefab;
        public int count = 5;
        public float spawnInterval = 0.5f;
    }

    [CreateAssetMenu(fileName = "WaveData", menuName = "Showcase/Wave Data")]
    public class WaveData : ScriptableObject
    {
        public WaveEntry[] entries;
    }
}
