using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace World
{
    public class PoolManager : IPoolManager
    {
        private ObjectPool<GameObject> _blockPool;
        private ObjectPool<GameObject> _enemyPool;
        private ObjectPool<GameObject> _arrowPool;

        public int ActiveBlocksCount => _blockPool.ActiveCount; // Добавляем реализацию свойства

        public PoolManager(
            GameObject blockPrefab,
            GameObject enemyPrefab,
            GameObject arrowPrefab,
            int maxBlocks
        )
        {
            _blockPool = new ObjectPool<GameObject>(
                () => Object.Instantiate(blockPrefab),
                obj => obj.SetActive(true),
                obj => obj.SetActive(false),
                maxBlocks
            );

            _enemyPool = new ObjectPool<GameObject>(
                () => Object.Instantiate(enemyPrefab),
                obj => obj.SetActive(true),
                obj => obj.SetActive(false),
                50 // Максимум врагов
            );

            _arrowPool = new ObjectPool<GameObject>(
                () => Object.Instantiate(arrowPrefab),
                obj => obj.SetActive(true),
                obj => obj.SetActive(false),
                100 // Максимум стрел
            );
        }

        public GameObject GetBlock() => _blockPool.Get();
        public void ReturnBlock(GameObject obj) => _blockPool.Release(obj);

        public GameObject GetEnemy() => _enemyPool.Get();
        public void ReturnEnemy(GameObject obj) => _enemyPool.Release(obj);

        public GameObject GetEnemyProjectile() => _arrowPool.Get();
        public void ReturnEnemyProjectile(GameObject obj) => _arrowPool.Release(obj);
    }

    public class ObjectPool<T> where T : class
    {
        private Queue<T> _pool = new Queue<T>();
        private Func<T> _createFunc;
        private Action<T> _onGet;
        private Action<T> _onRelease;
        private int _maxSize;
        private int _activeCount; // Добавляем счетчик

        public int ActiveCount => _activeCount; // Новое свойство

        public ObjectPool(
            Func<T> createFunc,
            Action<T> onGet = null,
            Action<T> onRelease = null,
            int maxSize = int.MaxValue)
        {
            _createFunc = createFunc;
            _onGet = onGet;
            _onRelease = onRelease;
            _maxSize = maxSize;
        }

        public T Get()
        {
            T item = _pool.Count > 0
                ? _pool.Dequeue()
                : _createFunc();

            _onGet?.Invoke(item);
            _activeCount++; // Увеличиваем счетчик
            return item;
        }

        public void Release(T item)
        {
            if (_pool.Count < _maxSize)
            {
                _onRelease?.Invoke(item);
                _pool.Enqueue(item);
                _activeCount--; // Уменьшаем счетчик
            }
        }
    }
}