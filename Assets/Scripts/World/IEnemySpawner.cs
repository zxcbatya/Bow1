using UnityEngine;

namespace World
{
    public interface IEnemySpawner
    {
        bool TrySpawnEnemy(Vector3 position);
        void Initialize(IPoolManager poolManager, WorldSettings settings);
    }
}