using System.Collections;
using PortfolioShowcase.Core.Events;
using PortfolioShowcase.Data;
using UnityEngine;

namespace PortfolioShowcase.Gameplay.Spawning
{
    /// <summary>
    /// Drives waves from data (WaveData assets), publishing WaveStarted /
    /// WaveCompleted so the HUD and any future "game flow" system can react
    /// without this class knowing either of them exist.
    /// </summary>
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] private WaveData[] waves;
        [SerializeField] private Transform[] spawnPoints;

        private int _currentWave = -1;

        private void Start() => StartCoroutine(RunWaves());

        private IEnumerator RunWaves()
        {
            foreach (var wave in waves)
            {
                _currentWave++;
                EventBus.Publish(new WaveStartedEvent(_currentWave));

                foreach (var entry in wave.entries)
                {
                    for (var i = 0; i < entry.count; i++)
                    {
                        SpawnEnemy(entry.enemyPrefab);
                        yield return new WaitForSeconds(entry.spawnInterval);
                    }
                }

                yield return new WaitUntil(AllEnemiesCleared);
                EventBus.Publish(new WaveCompletedEvent(_currentWave));
            }

            EventBus.Publish(new AllWavesCompletedEvent());
        }

        private void SpawnEnemy(GameObject prefab)
        {
            var point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Instantiate(prefab, point.position, point.rotation);
        }

        private bool AllEnemiesCleared() => GameObject.FindGameObjectsWithTag("Enemy").Length == 0;
    }
}
