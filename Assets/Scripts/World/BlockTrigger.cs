using System;
using UnityEngine;

namespace World
{
    public class BlockTrigger : MonoBehaviour
    {
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private float activationHeight = 0.5f; // Высота над блоком для активации
        [SerializeField] private bool showDebugGizmos = true;
        
        private bool _wasActivated = false;
        private int _row = 0; // Номер ряда, к которому принадлежит блок
        
        public static event Action<BlockTrigger> OnBlockPassed;
        
        private void OnEnable()
        {
            _wasActivated = false;
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.CompareTag(playerTag)) return;
            CheckIfPassed(collision.transform);
        }
        
        private void OnCollisionStay(Collision collision)
        {
            if (!_wasActivated && collision.gameObject.CompareTag(playerTag))
            {
                CheckIfPassed(collision.transform);
            }
        }
        
        private void CheckIfPassed(Transform playerTransform)
        {
            float playerY = playerTransform.position.y;
            float blockY = transform.position.y;
            float requiredY = blockY + activationHeight;
            
            
            if (playerY > requiredY)
            {
                if (!_wasActivated)
                {
                    _wasActivated = true;
                    OnBlockPassed?.Invoke(this);
                }
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;
            
            // Рисуем линию активации
            Gizmos.color = Color.yellow;
            Vector3 activationPoint = transform.position + Vector3.up * activationHeight;
            Gizmos.DrawLine(transform.position, activationPoint);
            Gizmos.DrawSphere(activationPoint, 0.1f);
        }
        
        public void SetRow(int row)
        {
            _row = row;
        }
        
        public int GetRow() => _row;
        public float GetHeight() => transform.position.y;
    }
} 