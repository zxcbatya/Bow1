using UnityEngine;

namespace World
{
    public interface IPoolManager 
    {
        GameObject GetBlock();
        void ReturnBlock(GameObject block);
        GameObject GetEnemy();
        void ReturnEnemy(GameObject enemy);
        GameObject GetEnemyProjectile();
        void ReturnEnemyProjectile(GameObject projectile);
    
        int ActiveBlocksCount { get; } // Объявление свойства
    }
}