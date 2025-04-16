using UnityEngine;

namespace Enemies.Enemies.Components
{
    public class EnemyStateController : MonoBehaviour
    {
        public event System.Action OnDeath;
        public event System.Action OnReset;
        
        public bool IsActive { get; private set; }

        public void Activate()
        {
            IsActive = true;
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            IsActive = false;
            gameObject.SetActive(false);
        }

        public void TakeDamage(float damage)
        {
            // Логика получения урона
            OnDeath?.Invoke();
        }

        public void ResetEnemy()
        {
            OnReset?.Invoke();
            Activate();
        }
    }
}