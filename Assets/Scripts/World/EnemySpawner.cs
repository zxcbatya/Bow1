using Enemies.Enemies.Components;
using UnityEngine;

namespace World
{
    
    public class EnemySpawner : MonoBehaviour, IEnemySpawner 
    {
        [Header("Collision Settings")]
        [SerializeField] private LayerMask _blockLayerMask;
        [SerializeField] private Vector3 _spawnOffset = new Vector3(0, 1.5f, 0);
        
        [Header("Dependencies")]
        [SerializeField] private WorldSettings _settings;

        private IPoolManager _pool;

        public void Initialize(IPoolManager poolManager, WorldSettings settings)
        {
            _pool = poolManager;
            _settings = settings;
        }

        public bool TrySpawnEnemy(Vector3 position)
        {
            if (!Physics.Raycast(position + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, _blockLayerMask))
            {
                return false;
            }

            var enemy = _pool.GetEnemy();
            if (enemy == null) return false;

            enemy.transform.position = hit.point + Vector3.up * 1f; 
            enemy.SetActive(true);
            return true;
        }

        private bool IsValidSpawnPosition(Vector3 position)
        {
            return !Physics.CheckSphere(position, _settings.minEnemyDistance, _blockLayerMask) 
                   && !HasBlocksAbove(position);
        }

        private bool HasBlocksAbove(Vector3 position)
        {
            return Physics.CheckBox(
                position + Vector3.up * 3f, 
                new Vector3(1f, 2f, 1f), 
                Quaternion.identity, 
                _blockLayerMask
            );
        }

        private void SetupEnemy(GameObject enemy, Vector3 position)
        {
            enemy.transform.position = position + _spawnOffset;
            enemy.SetActive(true);
            
            if (enemy.TryGetComponent<EnemyStateController>(out var controller))
            {
                controller.ResetEnemy();
            }
        }
    }
}