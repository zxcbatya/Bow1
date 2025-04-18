using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace World
{
    public class StructuredSpiralGenerator : MonoBehaviour, IWorldGenerator
    {
        [SerializeField] private WorldSettings _settings;
        private IPoolManager _pool;
        private Transform _player;
        private List<GameObject> _blocks = new List<GameObject>();
        
        private float _currentAngle;
        private int _currentRowIndex;
        private Vector3 _lastSpiralPosition;

        public Vector3 LastGeneratedPosition { get; private set; }
        public event Action<GameObject> OnBlockSpawned;
        public event Action<GameObject> OnBlockRemoved;

        public void Initialize(IPoolManager poolManager, WorldSettings settings, Transform player)
        {
            _pool = poolManager;
            _settings = settings;
            _player = player;
        }

        public void GenerateInitial()
        {
            for (int i = 0; i < _settings.initialBlocks; i++)
            {
                GenerateNextSection();
            }
            PlacePlayerOnBlock(_blocks[0]);
        }

        public void GenerateNextSection()
        {
            // Генерация основной спирали
            Vector3 spiralPos = CalculateSpiralPosition();
            GameObject mainBlock = SpawnBlock(spiralPos);
            LastGeneratedPosition = spiralPos;

            _currentRowIndex++;
        }

        private Vector3 CalculateSpiralPosition()
        {
            float angle = _currentRowIndex * _settings.blockSpacing / _settings.spiralRadius;
            float height = _currentRowIndex * _settings.heightStep;

            return new Vector3(
                Mathf.Cos(angle) * _settings.spiralRadius,
                height,
                Mathf.Sin(angle) * _settings.spiralRadius
            );
        }

        private void GenerateRows(Vector3 basePosition)
        {
            Vector3 spiralDirection = new Vector3(-basePosition.z, 0, basePosition.x).normalized;
            Vector3 rowDirection = Vector3.Cross(Vector3.up, spiralDirection).normalized;

            for (int row = 0; row < _settings.rowsCount; row++)
            {
                Vector3 rowStart = basePosition + spiralDirection * 
                    (_settings.initialRowDistance + row * _settings.distanceBetweenRows);

                for (int i = 0; i < _settings.blocksInRow; i++)
                {
                    Vector3 pos = rowStart + rowDirection * 
                        (i - _settings.blocksInRow/2) * _settings.blockDistanceInRow;
                    
                    SpawnBlock(pos);
                }
            }
        }

        private GameObject SpawnBlock(Vector3 position)
        {
            GameObject block = _pool.GetBlock();
            if (block == null) return null;

            block.transform.position = position;
            block.transform.localScale = Vector3.one * _settings.blockScale;

            if (_blocks.Count == 0) 
                block.name = "Block_0";

            _blocks.Add(block);
            OnBlockSpawned?.Invoke(block);
            return block;
        }

        private void PlacePlayerOnBlock(GameObject block)
        {
            if (_player == null || block == null) return;

            Collider col = block.GetComponent<Collider>();
            float blockHeight = col != null ? col.bounds.size.y : 1f;
            
            _player.position = block.transform.position + 
                Vector3.up * (_settings.playerSpawnHeight + blockHeight);
        }

        public void CleanupOldBlocks(Vector3 referencePoint, float cleanupDistance)
        {
            List<GameObject> toRemove = new List<GameObject>();

            foreach (var block in _blocks)
            {
                if (block != null && 
                    Vector3.Distance(block.transform.position, referencePoint) > cleanupDistance)
                {
                    OnBlockRemoved?.Invoke(block);
                    _pool.ReturnBlock(block);
                    toRemove.Add(block);
                }
            }

            foreach (var block in toRemove)
            {
                _blocks.Remove(block);
            }
        }

        public List<GameObject> GetActiveBlocks() => new List<GameObject>(_blocks);
    }
}