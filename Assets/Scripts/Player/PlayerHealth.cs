using System;
using Combat.Interfaces;
using UnityEngine;
using UI;

namespace Player
{
    /// <summary>
    /// Компонент здоровья игрока
    /// </summary>
    public class PlayerHealth : MonoBehaviour, IPlayerHealth
    {
        [Header("Здоровье")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _currentHealth = 100f;
        [SerializeField] private float _regenerationRate = 0f; // Регенерация здоровья в секунду
        
        [Header("Защита")]
        [SerializeField] private float _damageReduction = 0f; // Уменьшение урона (0-1)
        
        [Header("Эффекты получения урона")]
        [SerializeField] private float _invulnerabilityTime = 0.5f; // Время неуязвимости после получения урона
        [SerializeField] private GameObject _damageEffect; // Эффект при получении урона
        [SerializeField] private AudioClip _damageSound; // Звук при получении урона
        
        [Header("UI связи")]
        [SerializeField] private HealthBar _healthBar;
        
        // События
        public event Action<float> OnHealthChanged;
        public event Action OnPlayerDeath;
        
        private float _lastDamageTime;
        private AudioSource _audioSource;
        private bool _isDead = false;
        
        private void Awake()
        {
            _currentHealth = _maxHealth;
            _audioSource = GetComponent<AudioSource>();
            
            if (_audioSource == null && _damageSound != null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        private void Start()
        {
            UpdateHealthBar();
        }
        
        private void Update()
        {
            // Регенерация здоровья если включена
            if (_regenerationRate > 0 && _currentHealth < _maxHealth)
            {
                _currentHealth = Mathf.Min(_maxHealth, _currentHealth + _regenerationRate * Time.deltaTime);
                UpdateHealthBar();
            }
        }
        
        /// <summary>
        /// Получение урона от внешнего источника
        /// </summary>
        public void TakeDamage(float damage)
        {
            // Если игрок мертв или в неуязвимости - игнорируем
            if (_isDead || Time.time - _lastDamageTime < _invulnerabilityTime)
            {
                return;
            }
            
            // Применяем защиту
            float actualDamage = damage * (1f - _damageReduction);
            
            // Вычитаем здоровье
            _currentHealth = Mathf.Max(0, _currentHealth - actualDamage);
            _lastDamageTime = Time.time;
            
            // Проигрываем эффекты получения урона
            PlayDamageEffects();
            
            // Уведомляем интерфейс
            UpdateHealthBar();
            OnHealthChanged?.Invoke(_currentHealth);
            
            // Проверяем смерть
            if (_currentHealth <= 0 && !_isDead)
            {
                Die();
            }
            
            Debug.Log($"Игрок получил {actualDamage} урона. Оставшееся здоровье: {_currentHealth}");
        }
        
        /// <summary>
        /// Восстановление здоровья
        /// </summary>
        public void Heal(float amount)
        {
            if (_isDead) return;
            
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            UpdateHealthBar();
            OnHealthChanged?.Invoke(_currentHealth);
            
            Debug.Log($"Игрок восстановил {amount} здоровья. Текущее здоровье: {_currentHealth}");
        }
        
        /// <summary>
        /// Обработка смерти игрока
        /// </summary>
        private void Die()
        {
            _isDead = true;
            Debug.Log("Игрок погиб!");
            
            // Вызываем событие смерти для внешних слушателей
            OnPlayerDeath?.Invoke();
            
            // Можно добавить эффекты смерти, анимацию и т.д.
            
            // Перезапускаем игрока через 3 секунды
            Invoke(nameof(Respawn), 3f);
        }
        
        /// <summary>
        /// Возрождение игрока
        /// </summary>
        private void Respawn()
        {
            _isDead = false;
            _currentHealth = _maxHealth;
            UpdateHealthBar();
            
            // Можно добавить телепортацию на начальную позицию и т.д.
            
            Debug.Log("Игрок возрожден!");
        }
        
        /// <summary>
        /// Проигрывание эффектов получения урона
        /// </summary>
        private void PlayDamageEffects()
        {
            // Звуковой эффект
            if (_audioSource != null && _damageSound != null)
            {
                _audioSource.PlayOneShot(_damageSound);
            }
            
            // Визуальный эффект
            if (_damageEffect != null)
            {
                _damageEffect.SetActive(true);
                Invoke(nameof(DisableDamageEffect), 0.2f);
            }
        }
        
        private void DisableDamageEffect()
        {
            if (_damageEffect != null)
            {
                _damageEffect.SetActive(false);
            }
        }
        
        /// <summary>
        /// Обновление полоски здоровья в интерфейсе
        /// </summary>
        private void UpdateHealthBar()
        {
            if (_healthBar != null)
            {
                _healthBar.SetHealth(_currentHealth, _maxHealth);
            }
        }
        
        /// <summary>
        /// Получить текущее здоровье
        /// </summary>
        public float GetCurrentHealth()
        {
            return _currentHealth;
        }
        
        /// <summary>
        /// Получить максимальное здоровье
        /// </summary>
        public float GetMaxHealth()
        {
            return _maxHealth;
        }
    }
} 