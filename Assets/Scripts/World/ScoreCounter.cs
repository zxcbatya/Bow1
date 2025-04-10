using TMPro;
using UnityEngine;

namespace World
{
    /// <summary>
    /// Компонент для подсчета очков и отображения их в UI
    /// Реализует паттерн MVP (Model-View-Presenter)
    /// </summary>
    public class ScoreCounter : MonoBehaviour
    {
        [Header("Настройки")]
        [SerializeField] private int pointsPerBlock = 1;
        [SerializeField] private bool showDebugLogs = false;
        
        private int _score = 0;
        private ScoreDisplay _display;
        
        private void Awake()
        {
            _display = GetComponent<ScoreDisplay>();
        }
        
        private void OnEnable()
        {
            // Подписываемся на событие прохождения блока
            BlockTrigger.OnBlockPassed += OnBlockPassed;
        }
        
        private void OnDisable()
        {
            // Отписываемся от события
            BlockTrigger.OnBlockPassed -= OnBlockPassed;
        }
        
        private void Start()
        {
            // Сбрасываем счет при старте
            ResetScore();
        }
        
        // Обработчик события прохождения блока
        private void OnBlockPassed(BlockTrigger block)
        {
            AddPoints(pointsPerBlock);
            
            if (showDebugLogs)
            {
                Debug.Log($"Блок пройден! Текущий счет: {_score}");
            }
        }
        
        // Методы для управления счетом (Model)
        public void AddPoints(int points)
        {
            _score += points;
            UpdateScoreUI();
        }
        
        public void ResetScore()
        {
            _score = 0;
            UpdateScoreUI();
        }
        
        // Метод для отображения счета (View)
        private void UpdateScoreUI()
        {
            if (_display != null)
            {
                _display.UpdateDisplay(_score);
            }
        }
        
        // Геттер для получения текущего счета
        public int GetScore()
        {
            return _score;
        }
    }
} 