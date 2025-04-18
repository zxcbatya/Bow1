using UnityEngine;
using TMPro;
using Player;
using Combat.Interfaces;

namespace UI
{
    public class GameUI : BaseUIElement
    {
        [Header("UI элементы")]
        [SerializeField] private HealthBar _healthBar;
        [SerializeField] private TextMeshProUGUI _scoreDisplay;
        [SerializeField] private GameObject _crosshair;
        
        [Header("Дополнительные панели")]
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private GameObject _hurtOverlay;
        
        private int _score = 0;
        private PlayerHealth _playerHealth;
        
        public override void Initialize()
        {
            // Инициализируем вложенные элементы UI
            if (_healthBar != null)
            {
                _healthBar.Initialize();
            }
            else
            {
                Debug.LogWarning("HealthBar не назначен в GameUI!", this);
            }
            
            // Скрываем панель Game Over
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(false);
            }
            
            // Скрываем эффект получения урона
            if (_hurtOverlay != null)
            {
                _hurtOverlay.SetActive(false);
            }
            
            // Пытаемся найти компонент здоровья игрока
            FindPlayerHealth();
            
            // Устанавливаем начальное значение счета
            UpdateScoreDisplay();
        }
        
        private void FindPlayerHealth()
        {
            // Пытаемся найти игрока
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerHealth = player.GetComponent<PlayerHealth>();
                if (_playerHealth != null)
                {
                    // Подписываемся на события изменения здоровья и смерти
                    _playerHealth.OnHealthChanged += OnPlayerHealthChanged;
                    _playerHealth.OnPlayerDeath += OnPlayerDeath;
                    
                    // Обновляем UI здоровья
                    if (_healthBar != null)
                    {
                        _healthBar.SetHealth(_playerHealth.GetCurrentHealth(), _playerHealth.GetMaxHealth());
                    }
                }
                else
                {
                    Debug.LogWarning("Компонент PlayerHealth не найден на игроке!", this);
                }
            }
            else
            {
                Debug.LogWarning("Игрок не найден на сцене!", this);
            }
        }
        
        private void OnPlayerHealthChanged(float currentHealth)
        {
            // Обновляем полоску здоровья
            if (_healthBar != null)
            {
                _healthBar.SetHealth(currentHealth, _playerHealth.GetMaxHealth());
            }
            
            // Показываем эффект получения урона
            if (_hurtOverlay != null)
            {
                ShowHurtEffect();
            }
        }
        
        private void OnPlayerDeath()
        {
            // Показываем панель Game Over
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
            }
            
            // Скрываем основные элементы интерфейса
            if (_crosshair != null)
            {
                _crosshair.SetActive(false);
            }
        }
        
        /// <summary>
        /// Показывает кратковременный эффект получения урона
        /// </summary>
        private void ShowHurtEffect()
        {
            if (_hurtOverlay == null) return;
            
            _hurtOverlay.SetActive(true);
            
            // Скрываем эффект через короткое время
            CancelInvoke(nameof(HideHurtEffect));
            Invoke(nameof(HideHurtEffect), 0.3f);
        }
        
        private void HideHurtEffect()
        {
            if (_hurtOverlay != null)
            {
                _hurtOverlay.SetActive(false);
            }
        }
        
        /// <summary>
        /// Добавляет очки к текущему счету
        /// </summary>
        public void AddScore(int points)
        {
            _score += points;
            UpdateScoreDisplay();
        }
        
        /// <summary>
        /// Обновляет отображение счета
        /// </summary>
        private void UpdateScoreDisplay()
        {
            if (_scoreDisplay != null)
            {
                _scoreDisplay.text = $"Счет: {_score}";
            }
        }
        
        /// <summary>
        /// Обработчик нажатия кнопки "Начать заново"
        /// </summary>
        public void OnRestartButtonClick()
        {
            // Здесь можно добавить перезапуск уровня
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }
        
        /// <summary>
        /// Обработчик нажатия кнопки "Меню"
        /// </summary>
        public void OnMenuButtonClick()
        {
            // Вызываем соответствующий метод в UIController для показа главного меню
            UIController controller = GetComponentInParent<UIController>();
            if (controller != null)
            {
                controller.ShowMainMenu();
            }
        }
    }
} 