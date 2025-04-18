using UnityEngine;
using System.Collections;
using System.Linq;

namespace World
{
    public class WorldController : MonoBehaviour 
    {
        [Header("Settings")]
        [SerializeField] private WorldSettings _settings;
        [SerializeField] private Transform _player;
        
        [Header("Components")]
        [SerializeField] private StructuredSpiralGenerator _worldGenerator;
        [SerializeField] private EnemySpawner _enemySpawner;
        
        private IPoolManager _poolManager;
        private Vector3 _lastSafePosition;
        private float _nextSpawnCheckTime;
        private int _activeEnemiesCount;
        
        private void ValidateReferences()
        {
            if (_settings == null || _player == null || _worldGenerator == null)
            {
                Debug.LogError("Essential references missing!", gameObject);
                enabled = false;
            }
        }

        private IEnumerator InitializeSystemsCoroutine()
        {
            _poolManager = new PoolManager(_settings);
            _worldGenerator.Initialize(_poolManager, _settings, _player);
            _worldGenerator.GenerateInitial();
    
            yield return null; // Ждем завершения одного кадра
    
            PlacePlayerImmediately();
    
            if (_enemySpawner != null)
            {
                _enemySpawner.Initialize(_poolManager, _settings);
                StartCoroutine(SpawnEnemiesRoutine());
            }
        }

        private void Awake()
        {
            ValidateReferences();

            StartCoroutine(InitializeSystemsCoroutine());
        }

        private void PlacePlayerImmediately()
        {
            var startBlock = _worldGenerator.GetActiveBlocks().FirstOrDefault();
            if (startBlock != null)
            {
                _player.position = GetBlockSurfacePosition(startBlock);
                _lastSafePosition = _player.position;
            }
            else
            {
                Debug.LogError("Failed to find initial block!");
            }
        }

        private Vector3 GetBlockSurfacePosition(GameObject block)
        {
            var collider = block.GetComponent<Collider>();
            return collider != null 
                ? collider.bounds.center + Vector3.up * collider.bounds.extents.y
                : block.transform.position + Vector3.up;
        }

        private IEnumerator SpawnEnemiesRoutine()
        {
            yield return new WaitForSeconds(3f);
            while (true)
            {
                if (ShouldSpawnEnemy())
                {
                    TrySpawnEnemy();
                }
                yield return new WaitForSeconds(1f / _settings.enemySpawnRate);
            }
        }

        private void Update()
        {
            UpdateWorldGeneration();
            CheckPlayerSafety();
        }

        private void UpdateWorldGeneration()
        {
            if (Vector3.Distance(_player.position, _worldGenerator.LastGeneratedPosition) > 20f)
            {
                _worldGenerator.GenerateNextSection();
            }
        }

        private void CheckPlayerSafety()
        {
            if (_player.position.y < -50f)
            {
                RespawnPlayer();
                return;
            }

            if (Time.time % 2f < Time.deltaTime)
            {
                UpdateSafePosition();
            }
        }

        private void UpdateSafePosition()
        {
            var nearestBlock = FindNearestBlock(_player.position);
            if (nearestBlock != null)
            {
                _lastSafePosition = GetBlockSurfacePosition(nearestBlock);
            }
        }

        private GameObject FindNearestBlock(Vector3 position)
        {
            return _worldGenerator.GetActiveBlocks()
                .OrderBy(b => Vector3.Distance(b.transform.position, position))
                .FirstOrDefault();
        }

        private void RespawnPlayer()
        {
            _player.position = _lastSafePosition;
            var rb = _player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        private bool ShouldSpawnEnemy()
        {
            return _activeEnemiesCount < _settings.maxEnemiesAtOnce 
                && _enemySpawner != null 
                && _poolManager != null;
        }

        private void TrySpawnEnemy()
        {
            var spawnPos = _enemySpawner.GetSafeSpawnPosition(_player.position);
            if (spawnPos.HasValue && _enemySpawner.TrySpawnEnemy(spawnPos.Value))
            {
                _activeEnemiesCount++;
            }
        }

        public void OnEnemyDestroyed()
        {
            _activeEnemiesCount = Mathf.Max(0, _activeEnemiesCount - 1);
        }

        private void CleanupOldBlocks()
        {
            _worldGenerator.CleanupOldBlocks(_player.position, _settings.cleanupDistance);
        }
    }
}