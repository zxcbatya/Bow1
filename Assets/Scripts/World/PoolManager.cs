using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace World
{
    public class PoolManager : IPoolManager
    {
        private AdvancedObjectPool _blockPool;
        private AdvancedObjectPool _enemyPool;
        private AdvancedObjectPool _arrowPool;

        public int ActiveBlocksCount => _blockPool.ActiveCount;

        public PoolManager(WorldSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
    
            _blockPool = new AdvancedObjectPool(
                settings.blockPrefab, 
                settings.maxBlocksInMemory, 
                "Block"
            );
    
            _enemyPool = new AdvancedObjectPool(
                settings.enemyPrefab,
                settings.maxEnemiesAtOnce * 2,
                "Enemy"
            );
    
            _arrowPool = new AdvancedObjectPool(
                settings.arrowPrefab,
                100,
                "Arrow"
            );
        }
        public GameObject GetBlock() => _blockPool.GetObject();
        public void ReturnBlock(GameObject obj) => _blockPool.ReturnObject(obj);

        public GameObject GetEnemy() => _enemyPool.GetObject();
        public void ReturnEnemy(GameObject obj) => _enemyPool.ReturnObject(obj);

        public GameObject GetEnemyProjectile() => _arrowPool.GetObject();
        public void ReturnEnemyProjectile(GameObject obj) => _arrowPool.ReturnObject(obj);
    }

    public class AdvancedObjectPool
    {
        private GameObject _prefab;
        private int _maxSize;
        private string _name;
        private Transform _poolContainer;
        
        // Используем HashSet для быстрой проверки, активен ли объект
        private HashSet<GameObject> _activeObjects = new HashSet<GameObject>();
        private Queue<GameObject> _inactiveObjects = new Queue<GameObject>();
        
        public int ActiveCount => _activeObjects.Count;
        
        public AdvancedObjectPool(GameObject prefab, int maxSize, string name)
        {
            _prefab = prefab;
            _maxSize = maxSize;
            _name = name;
            
            // Создаем контейнер для объектов пула
            GameObject container = new GameObject($"{name}Pool");
            _poolContainer = container.transform;
            Object.DontDestroyOnLoad(container);
            
            // Предварительно создаем объекты (до 20% от максимума)
            int preWarmCount = Mathf.Min(maxSize, 20);
            for (int i = 0; i < preWarmCount; i++)
            {
                GameObject obj = CreateNewInstance();
                _inactiveObjects.Enqueue(obj);
            }
        }
        
        private GameObject CreateNewInstance()
        {
            GameObject instance = Object.Instantiate(_prefab, _poolContainer);
            instance.name = $"{_name}_{_activeObjects.Count + _inactiveObjects.Count}";
            instance.SetActive(false);
            return instance;
        }
        
        public GameObject GetObject()
        {
            GameObject obj;
            
            if (_inactiveObjects.Count > 0)
            {
                obj = _inactiveObjects.Dequeue();
            }
            else if (_activeObjects.Count < _maxSize)
            {
                obj = CreateNewInstance();
            }
            else
            {
                Debug.LogWarning($"Pool {_name} is at maximum capacity ({_maxSize})!");
                return null;
            }
            
            obj.SetActive(true);
            _activeObjects.Add(obj);
            return obj;
        }
        
        public void ReturnObject(GameObject obj)
        {
            if (obj == null || !_activeObjects.Contains(obj))
            {
                return;
            }
            
            obj.SetActive(false);
            _activeObjects.Remove(obj);
            
            // Перемещаем объект в контейнер пула
            obj.transform.SetParent(_poolContainer);
            
            // Сбрасываем трансформацию
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            
            _inactiveObjects.Enqueue(obj);
        }
        
        public void ReturnAllObjects()
        {
            List<GameObject> objectsToReturn = new List<GameObject>(_activeObjects);
            foreach (var obj in objectsToReturn)
            {
                ReturnObject(obj);
            }
        }
    }
}