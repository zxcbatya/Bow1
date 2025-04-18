using System;
using UnityEngine;
using System.Collections;
using Enemies.Enemies.Components;
using World;
using Random = UnityEngine.Random;

namespace Enemies
{
    public class EnemySkeleton : MonoBehaviour
    {
        [Header("Стрельба")]
        [SerializeField] private float _shootInterval = 2f;
        [SerializeField] private float _arrowSpeed = 10f;
        [SerializeField] private float _aimHeight = 1.5f;
        
        [Header("Ссылки")]
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private GameObject _arrowPrefab;

        private IPoolManager _poolManager;
        private Transform _player;
        private EnemyStateController _stateController;
        private float _shootTimer;

        #region Инициализация
        public void Initialize(IPoolManager poolManager, Transform player)
        {
            _poolManager = poolManager;
            _player = player;
            _stateController = GetComponent<EnemyStateController>();
            
            SetupShootPoint();
            ValidateComponents();
            
            _stateController.OnDeath += HandleDeath;
            _stateController.OnReset += ResetEnemy;
        }

        private void Awake()
        {
                Initialize(_poolManager, _player);
        }

        private void Start() 
        {
            if (_player == null)
            {
                _player = GameObject.FindGameObjectWithTag("Player").transform;
            }
        }

        private void SetupShootPoint()
        {
            if (_shootPoint == null)
            {
                _shootPoint = new GameObject("ShootPoint").transform;
                _shootPoint.SetParent(transform);
                _shootPoint.localPosition = new Vector3(0, -1.21f, 0.59f);
            }
        }

        private void ValidateComponents()
        {
            if (_arrowPrefab == null)
                Debug.LogError("Arrow prefab not set!", this);
        }
        #endregion

        #region Update logic
        private float _updateInterval = 0.1f; // Обновляем поведение только каждые 0.1 секунды
        private float _updateTimer = 0f;
        private bool _isPlayerVisible = false;
        private float _distanceToPlayer = float.MaxValue;

        private void Update()
        {
            if (!_stateController.IsActive) return;
            
            _updateTimer += Time.deltaTime;
            if (_updateTimer >= _updateInterval)
            {
                _updateTimer = 0f;
                CheckPlayerVisibility();
            }
            
            // Вращение к игроку и стрельба только если игрок виден
            if (_isPlayerVisible)
            {
                UpdateAim();
                UpdateShooting();
            }
        }

        private void CheckPlayerVisibility()
        {
            if (_player == null) return;
            
            _distanceToPlayer = Vector3.Distance(transform.position, _player.position);
            
            // Если игрок слишком далеко, нет смысла проверять видимость
            if (_distanceToPlayer > 40f)
            {
                _isPlayerVisible = false;
                return;
            }
            
            // Проверяем только если игрок не слишком далеко
            Vector3 directionToPlayer = (_player.position - transform.position).normalized;
            _isPlayerVisible = !Physics.Raycast(transform.position, directionToPlayer, _distanceToPlayer, 
                LayerMask.GetMask("Ground", "Default"));
        }

        private void UpdateAim()
        {
            Vector3 targetPosition = _player.position + Vector3.up * _aimHeight;
            Vector3 lookDirection = targetPosition - transform.position;
            lookDirection.y = 0;

            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
                _shootPoint.rotation = Quaternion.LookRotation(
                    (targetPosition - _shootPoint.position).normalized);
            }
        }

        private void UpdateShooting()
        {
            if (_distanceToPlayer > 40f) return;

            _shootTimer += Time.deltaTime;
            
            if (_shootTimer >= _shootInterval)
            {
                _shootTimer = 0f;
                Shoot();
            }
        }
        #endregion

        #region Shoting
        private void Shoot()
        {
            GameObject arrow = _poolManager.GetEnemyProjectile();
            if (arrow == null) return;

            SetupArrow(arrow);
            StartCoroutine(ReturnArrowAfterDelay(arrow));
        }

        private void SetupArrow(GameObject arrow)
        {
            arrow.transform.SetPositionAndRotation(
                _shootPoint.position,
                Quaternion.LookRotation(GetShootDirection())
            );

            if (arrow.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.linearVelocity = GetShootDirection() * _arrowSpeed;
            }
        }

        private Vector3 GetShootDirection()
        {
            return (_player.position + Vector3.up * _aimHeight - _shootPoint.position).normalized;
        }

        private IEnumerator ReturnArrowAfterDelay(GameObject arrow)
        {
            yield return new WaitForSeconds(5f);
            _poolManager.ReturnEnemyProjectile(arrow);
        }
        #endregion

        #region Обработка событий
        private void HandleDeath()
        {
            StopAllCoroutines();
            _poolManager.ReturnEnemy(gameObject);
        }

        private void ResetEnemy()
        {
            _shootTimer = Random.Range(0f, _shootInterval * 0.5f);
        }
        #endregion

        private void OnDrawGizmosSelected()
        {
            if (_shootPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(_shootPoint.position, 0.1f);
                Gizmos.DrawRay(_shootPoint.position, _shootPoint.forward * 2f);
            }
        }
    }
}