using System;

namespace Combat.Interfaces
{
    /// <summary>
    /// Интерфейс для взаимодействия с системой здоровья игрока
    /// </summary>
    public interface IPlayerHealth
    {
        /// <summary>
        /// Нанести урон игроку
        /// </summary>
        void TakeDamage(float damage);
        
        /// <summary>
        /// Восстановить здоровье игрока
        /// </summary>
        void Heal(float amount);
        
        /// <summary>
        /// Получить текущее здоровье игрока
        /// </summary>
        float GetCurrentHealth();
        
        /// <summary>
        /// Получить максимальное здоровье игрока
        /// </summary>
        float GetMaxHealth();
        
        /// <summary>
        /// Событие изменения здоровья
        /// </summary>
        event Action<float> OnHealthChanged;
        
        /// <summary>
        /// Событие смерти игрока
        /// </summary>
        event Action OnPlayerDeath;
    }
}