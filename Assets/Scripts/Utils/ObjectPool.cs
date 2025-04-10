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
            
        }
        
        private GameObject CreateNewObject()
        {
            if (_prefab == null) return null;
            
            GameObject obj = Object.Instantiate(_prefab);
            if (_parent != null)
            {
                obj.transform.SetParent(_parent);
            }
            
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
                return null;
            }
            
            // Очищаем недействительные элементы
            _pool.RemoveAll(item => item == null);
            
            if (_pool.Count == 0)
            {
                // Если пул пуст, создаем новый объект
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
                obj = CreateNewObject();
                if (obj == null) return null;
            }
            
            obj.SetActive(true);
            _activeObjects.Add(obj);
            
            return obj;
        }
        
        /// <summary>
        /// Вернуть объект в пул
        /// </summary>
        public void ReturnObject(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }
            
            // Проверяем, является ли объект частью этого пула
            if (!_activeObjects.Contains(obj))
            {
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
            }
        }
        
        /// <summary>
        /// Возвращает все активные объекты в пул
        /// </summary>
        public void ReturnAllObjects()
        {
            
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
                }
            }
            
            _activeObjects.Clear();
            
            int removed = _pool.RemoveAll(item => item == null);
            if (removed > 0)
            {
                Debug.LogWarning($"{_poolName}: Removed {removed} null objects from pool");
            }
            
        }
        
        /// <summary>
        /// Увеличить размер пула
        /// </summary>
        public void ExpandPool(int additionalSize)
        {
            if (additionalSize <= 0) return;
            
            
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
        }
    }
} 