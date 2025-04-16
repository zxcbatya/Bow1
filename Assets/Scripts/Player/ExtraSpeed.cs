using UnityEngine;

namespace Player
{
    public class ExtraSpeed : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Transform orientation;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Block"))
            {
                GameObject player = GameObject.Find("Player").gameObject;
                PlayerController playerMovement = player.GetComponent<PlayerController>();

                if (playerMovement.isMoving && !playerMovement.isCrouching)
                {
                    Rigidbody rb = player.GetComponent<Rigidbody>();
                    rb.AddForce(orientation.forward.normalized * 80f, ForceMode.Acceleration);
                }
            }
        }
    }
}