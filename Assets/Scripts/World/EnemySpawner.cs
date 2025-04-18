using Enemies.Enemies.Components;
using UnityEngine;
using System.Collections.Generic;

namespace World
{
    public class EnemySpawner : MonoBehaviour, IEnemySpawner 
    {
        [Header("Spawn Settings")]
        [SerializeField] private float spawnHeight = 3f;
        [SerializeField] private int maxBlocksToTrack = 100; // Ограничиваем количество блоков для отслеживания
        
        [Header("Dependencies")]
        [SerializeField] private WorldSettings _settings;
        [SerializeField] private StructuredSpiralGenerator _worldGenerator;

        private IPoolManager _pool;
        private List<GameObject> _availableBlocks;
        private HashSet<GameObject> _blocksSet; // Используем HashSet для быстрой проверки вхождения

        private void Awake()
        {
            _availableBlocks = new List<GameObject>(maxBlocksToTrack);
            _blocksSet = new HashSet<GameObject>();
        }

        private void OnEnable()
        {
            if (_worldGenerator != null)
            {
                _worldGenerator.OnBlockSpawned += RegisterBlock;
                _worldGenerator.OnBlockRemoved += UnregisterBlock;
            }
        }

        private void OnDisable()
        {
            if (_worldGenerator != null)
            {
                _worldGenerator.OnBlockSpawned -= RegisterBlock;
                _worldGenerator.OnBlockRemoved -= UnregisterBlock;
            }
        }

        public void Initialize(IPoolManager poolManager, WorldSettings settings)
        {
            _pool = poolManager;
            _settings = settings;
        }

        private void RegisterBlock(GameObject block)
        {
            if (block == null) return;
            
            if (!_blocksSet.Contains(block))
            {
                // Если достигли лимита, удаляем самый старый блок
                if (_availableBlocks.Count >= maxBlocksToTrack)
                {
                    var oldestBlock = _availableBlocks[0];
                    _blocksSet.Remove(oldestBlock);
                    _availableBlocks.RemoveAt(0);
                }
                
                _blocksSet.Add(block);
                _availableBlocks.Add(block);
            }
        }

        private void UnregisterBlock(GameObject block)
        {
            if (block == null) return;
            
            _blocksSet.Remove(block);
            _availableBlocks.Remove(block);
        }

        public bool TrySpawnEnemy(Vector3 position)
        {
            if (_availableBlocks.Count == 0) return false;

            // Берем случайные блоки из последней трети списка (самые новые)
            int startIndex = Mathf.Max(0, _availableBlocks.Count - _availableBlocks.Count / 3);
            int randomIndex = Random.Range(startIndex, _availableBlocks.Count);
            
            GameObject targetBlock = _availableBlocks[randomIndex];
            if (targetBlock == null) return false;

            var enemy = _pool.GetEnemy();
            if (enemy == null) return false;

            Vector3 spawnPosition = targetBlock.transform.position + Vector3.up * spawnHeight;
            enemy.transform.position = spawnPosition;
            
            enemy.SetActive(true);
            return true;
        }
        public Vector3? GetSafeSpawnPosition(Vector3 referencePoint)
        {
            if (_availableBlocks.Count == 0 || _settings == null) 
                return null;

            const int maxAttempts = 10;
            for (int i = 0; i < maxAttempts; i++)
            {
                int randomIndex = Random.Range(_availableBlocks.Count / 2, _availableBlocks.Count);
                GameObject block = _availableBlocks[randomIndex];
        
                if (block == null) continue;
        
                float distance = Vector3.Distance(block.transform.position, referencePoint);
                if (distance > _settings.minEnemyDistance && distance < _settings.enemySpawnRadius)
                {
                    return block.transform.position + Vector3.up * spawnHeight;
                }
            }
            return null;
        }

    }
}