using UnityEngine;

namespace Player
{
    public class MoveCamera : MonoBehaviour
    {
        [SerializeField] Transform cameraPosition = null;
        
        void Update()
        {
            transform.position = cameraPosition.position;
            transform.rotation = cameraPosition.rotation;
        }
    }
}