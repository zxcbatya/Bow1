using UnityEngine;
using Enemies;

namespace Combat
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Arrow : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float damage = 20f;
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private GameObject hitEffect;
        [SerializeField] private TrailRenderer trail;
        
        private bool _isEnemyArrow;
        private Rigidbody _rb;
        private Collider _collider;
        private bool _hasHit = false;
        
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            if (_rb == null)
            {
                _rb = gameObject.AddComponent<Rigidbody>();
                Debug.Log("Adding Rigidbody to Arrow in Awake");
            }
            
            _collider = GetComponent<Collider>();
            if (_collider == null)
            {
                _collider = gameObject.AddComponent<CapsuleCollider>();
                ((CapsuleCollider)_collider).radius = 0.1f;
                ((CapsuleCollider)_collider).height = 0.5f;
                ((CapsuleCollider)_collider).direction = 2; // Z-axis (forward)
                Debug.Log("Adding Collider to Arrow in Awake");
            }
            
            if (trail == null)
            {
                trail = GetComponent<TrailRenderer>();
                if (trail == null)
                {
                    trail = gameObject.AddComponent<TrailRenderer>();
                    SetupTrail();
                    Debug.Log("Adding TrailRenderer to Arrow in Awake");
                }
            }
            
            // Настройка Rigidbody для стрелы
            _rb.useGravity = true;
            _rb.detectCollisions = true;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            
            // Настройка коллайдера, чтобы он не был триггером
            _collider.isTrigger = false;
        }
        
        private void SetupTrail()
        {
            if (trail != null)
            {
                trail.startWidth = 0.1f;
                trail.endWidth = 0.01f;
                trail.time = 0.5f;
                trail.material = new Material(Shader.Find("Sprites/Default"));
                trail.startColor = Color.white;
                trail.endColor = new Color(1, 1, 1, 0);
            }
        }
        
        public void Initialize(bool isEnemy, Transform targetTransform = null, float chargePower = 1f)
        {
            _isEnemyArrow = isEnemy;
            _hasHit = false;
            
            // Повторная проверка компонентов
            if (_rb == null) _rb = GetComponent<Rigidbody>();
            if (_rb == null) _rb = gameObject.AddComponent<Rigidbody>();
            
            if (_collider == null) _collider = GetComponent<Collider>();
            if (_collider == null) 
            {
                _collider = gameObject.AddComponent<CapsuleCollider>();
                ((CapsuleCollider)_collider).radius = 0.1f;
                ((CapsuleCollider)_collider).height = 0.5f;
                ((CapsuleCollider)_collider).direction = 2;
            }
            
            // Устанавливаем урон в зависимости от силы натяжения
            damage *= chargePower;
            
            // Устанавливаем теги
            gameObject.tag = _isEnemyArrow ? "EnemyArrow" : "PlayerArrow";
            
            // Уничтожаем стрелу через lifetime секунд
            Destroy(gameObject, lifetime);
        }
        
        private void Update()
        {
            // Если стрела двигается, ориентируем ее по направлению движения
            if (!_hasHit && _rb != null && _rb.linearVelocity.sqrMagnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(_rb.linearVelocity);
            }
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (_hasHit) return;
            _hasHit = true;
            
            Debug.Log($"Arrow hit: {collision.gameObject.name} with tag: {collision.gameObject.tag}");
            
            // Останавливаем движение стрелы
            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _rb.isKinematic = true;
            }
            
            if (_isEnemyArrow)
            {
                // Если это вражеская стрела и она попала в игрока
                if (collision.gameObject.CompareTag("Player"))
                {
                    PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(damage);
                    }
                    
                    Debug.Log("Enemy arrow hit player!");
                }
            }
            else
            {
                // Если это стрела игрока и она попала в скелета
                EnemySkeleton skeleton = collision.gameObject.GetComponent<EnemySkeleton>();
                if (skeleton != null)
                {
                    skeleton.TakeDamage();
                    Debug.Log("Player arrow hit skeleton directly!");
                }
                else
                {
                    // Проверяем, есть ли скелет в родителе объекта
                    Transform parent = collision.transform.parent;
                    if (parent != null)
                    {
                        skeleton = parent.GetComponent<EnemySkeleton>();
                        if (skeleton != null)
                        {
                            skeleton.TakeDamage();
                            Debug.Log("Player arrow hit skeleton through parent!");
                        }
                    }
                }
            }
            
            // Создаём эффект попадания
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            
            // Уничтожаем стрелу
            Destroy(gameObject, 0.2f); // Небольшая задержка для визуального эффекта
        }
    }
} 