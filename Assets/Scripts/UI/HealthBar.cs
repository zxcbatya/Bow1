using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Player;

namespace UI
{
    /// <summary>
    /// Компонент для отображения полоски здоровья игрока
    /// </summary>
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private Image _fillImage;
        [SerializeField] private Gradient _healthGradient;
        
        [Header("Анимация")]
        [SerializeField] private float _smoothSpeed = 5f;
        [SerializeField] private bool _useSmoothing = true;
        
        private float _targetHealth = 100f;
        private float _currentDisplayedHealth = 100f;
        
        /// <summary>
        /// Инициализация полоски здоровья
        /// </summary>
        public void Initialize()
        {
            if (_healthSlider == null)
            {
                Debug.LogError("Health Slider не назначен для HealthBar!", this);
                enabled = false;
                return;
            }
            
            // Установка начальных значений
            _targetHealth = 100f;
            _currentDisplayedHealth = 100f;
            UpdateHealthDisplay(_targetHealth);
        }
        
        private void Update()
        {
            // Плавное обновление отображения здоровья
            if (_useSmoothing && Math.Abs(_currentDisplayedHealth - _targetHealth) > 0.01f)
            {
                _currentDisplayedHealth = Mathf.Lerp(_currentDisplayedHealth, _targetHealth, Time.deltaTime * _smoothSpeed);
                UpdateHealthDisplay(_currentDisplayedHealth);
            }
        }
        
        /// <summary>
        /// Устанавливает текущее значение здоровья
        /// </summary>
        /// <param name="currentHealth">Текущее здоровье</param>
        /// <param name="maxHealth">Максимальное здоровье</param>
        public void SetHealth(float currentHealth, float maxHealth)
        {
            if (maxHealth <= 0)
            {
                Debug.LogError("Максимальное здоровье должно быть больше нуля!", this);
                return;
            }
            
            // Нормализуем значение здоровья от 0 до 1
            _targetHealth = Mathf.Clamp01(currentHealth / maxHealth);
            
            // Если анимация отключена - обновляем мгновенно
            if (!_useSmoothing)
            {
                _currentDisplayedHealth = _targetHealth;
                UpdateHealthDisplay(_currentDisplayedHealth);
            }
        }
        
        /// <summary>
        /// Обновляет визуальное отображение здоровья
        /// </summary>
        private void UpdateHealthDisplay(float normalizedHealth)
        {
            if (_healthSlider != null)
            {
                _healthSlider.value = normalizedHealth;
            }
            
            if (_healthText != null)
            {
                _healthText.text = $"{Mathf.Round(normalizedHealth * 100)}%";
            }
            
            if (_fillImage != null && _healthGradient != null)
            {
                _fillImage.color = _healthGradient.Evaluate(normalizedHealth);
            }
        }
    }
} 