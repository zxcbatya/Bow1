using UnityEngine;
using ObjectPool = World.ObjectPool;

namespace Player
{
    public class PlayerShoting : MonoBehaviour
    {
        [Header("Настройки стрельбы")] [SerializeField]
        private GameObject arrowPrefab;

        [SerializeField] private Transform shootPoint;
        [SerializeField] private float shootForce = 20f;
        [SerializeField] private float shootCooldown = 0.5f;

        private float _nextShootTime;
        private ObjectPool _arrowPool;

        private void Start()
        {
            if (shootPoint == null)
            {
                Debug.LogError($"ShootPoint не назначен в {gameObject.name}!");
                enabled = false;
                return;
            }

            _arrowPool = new ObjectPool(arrowPrefab, 10);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && Time.time >= _nextShootTime)
            {
                Shoot();
            }
        }

        private void Shoot()
        {
            GameObject arrow = _arrowPool.GetObject();
            if (arrow != null)
            {
                arrow.transform.position = shootPoint.position;
                arrow.transform.rotation = shootPoint.rotation;

                Rigidbody rb = arrow.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = shootPoint.forward * shootForce;
                }

                _nextShootTime = Time.time + shootCooldown;
            }
        }
    }
}