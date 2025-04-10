using UnityEngine;

namespace World
{
    /// <summary>
    /// Управление сложностью на основе прогресса
    /// </summary>
    public class DifficultyManager
    {
        private readonly float _baseJumpDistance;
        private readonly float _maxJumpDistance;
        private readonly float _jumpDistanceIncrease;
        
        public float CurrentDifficulty { get; private set; }

        public DifficultyManager(float baseJumpDistance, float maxJumpDistance, float jumpDistanceIncrease)
        {
            _baseJumpDistance = baseJumpDistance;
            _maxJumpDistance = maxJumpDistance;
            _jumpDistanceIncrease = jumpDistanceIncrease;
            CurrentDifficulty = baseJumpDistance;
        }

        public void UpdateDifficulty(int level)
        {
            // Уровень сложности может быть основан на пройденных блоках или высоте
            float difficultyBonus = level * 1.0f; // +1 к дистанции за каждые 100 блоков или 10 единиц высоты
            
            CurrentDifficulty = Mathf.Min(
                _maxJumpDistance + difficultyBonus,
                _baseJumpDistance + difficultyBonus
            );
        }
    }
}