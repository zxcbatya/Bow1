using UnityEngine;
using System.Collections;
using Utils;

namespace Enemies
{
    public class EnemySkeleton : MonoBehaviour
    {
        [Header("Настройки стрельбы")]
        [SerializeField] private float shootInterval = 2f;
        [SerializeField] private float arrowSpeed = 10f;
        [SerializeField] private float aimHeight = 1.5f;
        
        [Header("Ссылки")]
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private Transform shootPoint;
        
        private Transform _player;
        private float _shootTimer;
        private ObjectPool _arrowPool;
        private bool _isInitialized;
        
        private void Awake()
        {
            if (shootPoint == null)
            {
                shootPoint = transform.Find("ShootPoint");
                if (shootPoint == null)
                {
                    shootPoint = new GameObject("ShootPoint").transform;
                    shootPoint.SetParent(transform);
                    shootPoint.localPosition = new Vector3(0, -1.21f, 0.59f);
                }
            }
            
            if (arrowPrefab != null)
            {
                _arrowPool = new ObjectPool(arrowPrefab, 5);
            }

            
            if (!gameObject.CompareTag("Enemy"))
            {
                gameObject.tag = "Enemy";
            }
        }
        
        public void Initialize(Transform player)
        {
            if (player == null)
            {
                return;
            }
            
            _player = player;
            _isInitialized = true;
            _shootTimer = Random.Range(0f, shootInterval * 0.5f); 
        }
        
        private void OnEnable()
        {
            _shootTimer = Random.Range(0f, shootInterval * 0.5f);
        }
        
        private void Update()
        {
            if (!_isInitialized)
            {
                return;
            }
            
            if (_player == null)
            {
                return;
            }
            
            float distanceToPlayer = Vector3.Distance(transform.position, _player.position);
            
            if (distanceToPlayer <= 40f)
            {
                Vector3 targetPosition = _player.position + Vector3.up * aimHeight;
                Vector3 lookDirection = targetPosition - transform.position;
                
                lookDirection.y = 0;
                
                if (lookDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    Quaternion newRotation = Quaternion.Euler(
                        transform.rotation.eulerAngles.x,
                        targetRotation.eulerAngles.y,
                        transform.rotation.eulerAngles.z);
                    transform.rotation = newRotation;
                    
                    Vector3 preciseDirection = (targetPosition - shootPoint.position).normalized;
                    shootPoint.rotation = Quaternion.LookRotation(preciseDirection);
                }
                
                _shootTimer += Time.deltaTime;
                
                if (_shootTimer >= shootInterval)
                {
                    _shootTimer = 0f;
                    Shoot();
                }
            }
        }
        
        private void Shoot()
        {
            if (!_isInitialized || _player == null)
            {
                return;
            }
            
            if (_arrowPool == null || arrowPrefab == null)
            {
                return;
            }
            
            Vector3 targetPosition = _player.position + Vector3.up * aimHeight;
            Vector3 direction = (targetPosition - shootPoint.position).normalized;
            
            GameObject arrow = _arrowPool.GetObject();
            if (arrow == null)
            {
                Debug.LogError($"[{gameObject.name}] Failed to get arrow from pool!");
                return;
            }
            
            arrow.transform.position = shootPoint.position;
            arrow.transform.rotation = Quaternion.LookRotation(direction);
            
            Rigidbody rb = arrow.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.linearVelocity = direction * arrowSpeed;
                
                arrow.transform.parent = null;
                
                if (!arrow.activeSelf)
                {
                    arrow.SetActive(true);
                }
                
                
                StartCoroutine(ReturnArrowToPoolDelayed(arrow));
            }
        }
        
        private IEnumerator ReturnArrowToPoolDelayed(GameObject arrow)
        {
            yield return new WaitForSeconds(5f);
            
            if (arrow != null && arrow.activeSelf && _arrowPool != null)
            {
                _arrowPool.ReturnObject(arrow);
            }
        }
        
        public void TakeDamage()
        {
            if (_arrowPool != null)
            {
                _arrowPool.ReturnAllObjects();
            }
            
            gameObject.SetActive(false);
        }
        
        private void OnDisable()
        {
            StopAllCoroutines();
        }
        
        private void OnDrawGizmos()
        {
            if (shootPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(shootPoint.position, 0.1f);
                Gizmos.DrawRay(shootPoint.position, shootPoint.forward * 2f);
            }
        }
    }
} 