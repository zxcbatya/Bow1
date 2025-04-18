using UnityEngine;
using System;

namespace World.Score
{
    public class ScoreModel
    {
        private const float MIN_HEIGHT_DIFFERENCE = 0.4f; // Минимальная разница в высоте для начисления очков (равна heightStep из WorldGenerator)
        
        private int _currentScore;
        private float _lastBlockY = float.MinValue;
        
        public event Action<int> OnScoreChanged;
        public int CurrentScore => _currentScore;
        
        public bool TryAddScore(float blockY, int blockRow)
        {
            Debug.Log($"Trying to add score. Current: {_currentScore}, BlockY: {blockY}, LastY: {_lastBlockY}");
            
            // Проверяем, находится ли блок выше предыдущего на минимальную высоту
            if (blockY > _lastBlockY + MIN_HEIGHT_DIFFERENCE)
            {
                _currentScore++;
                Debug.Log($"Score added! New score: {_currentScore}, Height difference: {blockY - _lastBlockY}");
                OnScoreChanged?.Invoke(_currentScore);
                _lastBlockY = blockY;
                return true;
            }
            
            Debug.Log($"Score not added. Height difference too small: {blockY - _lastBlockY} < {MIN_HEIGHT_DIFFERENCE}");
            _lastBlockY = blockY;
            return false;
        }
        
        public void ResetScore()
        {
            _currentScore = 0;
            _lastBlockY = float.MinValue;
            Debug.Log("Score reset");
            OnScoreChanged?.Invoke(_currentScore);
        }
    }
} 