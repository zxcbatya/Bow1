using UnityEngine;
using System.Collections.Generic;

namespace Utils
{
    /// <summary>
    /// Реализация пула объектов для оптимизации производительности
    /// </summary>
    public class ObjectPool
    {
        private GameObject _prefab;
        private int _size;
        private List<GameObject> _pool;
        private List<GameObject> _activeObjects;
        private Transform _parent;
        private string _poolName;
        
        public ObjectPool(GameObject prefab, int size, Transform parent = null)
        {
            if (prefab == null)
            {
                Debug.LogError("Cannot create object pool: prefab is null!");
                return;
            }
            
            _prefab = prefab;
            _size = Mathf.Max(1, size); // Минимум 1 объект
            _parent = parent;
            _poolName = prefab.name + "Pool";
            
            _pool = new List<GameObject>(_size);
            _activeObjects = new List<GameObject>(_size);
            
            Debug.Log($"Creating new {_poolName} with size {_size}");
            InitializePool();
        }
        
        private void InitializePool()
        {
            if (_prefab == null) return;
            
            for (int i = 0; i < _size; i++)
            {
                GameObject obj = CreateNewObject();
                if (obj != null)
                {
                    obj.SetActive(false);
                    _pool.Add(obj);
                }
            }
            
            Debug.Log($"{_poolName} initialized with {_pool.Count} objects");
        }
        
        private GameObject CreateNewObject()
        {
            if (_prefab == null) return null;
            
            GameObject obj = Object.Instantiate(_prefab);
            if (_parent != null)
            {
                obj.transform.SetParent(_parent);
            }
            
            // Добавляем уникальное имя для отслеживания объектов пула
            obj.name = $"{_prefab.name}_Pooled_{_pool.Count + _activeObjects.Count}";
            
            return obj;
        }
        
        /// <summary>
        /// Получить объект из пула. Если пул пуст, будет создан новый объект.
        /// </summary>
        public GameObject GetObject()
        {
            if (_prefab == null)
            {
                Debug.LogError($"{_poolName}: Cannot get object - prefab is null!");
                return null;
            }
            
            // Очищаем недействительные элементы
            _pool.RemoveAll(item => item == null);
            
            if (_pool.Count == 0)
            {
                // Если пул пуст, создаем новый объект
                Debug.Log($"{_poolName}: Pool is empty, creating new object");
                GameObject newObj = CreateNewObject();
                if (newObj != null)
                {
                    newObj.SetActive(true);
                    _activeObjects.Add(newObj);
                    return newObj;
                }
                return null;
            }
            
            // Берем первый объект из пула
            GameObject obj = _pool[0];
            _pool.RemoveAt(0);
            
            if (obj == null)
            {
                // Если объект был уничтожен, создаем новый
                Debug.LogWarning($"{_poolName}: Object in pool was destroyed, creating new one");
                obj = CreateNewObject();
                if (obj == null) return null;
            }
            
            obj.SetActive(true);
            _activeObjects.Add(obj);
            
            Debug.Log($"{_poolName}: Retrieved object {obj.name}, {_pool.Count} left in pool, {_activeObjects.Count} active");
            return obj;
        }
        
        /// <summary>
        /// Вернуть объект в пул
        /// </summary>
        public void ReturnObject(GameObject obj)
        {
            if (obj == null)
            {
                Debug.LogWarning($"{_poolName}: Attempted to return null object to pool");
                return;
            }
            
            // Проверяем, является ли объект частью этого пула
            if (!_activeObjects.Contains(obj))
            {
                Debug.LogWarning($"{_poolName}: Object {obj.name} is not from this pool!");
                return;
            }
            
            // Деактивируем объект
            obj.SetActive(false);
            
            // Сбрасываем родителя, если нужно
            if (_parent != null && obj.transform.parent != _parent)
            {
                obj.transform.SetParent(_parent);
            }
            
            // Удаляем из списка активных
            _activeObjects.Remove(obj);
            
            // Добавляем в пул, если объект не был уничтожен
            if (obj != null)
            {
                _pool.Add(obj);
                Debug.Log($"{_poolName}: Returned object {obj.name} to pool, now {_pool.Count} in pool, {_activeObjects.Count} active");
            }
        }
        
        /// <summary>
        /// Возвращает все активные объекты в пул
        /// </summary>
        public void ReturnAllObjects()
        {
            Debug.Log($"{_poolName}: Returning all {_activeObjects.Count} active objects to pool");
            
            // Создаем копию списка, так как будем модифицировать оригинал в цикле
            List<GameObject> activeObjectsCopy = new List<GameObject>(_activeObjects);
            
            foreach (GameObject obj in activeObjectsCopy)
            {
                if (obj != null)
                {
                    ReturnObject(obj);
                }
                else
                {
                    // Удаляем null объекты из списка активных
                    _activeObjects.Remove(obj);
                    Debug.LogWarning($"{_poolName}: Removed null object from active list");
                }
            }
            
            // На всякий случай очищаем список активных объектов
            _activeObjects.Clear();
            
            // Очищаем недействительные элементы в пуле
            int removed = _pool.RemoveAll(item => item == null);
            if (removed > 0)
            {
                Debug.LogWarning($"{_poolName}: Removed {removed} null objects from pool");
            }
            
            Debug.Log($"{_poolName}: All objects returned, now {_pool.Count} in pool, {_activeObjects.Count} active");
        }
        
        /// <summary>
        /// Увеличить размер пула
        /// </summary>
        public void ExpandPool(int additionalSize)
        {
            if (additionalSize <= 0) return;
            
            Debug.Log($"{_poolName}: Expanding pool by {additionalSize} objects");
            
            for (int i = 0; i < additionalSize; i++)
            {
                GameObject obj = CreateNewObject();
                if (obj != null)
                {
                    obj.SetActive(false);
                    _pool.Add(obj);
                }
            }
            
            _size += additionalSize;
            Debug.Log($"{_poolName}: Pool expanded to {_size} total capacity, {_pool.Count} available");
        }
    }
} 