using Enemies;
using UnityEngine;
using Enemies.Enemies.Components;

namespace World
{
    public class WorldController : MonoBehaviour 
    {
        [Header("Настройки")]
        [SerializeField] private WorldSettings _settings;
        [SerializeField] private GameObject _blockPrefab;
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private GameObject _arrowPrefab; 
        [SerializeField] private Transform _player;
        
        [Header("Генератор")]
        [SerializeField] private SpiralWorldGenerator _worldGenerator;
        
        private IPoolManager _poolManager;
        private IEnemySpawner _enemySpawner;
        private float _nextSpawnCheckTime;

        private void Awake() 
        {
            _poolManager = new PoolManager(
                _blockPrefab,
                _enemyPrefab,
                _arrowPrefab, 
                _settings.maxBlocksInMemory 
            );

            _worldGenerator.Initialize(_poolManager, _settings);
            _worldGenerator.GenerateInitial();

            _enemySpawner = GetComponent<EnemySpawner>();
            _enemySpawner.Initialize(_poolManager, _settings);
            var enemy = _poolManager.GetEnemy();
            if (enemy.TryGetComponent<EnemySkeleton>(out var skeleton))
            {
                skeleton.Initialize(_poolManager, _player); // Передаём игрока
                skeleton.GetComponent<EnemyStateController>().ResetEnemy();
            }
        }

        private void Update()
        {
            UpdateGeneration();
            UpdateEnemySpawning();
            CleanupOldBlocks();
        }

        private void UpdateGeneration()
        {
            if (ShouldGenerateMoreBlocks())
            {
                _worldGenerator.GenerateNextSection();
            }
        }

        private bool ShouldGenerateMoreBlocks()
        {
            return _poolManager.ActiveBlocksCount < _settings.maxBlocksInMemory;
        }

        private void UpdateEnemySpawning()
        {
            if (Time.time > _nextSpawnCheckTime)
            {
                TrySpawnEnemyGroup();
                _nextSpawnCheckTime = Time.time + Random.Range(5f, 10f);
            }
        }

        private void TrySpawnEnemyGroup()
        {
            for (int i = 0; i < 3; i++)
            {
                var spawnPos = GetRandomSpawnPosition();
                if (spawnPos != Vector3.zero)
                {
                    _enemySpawner.TrySpawnEnemy(spawnPos);
                }
            }
        }

        private Vector3 GetRandomSpawnPosition()
        {
            var angle = Random.Range(0f, Mathf.PI * 2f);
            return new Vector3(
                Mathf.Cos(angle) * _settings.spiralRadius,
                _player.position.y,
                Mathf.Sin(angle) * _settings.spiralRadius
            );
        }

        private void CleanupOldBlocks()
        {
            _worldGenerator.CleanupOldBlocks(_player.position, _settings.cleanupDistance);
        }
    }
}