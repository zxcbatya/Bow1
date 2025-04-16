using UnityEngine;

namespace World
{
    public interface IWorldGenerator {
        void GenerateInitial();
        void GenerateNextSection();
        void CleanupOldBlocks(Vector3 referencePoint, float cleanupDistance);
    }
}