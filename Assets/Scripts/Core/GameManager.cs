using System;
using UnityEngine;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
    
        public event Action OnGameStart;
        public event Action OnGameOver;

        private float Score { get; set; }
        private float HighestPoint { get; set; }
    
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    
        public void StartGame()
        {
            Score = 0;
            HighestPoint = 0;
            OnGameStart?.Invoke();
        }
    
        public void GameOver()
        {
            OnGameOver?.Invoke();
        }
    
        public void UpdateScore(float height)
        {
            if (height > HighestPoint)
            {
                Score += (height - HighestPoint);
                HighestPoint = height;
            }
        }
    }
} 