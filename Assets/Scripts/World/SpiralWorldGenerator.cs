using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class SpiralWorldGenerator : MonoBehaviour, IWorldGenerator
    {
        [SerializeField] private WorldSettings _settings;
        [SerializeField] private GameObject _blockPrefab;
        
        private IPoolManager _pool;
    
        private List<GameObject> _blocks = new List<GameObject>();
        private float _currentAngle;
        private float _currentHeight;

        public void Initialize(IPoolManager poolManager, WorldSettings settings)
        {
            _pool = poolManager;
            _settings = settings;
        }
        public void GenerateInitial()
        {
            StartCoroutine(GenerateInitialBlocks());
        }

        private IEnumerator GenerateInitialBlocks()
        {
            for (int i = 0; i < _settings.initialBlocks; i++)
            {
                GenerateNextSection();
                yield return new WaitForSeconds(0.1f);
            }
        }

        public void GenerateNextSection()
        {
            Vector3 position = CalculateSpiralPosition(_blockIndex);
            GameObject block = Instantiate(_blockPrefab, position, Quaternion.identity);
            _blocks.Add(block);
        
            _currentAngle += _settings.blockSpacing / _settings.spiralRadius;
            _currentHeight += _settings.heightStep;
        }

        private Vector3 CalculateSpiralPosition(int blockIndex)
        {
            float baseSpacing = _settings.blockSpacing;
            float dynamicSpacing = baseSpacing + (blockIndex / 100) * 1f; 

            float angle = blockIndex * dynamicSpacing / _settings.spiralRadius;
            float height = blockIndex * _settings.heightStep;

            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * _settings.spiralRadius,
                height,
                Mathf.Sin(angle) * _settings.spiralRadius
            );

            if (Physics.CheckSphere(pos, 0.5f, _blockLayerMask))
            {
                pos += Vector3.up * 1f; 
            }

            return pos;
        }

        public void CleanupOldBlocks(Vector3 referencePoint, float cleanupDistance)
        {
            List<GameObject> toRemove = new List<GameObject>();
        
            foreach (var block in _blocks)
            {
                if (Vector3.Distance(block.transform.position, referencePoint) > cleanupDistance)
                {
                    toRemove.Add(block);
                    Destroy(block);
                }
            }
        
            foreach (var block in toRemove)
            {
                _blocks.Remove(block);
            }
        }
    }
}