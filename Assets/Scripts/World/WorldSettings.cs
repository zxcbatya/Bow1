using UnityEngine;

namespace World
{
    [CreateAssetMenu(menuName = "World/GenerationSettings")]
    public class WorldSettings : ScriptableObject {
        [Header("Spiral Settings")]
        [Range(8f, 20f)] public float spiralRadius = 10f;
        public float blockSpacing = 10f;
        [Range(0.1f, 1f)] public float heightStep = 0.1f;
        [Range(10, 100)] public int initialBlocks = 40;
    
        [Header("Row Settings")]
        [Range(1, 5)] public int rowsCount = 3;
        [Range(0.5f, 3f)] public float rowOffset = 1.2f;
        [Tooltip("Расстояние между основной спиралью и первым рядом")]
        [Range(1f, 5f)] public float initialRowDistance = 1.5f;
        [Tooltip("Расстояние между соседними рядами")]
        [Range(1f, 3f)] public float distanceBetweenRows = 1.5f;
        [Tooltip("Расстояние между блоками в одном ряду")]
        [Range(1f, 3f)] public float blockDistanceInRow = 1.2f;
        [Tooltip("Количество блоков в каждом ряду")]
        [Range(1, 5)] public int blocksInRow = 3;
    
        [Header("Enemy Settings")]
        [Range(10f, 50f)] public float enemySpawnRadius = 20f;
        [Range(5f, 20f)] public float minEnemyDistance = 10f;
        [Range(0.5f, 5f)] public float enemySpawnRate = 2f;
        [Range(1, 10)] public int maxEnemiesAtOnce = 5;
    
        [Header("Performance")]
        [Range(500, 3000)] public int maxBlocksInMemory = 1000;
        [Range(50f, 200f)] public float cleanupDistance = 100f;
        
        [Header("Terrain Settings")]
        public LayerMask blockLayerMask;
        [Range(0.8f, 1.5f)] public float blockScale = 1f;
        [Range(0f, 1f)] public float blockVariation = 0.05f;
        
        [Header("Player Spawn Settings")]
        [Range(1f, 5f)] public float playerSpawnHeight = 3f;

        [Header("Parkour Settings")]
        [Range(1f, 5f)] public float maxJumpDistance = 3f;
        [Range(0f, 1f)] public float parkourDensity = 0.7f;
        [Header("Game Settings")]
        public GameObject blockPrefab;

        public GameObject enemyPrefab;
        public GameObject arrowPrefab;
    }
}