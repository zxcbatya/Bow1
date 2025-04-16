using UnityEngine;

namespace World
{
    [CreateAssetMenu(menuName = "World/GenerationSettings")]
    public class WorldSettings : ScriptableObject {
        [Header("Spiral Settings")]
        public float spiralRadius = 12f;
        public float blockSpacing = 1.2f;
        public float heightStep = 0.4f;
        public int initialBlocks = 50;
    
        [Header("Enemy Settings")]
        public float enemySpawnRadius = 25f;
        public float minEnemyDistance = 15f;
    
        [Header("Performance")]
        public int maxBlocksInMemory = 3000;
        public float cleanupDistance = 150f;
        
        [Header("Terrain Settings")]
        public LayerMask blockLayerMask ;
    }
}