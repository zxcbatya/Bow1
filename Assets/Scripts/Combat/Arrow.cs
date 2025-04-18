using UnityEngine;
using World;
using System.Collections;
using Combat.Interfaces;
using Enemies.Enemies.Components;

namespace Combat
{
    public interface IPoolable
    {
        void ReturnToPool();
    }

    public class Arrow : MonoBehaviour, IPoolable
    {
        [Header("Settings")]
        [SerializeField] private float _damage = 20f;
        [SerializeField] private float _lifetime = 5f;
        [SerializeField] private GameObject _hitEffectPrefab;
        [SerializeField] private TrailRenderer _trail;

        private Rigidbody _rb;
        private Collider _collider;
        private bool _isEnemyArrow;
        private bool _hasHit;
        private IPoolManager _poolManager;

        #region Initialization
        public void Initialize(IPoolManager poolManager, bool isEnemyArrow, float chargePower = 1f)
        {
            _poolManager = poolManager;
            _isEnemyArrow = isEnemyArrow;
            _damage *= chargePower;

            CacheComponents();
            ResetState();
            SetTrailActive(true);

            gameObject.tag = _isEnemyArrow ? "EnemyProjectile" : "PlayerProjectile";
        }
        private void Awake() {
            _rb = GetComponent<Rigidbody>();
        }
        private void CacheComponents()
        {
            if (!_rb) _rb = GetComponent<Rigidbody>();
            if (!_collider) _collider = GetComponent<Collider>();
        }

        private void ResetState()
        {
            _hasHit = false;
            _rb.isKinematic = false;
            _rb.linearVelocity = Vector3.zero;
            _collider.enabled = true;
        }
        #endregion

        #region Physics
        public void Launch(Vector3 direction, float force)
        {
            _rb.AddForce(direction * force, ForceMode.Impulse);
            StartCoroutine(AutoReturnRoutine());
        }

        private void FixedUpdate()
        {
            if (!_hasHit && _rb.linearVelocity.sqrMagnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(_rb.linearVelocity);
            }
        }
        #endregion

        #region Collision Handling
        private void OnCollisionEnter(Collision collision)
        {
            if (_hasHit) return;
            _hasHit = true;

            HandleDamage(collision.gameObject);
            PlayHitEffects();
            StartCoroutine(ReturnToPoolRoutine());
        }

        private void HandleDamage(GameObject target)
        {
            if (_isEnemyArrow)
            {
                var playerHealth = target.GetComponent<IPlayerHealth>();
                playerHealth?.TakeDamage(_damage);
            }
            else
            {
                var enemy = target.GetComponentInParent<EnemyStateController>();
                enemy?.TakeDamage(_damage);
            }
        }
        #endregion

        #region Pooling
        private IEnumerator AutoReturnRoutine()
        {
            yield return new WaitForSeconds(_lifetime);
            ReturnToPool();
        }

        private IEnumerator ReturnToPoolRoutine()
        {
            yield return new WaitForSeconds(0.2f);
            ReturnToPool();
        }

        public void ReturnToPool()
        {
            if (this == null || gameObject == null) return;
            
            SetTrailActive(false);
            
            if (_poolManager == null)
            {
                Debug.LogWarning("Arrow: _poolManager is null, destroying object instead", this);
                Destroy(gameObject);
                return;
            }
            
            _poolManager.ReturnEnemyProjectile(gameObject);
        }

        private void SetTrailActive(bool state)
        {
            if (_trail)
            {
                _trail.Clear();
                _trail.enabled = state;
            }
        }
        #endregion

        #region Effects
        private void PlayHitEffects()
        {
            if (_hitEffectPrefab)
            {
                Instantiate(_hitEffectPrefab, transform.position, Quaternion.identity);
            }
        }
        #endregion
    }
}