using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class ObjectPool {
        private GameObject _prefab;
        private Queue<GameObject> _pool = new Queue<GameObject>();
        private int _activeCount;
        private int _maxSize;

        public ObjectPool(GameObject prefab, int maxSize) {
            _prefab = prefab;
            _maxSize = maxSize;
        }

        public GameObject GetObject() {
            if (_pool.Count > 0) {
                _activeCount++;
                return _pool.Dequeue();
            }
        
            if (_activeCount < _maxSize) {
                _activeCount++;
                return Object.Instantiate(_prefab);
            }
        
            return null;
        }

        public void ReturnObject(GameObject obj) {
            obj.SetActive(false);
            _pool.Enqueue(obj);
            _activeCount--;
        }

        public int ActiveCount => _activeCount;
    }
}